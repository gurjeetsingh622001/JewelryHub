using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Auth.Common;
using JewelryHub.Domain.Customers;
using JewelryHub.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Auth.Commands.RegisterCustomer;

public record RegisterCustomerCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber) : IRequest<AuthResponse>;

public class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
    }
}

public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, AuthResponse>
{
    private const string CustomerRoleName = "CUSTOMER";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;

    public RegisterCustomerCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await _unitOfWork.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (emailTaken)
        {
            throw new BusinessRuleException("An account with this email already exists.", nameof(request.Email));
        }

        var customerRole = await _unitOfWork.Roles.Query()
            .FirstOrDefaultAsync(r => r.NormalizedName == CustomerRoleName, cancellationToken)
            ?? throw new InvalidOperationException("The 'Customer' role is not seeded. Run database seeding before accepting registrations.");

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            SecurityStamp = Guid.NewGuid().ToString("N"),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
        };
        user.UserRoles.Add(new UserRole { RoleId = customerRole.Id, User = user });

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.Customers.AddAsync(new Customer { UserId = user.Id, User = user }, cancellationToken);

        var tokens = _tokenService.GenerateTokenPair(user.Id, user.Email, new[] { "Customer" });
        await _unitOfWork.RefreshTokens.AddAsync(new Domain.Identity.RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(tokens.RefreshToken),
            ExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc,
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, new[] { "Customer" },
            tokens.AccessToken, tokens.AccessTokenExpiresAtUtc, tokens.RefreshToken, tokens.RefreshTokenExpiresAtUtc);
    }
}

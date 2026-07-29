using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Auth.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Identity;
using JewelryHub.Domain.Sellers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Auth.Commands.RegisterSeller;

/// <summary>
/// Creates the User + Seller profile, but the seller starts in
/// PendingApproval and cannot list products until an admin verifies their
/// KYC documents (submitted separately) — this command only opens the
/// account, it does not grant selling rights.
/// </summary>
public record RegisterSellerCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string BusinessName,
    string GstNumber,
    string BusinessRegistrationNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode) : IRequest<AuthResponse>;

public class RegisterSellerCommandValidator : AbstractValidator<RegisterSellerCommand>
{
    public RegisterSellerCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.GstNumber).NotEmpty().Length(15).Matches("^[0-9A-Z]{15}$").WithMessage("GST number must be a valid 15-character GSTIN.");
        RuleFor(x => x.BusinessRegistrationNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(250);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
    }
}

public class RegisterSellerCommandHandler : IRequestHandler<RegisterSellerCommand, AuthResponse>
{
    private const string SellerRoleName = "SELLER";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;

    public RegisterSellerCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RegisterSellerCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _unitOfWork.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken))
        {
            throw new BusinessRuleException("An account with this email already exists.", nameof(request.Email));
        }

        if (await _unitOfWork.Sellers.AnyAsync(s => s.GstNumber == request.GstNumber, cancellationToken))
        {
            throw new BusinessRuleException("A seller is already registered with this GST number.", nameof(request.GstNumber));
        }

        var sellerRole = await _unitOfWork.Roles.Query()
            .FirstOrDefaultAsync(r => r.NormalizedName == SellerRoleName, cancellationToken)
            ?? throw new InvalidOperationException("The 'Seller' role is not seeded. Run database seeding before accepting registrations.");

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            SecurityStamp = Guid.NewGuid().ToString("N"),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
        };
        user.UserRoles.Add(new UserRole { RoleId = sellerRole.Id, User = user });

        var seller = new Seller
        {
            UserId = user.Id,
            User = user,
            BusinessName = request.BusinessName.Trim(),
            GstNumber = request.GstNumber.Trim().ToUpperInvariant(),
            BusinessRegistrationNumber = request.BusinessRegistrationNumber.Trim(),
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = request.AddressLine2?.Trim(),
            City = request.City.Trim(),
            State = request.State.Trim(),
            PostalCode = request.PostalCode.Trim(),
            VerificationStatus = SellerVerificationStatus.PendingApproval,
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.Sellers.AddAsync(seller, cancellationToken);

        var tokens = _tokenService.GenerateTokenPair(user.Id, user.Email, new[] { "Seller" });
        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(tokens.RefreshToken),
            ExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc,
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, new[] { "Seller" },
            tokens.AccessToken, tokens.AccessTokenExpiresAtUtc, tokens.RefreshToken, tokens.RefreshTokenExpiresAtUtc);
    }
}

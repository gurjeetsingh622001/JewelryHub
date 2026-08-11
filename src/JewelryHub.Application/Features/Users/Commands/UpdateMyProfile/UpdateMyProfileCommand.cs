using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Users.Common;
using JewelryHub.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Users.Commands.UpdateMyProfile;

public record UpdateMyProfileCommand(string FirstName, string LastName, string? PhoneNumber, string? PhotoUrl) : IRequest<MyProfileDto>;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
    }
}

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, MyProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateMyProfileCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<MyProfileDto> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.QueryTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), _currentUser.UserId!.Value);

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        // The photo field lives on whichever role-specific entity owns it —
        // Admin has neither, so PhotoUrl is silently a no-op for that role.
        string? photoUrl = null;
        if (roles.Contains("Customer"))
        {
            var customer = await _unitOfWork.Customers.QueryTracking().FirstOrDefaultAsync(c => c.UserId == user.Id, cancellationToken);
            if (customer is not null)
            {
                customer.ProfileImageUrl = request.PhotoUrl;
                photoUrl = customer.ProfileImageUrl;
            }
        }
        else if (roles.Contains("Seller"))
        {
            var seller = await _unitOfWork.Sellers.QueryTracking().FirstOrDefaultAsync(s => s.UserId == user.Id, cancellationToken);
            if (seller is not null)
            {
                seller.LogoUrl = request.PhotoUrl;
                photoUrl = seller.LogoUrl;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MyProfileDto(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber, photoUrl, roles);
    }
}

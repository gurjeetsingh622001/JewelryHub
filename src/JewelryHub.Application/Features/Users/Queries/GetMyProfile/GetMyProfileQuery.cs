using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Users.Common;
using JewelryHub.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Users.Queries.GetMyProfile;

public record GetMyProfileQuery : IRequest<MyProfileDto>;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, MyProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<MyProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), _currentUser.UserId!.Value);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        // Photo lives on whichever role-specific entity owns it — see MyProfileDto's remarks.
        string? photoUrl = null;
        if (roles.Contains("Customer"))
        {
            var customer = await _unitOfWork.Customers.Query().FirstOrDefaultAsync(c => c.UserId == user.Id, cancellationToken);
            photoUrl = customer?.ProfileImageUrl;
        }
        else if (roles.Contains("Seller"))
        {
            var seller = await _unitOfWork.Sellers.Query().FirstOrDefaultAsync(s => s.UserId == user.Id, cancellationToken);
            photoUrl = seller?.LogoUrl;
        }

        return new MyProfileDto(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber, photoUrl, roles);
    }
}

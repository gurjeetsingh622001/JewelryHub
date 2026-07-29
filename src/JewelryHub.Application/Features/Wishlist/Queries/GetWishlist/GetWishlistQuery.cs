using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Wishlist.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Wishlist.Queries.GetWishlist;

public record GetWishlistQuery : IRequest<WishlistDto>;

public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, WishlistDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetWishlistQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<WishlistDto> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        var wishlist = await _unitOfWork.Wishlists.Query()
            .Include(w => w.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(w => w.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Inventory)
            .FirstOrDefaultAsync(w => w.Customer.UserId == _currentUser.UserId, cancellationToken);

        if (wishlist is null)
        {
            return new WishlistDto(Guid.Empty, Array.Empty<WishlistItemDto>());
        }

        return WishlistMapper.ToDto(wishlist);
    }
}

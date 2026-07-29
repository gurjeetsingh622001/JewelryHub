using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Wishlist.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Wishlist.Commands.AddToWishlist;

public record AddToWishlistCommand(Guid ProductId) : IRequest<WishlistDto>;

public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, WishlistDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AddToWishlistCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<WishlistDto> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        var productExists = await _unitOfWork.Products.AnyAsync(p => p.Id == request.ProductId, cancellationToken);
        if (!productExists)
        {
            throw new NotFoundException("Product", request.ProductId);
        }

        var customer = await _unitOfWork.Customers.Query()
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new BusinessRuleException("No customer profile is associated with this account.");

        var wishlist = await _unitOfWork.Wishlists.QueryTracking()
            .Include(w => w.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(w => w.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Inventory)
            .FirstOrDefaultAsync(w => w.CustomerId == customer.Id, cancellationToken);

        if (wishlist is null)
        {
            wishlist = new Domain.Wishlist.Wishlist { CustomerId = customer.Id };
            await _unitOfWork.Wishlists.AddAsync(wishlist, cancellationToken);
        }

        // Adding an already-wishlisted product is a no-op, not a duplicate/error.
        if (wishlist.Items.All(i => i.ProductId != request.ProductId))
        {
            wishlist.Items.Add(new Domain.Wishlist.WishlistItem { WishlistId = wishlist.Id, ProductId = request.ProductId });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-fetch tracked-but-fresh so the newly added item's Product nav is populated for mapping.
        var reloaded = await _unitOfWork.Wishlists.Query()
            .Include(w => w.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(w => w.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Inventory)
            .FirstAsync(w => w.Id == wishlist.Id, cancellationToken);

        return WishlistMapper.ToDto(reloaded);
    }
}

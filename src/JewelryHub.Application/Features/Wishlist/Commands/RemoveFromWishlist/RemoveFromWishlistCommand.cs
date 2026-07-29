using JewelryHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Wishlist.Commands.RemoveFromWishlist;

public record RemoveFromWishlistCommand(Guid ProductId) : IRequest;

public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RemoveFromWishlistCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        var wishlist = await _unitOfWork.Wishlists.QueryTracking()
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.Customer.UserId == _currentUser.UserId, cancellationToken);

        var item = wishlist?.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        // Removing something already absent is a no-op, not an error.
        if (item is null) return;

        wishlist!.Items.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

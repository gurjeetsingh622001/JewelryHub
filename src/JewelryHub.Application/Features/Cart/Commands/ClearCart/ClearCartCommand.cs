using JewelryHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Cart.Commands.ClearCart;

public record ClearCartCommand : IRequest;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ClearCartCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.Carts.QueryTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Customer.UserId == _currentUser.UserId, cancellationToken);

        // Nothing to clear is a no-op, not an error.
        if (cart is null) return;

        cart.Items.Clear();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

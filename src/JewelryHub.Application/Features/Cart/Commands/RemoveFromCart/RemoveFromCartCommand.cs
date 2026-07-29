using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Cart.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Cart.Commands.RemoveFromCart;

public record RemoveFromCartCommand(Guid ProductId) : IRequest<CartDto>;

public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, CartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RemoveFromCartCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<CartDto> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.Carts.QueryTracking()
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Inventory)
            .FirstOrDefaultAsync(c => c.Customer.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new BusinessRuleException("Cart is empty.");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId)
            ?? throw new NotFoundException("CartItem", request.ProductId);

        cart.Items.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CartMapper.ToDto(cart);
    }
}

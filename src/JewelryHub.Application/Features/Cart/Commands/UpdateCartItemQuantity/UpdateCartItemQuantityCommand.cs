using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Cart.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Cart.Commands.UpdateCartItemQuantity;

public record UpdateCartItemQuantityCommand(Guid ProductId, int Quantity) : IRequest<CartDto>;

public class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
    }
}

public class UpdateCartItemQuantityCommandHandler : IRequestHandler<UpdateCartItemQuantityCommand, CartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateCartItemQuantityCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<CartDto> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.Carts.QueryTracking()
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Inventory)
            .FirstOrDefaultAsync(c => c.Customer.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new BusinessRuleException("Cart is empty.");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId)
            ?? throw new NotFoundException("CartItem", request.ProductId);

        if (request.Quantity == 0)
        {
            cart.Items.Remove(item);
            _unitOfWork.Carts.Update(cart);
        }
        else
        {
            if (item.Product.Inventory is { TrackInventory: true } inv
                && request.Quantity > inv.QuantityAvailable - inv.QuantityReserved)
            {
                throw new BusinessRuleException($"Only {inv.QuantityAvailable - inv.QuantityReserved} unit(s) of this product are available.");
            }

            item.Quantity = request.Quantity;
            _unitOfWork.Carts.Update(cart);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CartMapper.ToDto(cart);
    }
}

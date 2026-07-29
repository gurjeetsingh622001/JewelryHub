using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Cart.Common;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Cart.Commands.AddToCart;

public record AddToCartCommand(Guid ProductId, int Quantity) : IRequest<CartDto>;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AddToCartCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<CartDto> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.Query()
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (product.Status is not (ProductStatus.Active or ProductStatus.OutOfStock))
        {
            throw new BusinessRuleException("This product is not currently available for purchase.");
        }

        var customer = await _unitOfWork.Customers.Query()
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new BusinessRuleException("No customer profile is associated with this account.");

        var cart = await _unitOfWork.Carts.QueryTracking()
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Inventory)
            .FirstOrDefaultAsync(c => c.CustomerId == customer.Id, cancellationToken);

        if (cart is null)
        {
            cart = new Domain.Cart.Cart { CustomerId = customer.Id };
            await _unitOfWork.Carts.AddAsync(cart, cancellationToken);
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        var requestedTotalQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;

        if (product.Inventory is { TrackInventory: true } inv && requestedTotalQuantity > inv.QuantityAvailable - inv.QuantityReserved)
        {
            throw new BusinessRuleException($"Only {inv.QuantityAvailable - inv.QuantityReserved} unit(s) of this product are available.");
        }

        if (existingItem is not null)
        {
            existingItem.Quantity = requestedTotalQuantity;
            existingItem.UnitPriceSnapshot = product.BasePrice; // refresh snapshot to the current price on every add
        }
        else
        {
            cart.Items.Add(new Domain.Cart.CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Product = product,
                Quantity = request.Quantity,
                UnitPriceSnapshot = product.BasePrice,
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CartMapper.ToDto(cart);
    }
}

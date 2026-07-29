using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Orders.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(Guid ShippingAddressId, Guid BillingAddressId, PaymentMethod PaymentMethod, string? CustomerNote) : IRequest<OrderDto>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.ShippingAddressId).NotEmpty();
        RuleFor(x => x.BillingAddressId).NotEmpty();
        RuleFor(x => x.CustomerNote).MaximumLength(1000);
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ITaxCalculator _taxCalculator;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ITaxCalculator taxCalculator)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _taxCalculator = taxCalculator;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.Query()
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new BusinessRuleException("No customer profile is associated with this account.");

        var shippingAddress = customer.Addresses.FirstOrDefault(a => a.Id == request.ShippingAddressId)
            ?? throw new NotFoundException("CustomerAddress", request.ShippingAddressId);
        var billingAddress = customer.Addresses.FirstOrDefault(a => a.Id == request.BillingAddressId)
            ?? throw new NotFoundException("CustomerAddress", request.BillingAddressId);

        var cart = await _unitOfWork.Carts.QueryTracking()
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Seller)
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Inventory)
            .FirstOrDefaultAsync(c => c.CustomerId == customer.Id, cancellationToken);

        if (cart is null || cart.Items.Count == 0)
        {
            throw new BusinessRuleException("Your cart is empty.");
        }

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customer.Id,
            ShippingAddressId = shippingAddress.Id,
            BillingAddressId = billingAddress.Id,
            CustomerNote = request.CustomerNote,
            Status = OrderStatus.PendingPayment,
        };

        decimal subtotal = 0, totalTax = 0;

        foreach (var cartItem in cart.Items)
        {
            var product = cartItem.Product;
            var inventory = product.Inventory;

            if (product.Status is not (ProductStatus.Active or ProductStatus.OutOfStock))
            {
                throw new BusinessRuleException($"'{product.Name}' is no longer available and was not ordered.");
            }

            if (inventory is { TrackInventory: true } && inventory.QuantityAvailable - inventory.QuantityReserved < cartItem.Quantity)
            {
                throw new BusinessRuleException($"Only {inventory.QuantityAvailable - inventory.QuantityReserved} unit(s) of '{product.Name}' are available.");
            }

            // Priced at the current, authoritative product price — never
            // trust the cart's snapshot for the actual charge, since gold
            // rates (and therefore BasePrice) can move between add-to-cart
            // and checkout.
            var unitPrice = product.BasePrice;
            var lineTotal = unitPrice * cartItem.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                SellerId = product.SellerId,
                Seller = product.Seller,
                ProductNameSnapshot = product.Name,
                MetalRateSnapshot = product.MetalRatePerGramAtListing,
                UnitPriceSnapshot = unitPrice,
                Quantity = cartItem.Quantity,
                LineTotal = lineTotal,
                ItemStatus = OrderStatus.PendingPayment,
            };

            var taxLines = await _taxCalculator.CalculateAsync(
                product.CategoryId, product.MetalType, product.Seller.State, shippingAddress.State, lineTotal, cancellationToken);

            foreach (var tax in taxLines)
            {
                orderItem.Taxes.Add(new OrderTax
                {
                    TaxRateId = tax.TaxRateId,
                    ComponentType = tax.ComponentType,
                    RatePercentageApplied = tax.RatePercentageApplied,
                    TaxableAmount = tax.TaxableAmount,
                    TaxAmount = tax.TaxAmount,
                });
            }

            order.Items.Add(orderItem);
            subtotal += lineTotal;
            totalTax += taxLines.Sum(t => t.TaxAmount);

            // Reserve, don't decrement yet — the stock is only truly
            // consumed once payment is confirmed (see ConfirmPayment).
            // This keeps an abandoned/failed payment from permanently
            // losing the seller's stock.
            if (inventory is { TrackInventory: true })
            {
                inventory.QuantityReserved += cartItem.Quantity;
            }
        }

        order.Subtotal = subtotal;
        order.TotalTax = totalTax;
        order.ShippingCharges = 0; // TODO: shipping rate calculation is a future slice
        order.DiscountAmount = 0;
        order.GrandTotal = subtotal + totalTax + order.ShippingCharges - order.DiscountAmount;

        order.Payments.Add(new Domain.Payments.Payment
        {
            Method = request.PaymentMethod,
            Status = PaymentStatus.Pending,
            Amount = order.GrandTotal,
        });

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        cart.Items.Clear();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        order.Customer = customer;
        order.ShippingAddress = shippingAddress;
        order.BillingAddress = billingAddress;

        return OrderMapper.ToDto(order);
    }

    private static string GenerateOrderNumber() =>
        $"JH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}

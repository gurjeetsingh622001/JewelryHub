using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Services;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Orders;
using JewelryHub.Domain.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Orders.Commands.ConfirmPayment;

/// <summary>
/// Stands in for a real payment gateway webhook (Razorpay/Stripe) — in
/// production this would be called from an Infrastructure webhook
/// endpoint after verifying the gateway's signature, not directly by the
/// client. Kept as an explicit command for now so checkout is testable
/// end-to-end before a real gateway integration exists.
/// </summary>
public record ConfirmPaymentCommand(Guid OrderId, Guid PaymentId, string GatewayTransactionId) : IRequest;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public ConfirmPaymentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.QueryTracking()
            .Include(o => o.Customer)
            .Include(o => o.Payments)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Inventory)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (order.Customer.UserId != _currentUser.UserId && !_currentUser.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("You can only confirm payment on your own orders.");
        }

        var payment = order.Payments.FirstOrDefault(p => p.Id == request.PaymentId)
            ?? throw new NotFoundException(nameof(Payment), request.PaymentId);

        if (payment.Status == PaymentStatus.Completed)
        {
            return; // already confirmed — idempotent on webhook retry
        }

        if (order.Status != OrderStatus.PendingPayment)
        {
            throw new BusinessRuleException("This order is not awaiting payment.");
        }

        payment.Status = PaymentStatus.Completed;
        payment.GatewayTransactionId = request.GatewayTransactionId;
        payment.PaidAtUtc = DateTime.UtcNow;

        order.Status = OrderStatus.Confirmed;
        order.ConfirmedAtUtc = DateTime.UtcNow;

        foreach (var item in order.Items)
        {
            item.ItemStatus = OrderStatus.Confirmed;

            // Turn the checkout-time reservation into an actual deduction now that money has moved.
            if (item.Product.Inventory is { TrackInventory: true } inventory)
            {
                inventory.QuantityReserved = Math.Max(0, inventory.QuantityReserved - item.Quantity);
                inventory.QuantityAvailable = Math.Max(0, inventory.QuantityAvailable - item.Quantity);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyAsync(
            order.Customer.UserId, "Order", "Order confirmed",
            $"Your order {order.OrderNumber} is confirmed and being prepared.",
            linkUrl: $"/orders/{order.Id}", cancellationToken: cancellationToken);
    }
}

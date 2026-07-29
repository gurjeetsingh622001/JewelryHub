using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Orders.Commands.CancelOrder;

public record CancelOrderCommand(Guid OrderId, string? Reason) : IRequest;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private static readonly OrderStatus[] CancellableStatuses = { OrderStatus.PendingPayment, OrderStatus.Confirmed, OrderStatus.Processing };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CancelOrderCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.QueryTracking()
            .Include(o => o.Customer)
            .Include(o => o.Payments)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Inventory)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (order.Customer.UserId != _currentUser.UserId && !_currentUser.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("You can only cancel your own orders.");
        }

        if (!CancellableStatuses.Contains(order.Status))
        {
            throw new BusinessRuleException($"An order in '{order.Status}' status can no longer be cancelled.");
        }

        var wasPaid = order.Payments.Any(p => p.Status == PaymentStatus.Completed);

        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = request.Reason;

        foreach (var item in order.Items)
        {
            item.ItemStatus = OrderStatus.Cancelled;

            if (item.Product.Inventory is { TrackInventory: true } inventory)
            {
                if (wasPaid)
                {
                    // Stock was already deducted at payment confirmation — give it back.
                    inventory.QuantityAvailable += item.Quantity;
                }
                else
                {
                    // Still only reserved — release the hold.
                    inventory.QuantityReserved = Math.Max(0, inventory.QuantityReserved - item.Quantity);
                }
            }
        }

        // A paid order being cancelled needs an actual refund — flagging
        // the payment rather than auto-completing it, since real refunds
        // go back through the gateway (Infrastructure), not just a DB flip.
        if (wasPaid)
        {
            foreach (var payment in order.Payments.Where(p => p.Status == PaymentStatus.Completed))
            {
                payment.Status = PaymentStatus.Refunded;
                payment.RefundedAmount = payment.Amount;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

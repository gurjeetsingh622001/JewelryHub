using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Services;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Orders.Commands.UpdateOrderItemStatus;

public record UpdateOrderItemStatusCommand(
    Guid OrderItemId,
    OrderStatus NewStatus,
    string? Carrier,
    string? TrackingNumber,
    string? TrackingUrl) : IRequest;

public class UpdateOrderItemStatusCommandValidator : AbstractValidator<UpdateOrderItemStatusCommand>
{
    private static readonly OrderStatus[] SellerSettableStatuses = { OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered };

    public UpdateOrderItemStatusCommandValidator()
    {
        RuleFor(x => x.NewStatus).Must(s => SellerSettableStatuses.Contains(s))
            .WithMessage("Sellers can only move an item to Processing, Shipped, or Delivered.");
        RuleFor(x => x.Carrier).NotEmpty().When(x => x.NewStatus == OrderStatus.Shipped)
            .WithMessage("Carrier is required when marking an item Shipped.");
        RuleFor(x => x.TrackingNumber).NotEmpty().When(x => x.NewStatus == OrderStatus.Shipped)
            .WithMessage("A tracking number is required when marking an item Shipped.");
    }
}

public class UpdateOrderItemStatusCommandHandler : IRequestHandler<UpdateOrderItemStatusCommand>
{
    // Fulfillment can only move forward — a shipped item can't silently go back to Processing.
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Confirmed] = new[] { OrderStatus.Processing },
        [OrderStatus.Processing] = new[] { OrderStatus.Shipped },
        [OrderStatus.Shipped] = new[] { OrderStatus.Delivered },
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public UpdateOrderItemStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task Handle(UpdateOrderItemStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.QueryTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Seller)
            .Include(o => o.Items).ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(o => o.Items.Any(i => i.Id == request.OrderItemId), cancellationToken)
            ?? throw new NotFoundException(nameof(OrderItem), request.OrderItemId);

        var item = order.Items.First(i => i.Id == request.OrderItemId);

        if (item.Seller.UserId != _currentUser.UserId && !_currentUser.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("You can only update your own order items.");
        }

        if (!AllowedTransitions.TryGetValue(item.ItemStatus, out var allowedNext) || !allowedNext.Contains(request.NewStatus))
        {
            throw new BusinessRuleException($"Cannot move an item from '{item.ItemStatus}' to '{request.NewStatus}'.");
        }

        item.ItemStatus = request.NewStatus;

        if (request.NewStatus == OrderStatus.Shipped)
        {
            if (item.Shipment is null)
            {
                // Assigning a brand-new entity to a navigation on an already-tracked
                // parent isn't reliably picked up as an Added entity by EF Core's
                // change tracker when the entity's Guid key is client-generated
                // (BaseEntity sets Id in its property initializer) — it can be
                // mistaken for an existing, unchanged-key entity and issued as an
                // UPDATE instead of an INSERT (0 rows affected). Routing it through
                // the repository's AddAsync explicitly marks it Added.
                item.Shipment = new Shipment { OrderItemId = item.Id };
                await _unitOfWork.Shipments.AddAsync(item.Shipment, cancellationToken);
            }
            item.Shipment.Status = ShipmentStatus.Shipped;
            item.Shipment.Carrier = request.Carrier;
            item.Shipment.TrackingNumber = request.TrackingNumber;
            item.Shipment.TrackingUrl = request.TrackingUrl;
            item.Shipment.ShippedAtUtc = DateTime.UtcNow;
        }
        else if (request.NewStatus == OrderStatus.Delivered && item.Shipment is not null)
        {
            item.Shipment.Status = ShipmentStatus.Delivered;
            item.Shipment.DeliveredAtUtc = DateTime.UtcNow;
        }

        var orderNewlyCompleted = false;
        if (request.NewStatus == OrderStatus.Delivered)
        {
            // A seller's own rollup updates the moment their line is
            // delivered — it doesn't wait on other sellers' items in the
            // same order, since each seller fulfills independently.
            item.Seller.TotalOrdersFulfilled += 1;
            item.Seller.TotalRevenue += item.LineTotal;

            if (order.Items.All(i => i.ItemStatus == OrderStatus.Delivered))
            {
                order.Status = OrderStatus.Delivered;
                order.DeliveredAtUtc = DateTime.UtcNow;
                orderNewlyCompleted = true;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.NewStatus is OrderStatus.Shipped or OrderStatus.Delivered)
        {
            var verb = request.NewStatus == OrderStatus.Shipped ? "shipped" : "delivered";
            await _notificationService.NotifyAsync(
                order.Customer.UserId, "Order", $"Item {verb}",
                $"'{item.ProductNameSnapshot}' from order {order.OrderNumber} has been {verb}.",
                linkUrl: $"/orders/{order.Id}", cancellationToken: cancellationToken);
        }

        if (orderNewlyCompleted)
        {
            await _notificationService.NotifyAsync(
                order.Customer.UserId, "Order", "Order delivered",
                $"Every item in order {order.OrderNumber} has been delivered.",
                linkUrl: $"/orders/{order.Id}", cancellationToken: cancellationToken);
        }
    }
}

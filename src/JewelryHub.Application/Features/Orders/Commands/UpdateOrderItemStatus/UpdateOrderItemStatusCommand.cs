using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
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

    public UpdateOrderItemStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateOrderItemStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.QueryTracking()
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
            item.Shipment ??= new Shipment { OrderItemId = item.Id };
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: once every item on the parent Order reaches Delivered, a
        // background/orchestration step should roll the Order itself to
        // Delivered and update Seller.TotalOrdersFulfilled/TotalRevenue —
        // left as a follow-up rather than computed inline here so this
        // single-item command doesn't need to re-load and lock the whole
        // order graph on every fulfillment update.
    }
}

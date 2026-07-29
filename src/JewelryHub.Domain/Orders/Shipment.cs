using JewelryHub.Domain.Common;
using JewelryHub.Domain.Enums;

namespace JewelryHub.Domain.Orders;

/// <summary>
/// Shipment for a single OrderItem. Kept per-item (not per-order) because
/// each seller fulfills and ships their own items independently, often via
/// different couriers and on different days.
/// </summary>
public class Shipment : AuditableEntity
{
    public Guid OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = default!;

    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }

    public DateTime? ShippedAtUtc { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
}

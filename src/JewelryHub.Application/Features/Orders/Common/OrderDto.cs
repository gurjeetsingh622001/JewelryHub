using JewelryHub.Domain.Enums;

namespace JewelryHub.Application.Features.Orders.Common;

public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    decimal GrandTotal,
    int ItemCount,
    DateTime CreatedAtUtc);

public record OrderDto(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    decimal Subtotal,
    decimal TotalTax,
    decimal ShippingCharges,
    decimal DiscountAmount,
    decimal GrandTotal,
    string ShippingAddressSummary,
    string? CustomerNote,
    DateTime? ConfirmedAtUtc,
    DateTime? DeliveredAtUtc,
    DateTime CreatedAtUtc,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<PaymentDto> Payments);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductNameSnapshot,
    Guid SellerId,
    string SellerBusinessName,
    decimal UnitPriceSnapshot,
    int Quantity,
    decimal LineTotal,
    decimal ItemTaxTotal,
    OrderStatus ItemStatus,
    IReadOnlyList<OrderTaxDto> Taxes,
    ShipmentDto? Shipment);

public record OrderTaxDto(TaxComponentType ComponentType, decimal RatePercentageApplied, decimal TaxableAmount, decimal TaxAmount);

public record ShipmentDto(ShipmentStatus Status, string? Carrier, string? TrackingNumber, string? TrackingUrl, DateTime? ShippedAtUtc, DateTime? DeliveredAtUtc);

public record PaymentDto(Guid Id, PaymentMethod Method, PaymentStatus Status, decimal Amount, DateTime? PaidAtUtc);

/// <summary>Returned by the seller order queue — one row per OrderItem the seller owns, with just enough order context to act on it.</summary>
public record SellerOrderItemDto(
    Guid OrderItemId,
    Guid OrderId,
    string OrderNumber,
    string ProductNameSnapshot,
    decimal UnitPriceSnapshot,
    int Quantity,
    decimal LineTotal,
    OrderStatus ItemStatus,
    DateTime OrderCreatedAtUtc,
    ShipmentDto? Shipment);

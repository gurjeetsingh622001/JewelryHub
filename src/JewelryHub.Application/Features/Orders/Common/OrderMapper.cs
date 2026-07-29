using JewelryHub.Domain.Orders;

namespace JewelryHub.Application.Features.Orders.Common;

/// <summary>Caller must have loaded Items.Product/Seller/Taxes/Shipment and Payments first.</summary>
public static class OrderMapper
{
    public static OrderSummaryDto ToSummaryDto(Order o) =>
        new(o.Id, o.OrderNumber, o.Status, o.GrandTotal, o.Items.Sum(i => i.Quantity), o.CreatedAtUtc);

    public static OrderDto ToDto(Order o) => new(
        o.Id, o.OrderNumber, o.Status, o.Subtotal, o.TotalTax, o.ShippingCharges, o.DiscountAmount, o.GrandTotal,
        $"{o.ShippingAddress.AddressLine1}, {o.ShippingAddress.City}, {o.ShippingAddress.State} {o.ShippingAddress.PostalCode}",
        o.CustomerNote, o.ConfirmedAtUtc, o.DeliveredAtUtc, o.CreatedAtUtc,
        o.Items.Select(ToItemDto).ToList(),
        o.Payments.Select(p => new PaymentDto(p.Id, p.Method, p.Status, p.Amount, p.PaidAtUtc)).ToList());

    public static OrderItemDto ToItemDto(OrderItem i) => new(
        i.Id, i.ProductId, i.ProductNameSnapshot, i.SellerId, i.Seller.BusinessName,
        i.UnitPriceSnapshot, i.Quantity, i.LineTotal, i.Taxes.Sum(t => t.TaxAmount), i.ItemStatus,
        i.Taxes.Select(t => new OrderTaxDto(t.ComponentType, t.RatePercentageApplied, t.TaxableAmount, t.TaxAmount)).ToList(),
        i.Shipment is null ? null : ToShipmentDto(i.Shipment));

    public static ShipmentDto ToShipmentDto(Shipment s) =>
        new(s.Status, s.Carrier, s.TrackingNumber, s.TrackingUrl, s.ShippedAtUtc, s.DeliveredAtUtc);

    public static SellerOrderItemDto ToSellerItemDto(OrderItem i) => new(
        i.Id, i.OrderId, i.Order.OrderNumber, i.ProductNameSnapshot, i.UnitPriceSnapshot, i.Quantity, i.LineTotal,
        i.ItemStatus, i.Order.CreatedAtUtc, i.Shipment is null ? null : ToShipmentDto(i.Shipment));
}

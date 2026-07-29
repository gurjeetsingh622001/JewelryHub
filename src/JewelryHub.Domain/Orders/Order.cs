using JewelryHub.Domain.Catalog;
using JewelryHub.Domain.Common;
using JewelryHub.Domain.Customers;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Payments;
using JewelryHub.Domain.Sellers;
using JewelryHub.Domain.Tax;

namespace JewelryHub.Domain.Orders;

/// <summary>
/// A confirmed purchase. Deliberately NOT split per-seller at the Order
/// level even though a cart can span multiple sellers — instead each
/// OrderItem carries its own SellerId, and an Order is the customer-facing
/// unit while sellers query their own OrderItems for fulfillment/revenue.
/// This avoids fragmenting a customer's checkout into several orders while
/// still letting each seller manage only their own line items.
/// </summary>
public class Order : AuditableEntity, ISoftDelete
{
    public string OrderNumber { get; set; } = default!; // human-readable, e.g. JH-2026-000123

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;

    public Guid ShippingAddressId { get; set; }
    public CustomerAddress ShippingAddress { get; set; } = default!;
    public Guid BillingAddressId { get; set; }
    public CustomerAddress BillingAddress { get; set; } = default!;

    public decimal Subtotal { get; set; }      // sum of item BasePrice * qty
    public decimal TotalTax { get; set; }
    public decimal ShippingCharges { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }

    public string? CustomerNote { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

/// <summary>One product line in an order. Carries a full price snapshot so historical invoices stay accurate even if the product is later re-priced.</summary>
public class OrderItem : AuditableEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public Guid SellerId { get; set; } // denormalized for fast per-seller order queries
    public Seller Seller { get; set; } = default!;

    public string ProductNameSnapshot { get; set; } = default!;
    public decimal MetalRateSnapshot { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    public OrderStatus ItemStatus { get; set; } = OrderStatus.PendingPayment; // sellers fulfill independently
    public Shipment? Shipment { get; set; }

    public ICollection<OrderTax> Taxes { get; set; } = new List<OrderTax>();
}

/// <summary>
/// Frozen, per-line tax breakdown captured at checkout. Storing the actual
/// applied amount (not just a reference to TaxRate) means invoices remain
/// correct forever even after an admin later changes the rate.
/// </summary>
public class OrderTax : BaseEntity
{
    public Guid OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = default!;

    public Guid TaxRateId { get; set; }
    public TaxRate TaxRate { get; set; } = default!;

    public TaxComponentType ComponentType { get; set; }
    public decimal RatePercentageApplied { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

using JewelryHub.Domain.Catalog;
using JewelryHub.Domain.Common;
using JewelryHub.Domain.Customers;
using JewelryHub.Domain.Orders;
using JewelryHub.Domain.Sellers;

namespace JewelryHub.Domain.Reviews;

/// <summary>
/// A single review that can target a Product and/or its Seller (both
/// nullable-paired FKs on one row rather than two entities, since in
/// practice a customer reviews the purchase experience as a whole and the
/// UI captures both in one form). Linked to the OrderItem it came from so
/// only verified purchasers can review — a key trust signal for jewelry.
/// </summary>
public class Review : AuditableEntity, ISoftDelete
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    public Guid OrderItemId { get; set; } // proves a verified purchase
    public OrderItem OrderItem { get; set; } = default!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public Guid SellerId { get; set; }
    public Seller Seller { get; set; } = default!;

    public int ProductRating { get; set; }  // 1-5
    public int SellerRating { get; set; }   // 1-5
    public string? Title { get; set; }
    public string? Comment { get; set; }

    public string? SellerResponse { get; set; }
    public DateTime? SellerRespondedAtUtc { get; set; }

    public bool IsFlagged { get; set; }
    public bool IsApproved { get; set; } = true; // admin can moderate

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

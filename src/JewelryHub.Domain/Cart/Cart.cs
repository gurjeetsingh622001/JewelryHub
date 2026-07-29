using JewelryHub.Domain.Catalog;
using JewelryHub.Domain.Common;
using JewelryHub.Domain.Customers;

namespace JewelryHub.Domain.Cart;

/// <summary>One active cart per customer. Items snapshot the price at add-time; checkout re-validates against current price/stock before confirming.</summary>
public class Cart : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public class CartItem : AuditableEntity
{
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = default!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public int Quantity { get; set; } = 1;
    public decimal UnitPriceSnapshot { get; set; } // price at time of adding, re-checked at checkout
}

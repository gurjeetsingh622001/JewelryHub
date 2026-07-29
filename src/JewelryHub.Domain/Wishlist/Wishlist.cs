using JewelryHub.Domain.Catalog;
using JewelryHub.Domain.Common;
using JewelryHub.Domain.Customers;

namespace JewelryHub.Domain.Wishlist;

public class Wishlist : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}

public class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; set; }
    public Wishlist Wishlist { get; set; } = default!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
}

using JewelryHub.Domain.Common;
using JewelryHub.Domain.Identity;
using JewelryHub.Domain.Orders;
using JewelryHub.Domain.Reviews;

namespace JewelryHub.Domain.Customers;

/// <summary>
/// Buyer-facing profile data, separate from User so authentication concerns
/// (password, lockout, tokens) never mix with commerce/profile concerns.
/// </summary>
public class Customer : AuditableEntity, ISoftDelete
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public DateOnly? DateOfBirth { get; set; }
    public string? GenderIdentity { get; set; }
    public string? ProfileImageUrl { get; set; }

    public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

/// <summary>
/// Reusable shipping/billing address, since a customer may have several.
/// Soft-deleted rather than hard-deleted (see OrderConfigurations' remarks
/// on Order.ShippingAddress/BillingAddress) so a historical Order can
/// always resolve the address it was placed with, even after the customer
/// removes it from their saved list.
/// </summary>
public class CustomerAddress : AuditableEntity, ISoftDelete
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    public string Label { get; set; } = default!; // "Home", "Office"
    public string AddressLine1 { get; set; } = default!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = default!;
    public string State { get; set; } = default!; // drives CGST/SGST vs IGST determination
    public string PostalCode { get; set; } = default!;
    public string Country { get; set; } = "India";
    public string? ContactPhone { get; set; }
    public bool IsDefault { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

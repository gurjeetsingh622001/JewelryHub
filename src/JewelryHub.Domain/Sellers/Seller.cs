using JewelryHub.Domain.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Identity;

namespace JewelryHub.Domain.Sellers;

/// <summary>
/// A jewelry business account. Split from User for the same reason as
/// Customer, plus sellers carry a verification workflow and business/tax
/// data that has no meaning for a plain customer account.
/// </summary>
public class Seller : AuditableEntity, ISoftDelete
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string BusinessName { get; set; } = default!;
    public string? BusinessDescription { get; set; }
    public string? LogoUrl { get; set; }

    public string GstNumber { get; set; } = default!;   // GSTIN — required to compute tax jurisdiction (CGST/SGST vs IGST)
    public string? PanNumber { get; set; }
    public string BusinessRegistrationNumber { get; set; } = default!;

    public string AddressLine1 { get; set; } = default!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;       // seller's home state — the other half of the GST determination
    public string PostalCode { get; set; } = default!;
    public string Country { get; set; } = "India";

    public SellerVerificationStatus VerificationStatus { get; set; } = SellerVerificationStatus.PendingApproval;
    public string? RejectionReason { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public Guid? VerifiedByAdminId { get; set; }

    // Denormalized, periodically recalculated rollups — avoids expensive
    // aggregate queries every time a seller dashboard is opened.
    public decimal TotalRevenue { get; set; }
    public int TotalOrdersFulfilled { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public ICollection<SellerDocument> Documents { get; set; } = new List<SellerDocument>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

/// <summary>A single uploaded KYC/business document, verified independently by admin staff.</summary>
public class SellerDocument : AuditableEntity
{
    public Guid SellerId { get; set; }
    public Seller Seller { get; set; } = default!;

    public string DocumentType { get; set; } = default!; // "GST Certificate", "PAN Card", "Shop License", "Hallmark License"
    public string FileUrl { get; set; } = default!;       // Azure Blob Storage URL
    public string? FileName { get; set; }

    public DocumentVerificationStatus Status { get; set; } = DocumentVerificationStatus.Pending;
    public string? ReviewerNote { get; set; }
    public Guid? ReviewedByAdminId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
}

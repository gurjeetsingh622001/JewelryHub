using JewelryHub.Domain.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Sellers;

namespace JewelryHub.Domain.Unions;

/// <summary>
/// A jewelry business union/association. Sellers join as members and
/// govern themselves through meetings, polls, and announcements — this
/// is the root of the "professional community" side of the platform,
/// separate from the marketplace/commerce side.
/// </summary>
public class Union : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }

    public string City { get; set; } = default!;
    public string State { get; set; } = default!;

    public Guid CreatedBySellerId { get; set; }
    public Seller CreatedBySeller { get; set; } = default!;

    public bool IsApprovedByAdmin { get; set; } // admin approves union creation, per spec "Manage jewelry unions"
    public Guid? ApprovedByAdminId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }

    public decimal? AnnualMembershipFee { get; set; }

    public ICollection<UnionMember> Members { get; set; } = new List<UnionMember>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

/// <summary>
/// A seller's membership in a specific union, including their governance
/// role there. A seller can belong to multiple unions with different roles
/// in each, so role lives here rather than on Seller.
/// </summary>
public class UnionMember : AuditableEntity
{
    public Guid UnionId { get; set; }
    public Union Union { get; set; } = default!;

    public Guid SellerId { get; set; }
    public Seller Seller { get; set; } = default!;

    public UnionMemberRole Role { get; set; } = UnionMemberRole.Member;
    public UnionMembershipStatus Status { get; set; } = UnionMembershipStatus.PendingApproval;

    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? MembershipFeePaidThroughUtc { get; set; }
}

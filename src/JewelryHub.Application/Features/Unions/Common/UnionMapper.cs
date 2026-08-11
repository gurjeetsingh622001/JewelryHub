using JewelryHub.Domain.Unions;

namespace JewelryHub.Application.Features.Unions.Common;

/// <summary>
/// Caller must have loaded CreatedBySeller (.Include(u => u.CreatedBySeller)) first.
/// ActiveMemberCount is passed in rather than computed off a loaded Members
/// collection, so list queries can batch it as one GROUP BY instead of an
/// N+1 count per union — see GetUnionsQuery/GetPendingUnionsQuery.
/// </summary>
public static class UnionMapper
{
    public static UnionDto ToDto(Union u, int activeMemberCount) => new(
        u.Id, u.Name, u.Description, u.LogoUrl, u.City, u.State,
        u.CreatedBySellerId, u.CreatedBySeller.BusinessName,
        u.IsApprovedByAdmin, u.ApprovedAtUtc, u.AnnualMembershipFee,
        activeMemberCount, u.CreatedAtUtc);

    /// <summary>Caller must have loaded Seller (.Include(m => m.Seller)).</summary>
    public static UnionMemberDto ToDto(UnionMember m) => new(
        m.Id, m.UnionId, m.SellerId, m.Seller.BusinessName, m.Seller.LogoUrl,
        m.Role, m.Status, m.JoinedAtUtc, m.MembershipFeePaidThroughUtc);

    /// <summary>Caller must have loaded PublishedByMember.Seller.</summary>
    public static UnionAnnouncementDto ToDto(UnionAnnouncement a) => new(
        a.Id, a.UnionId, a.Title, a.Body, a.IsPinned,
        a.PublishedByMemberId, a.PublishedByMember.Seller.BusinessName, a.CreatedAtUtc);

    /// <summary>Caller must have loaded UploadedByMember.Seller.</summary>
    public static UnionDocumentDto ToDto(UnionDocument d) => new(
        d.Id, d.UnionId, d.Title, d.FileUrl, d.Category,
        d.UploadedByMemberId, d.UploadedByMember.Seller.BusinessName, d.CreatedAtUtc);

    public static UnionEventDto ToDto(UnionEvent e) => new(
        e.Id, e.UnionId, e.Title, e.Description, e.Location, e.StartsAtUtc, e.EndsAtUtc, e.Status);
}

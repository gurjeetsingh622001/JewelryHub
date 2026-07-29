using JewelryHub.Domain.Sellers;

namespace JewelryHub.Application.Features.Sellers.Common;

/// <summary>Caller must have loaded Documents (.Include(s => s.Documents)) first — see note on ProductMapper for why these mappers stay explicit instead of lazy-loading.</summary>
public static class SellerMapper
{
    public static SellerDto ToDto(Seller s) => new(
        s.Id, s.UserId, s.BusinessName, s.BusinessDescription, s.LogoUrl, s.GstNumber, s.City, s.State,
        s.VerificationStatus, s.RejectionReason, s.TotalRevenue, s.TotalOrdersFulfilled, s.AverageRating, s.ReviewCount,
        s.Documents.OrderByDescending(d => d.CreatedAtUtc)
            .Select(d => new SellerDocumentDto(d.Id, d.DocumentType, d.FileUrl, d.FileName, d.Status, d.ReviewerNote, d.CreatedAtUtc))
            .ToList());
}

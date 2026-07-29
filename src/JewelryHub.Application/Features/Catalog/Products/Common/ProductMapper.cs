using JewelryHub.Domain.Catalog;

namespace JewelryHub.Application.Features.Catalog.Products.Common;

/// <summary>
/// Callers must have loaded Category, Seller, Images, Gemstones,
/// Certificates and Inventory (via .Include(...)) before calling these —
/// they don't lazy-load, so a forgotten Include shows up immediately as a
/// null-reference in a code review/test rather than a silent N+1 query.
/// </summary>
public static class ProductMapper
{
    public static ProductListItemDto ToListItemDto(Product p) => new(
        p.Id, p.Name, p.Slug, p.MetalType, p.Purity, p.BasePrice, p.DiscountPercentage,
        p.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).FirstOrDefault()?.Url,
        p.AverageRating, p.ReviewCount, p.Status,
        p.SellerId, p.Seller.BusinessName, p.Category.Name,
        InStock: p.Inventory is null || !p.Inventory.TrackInventory || p.Inventory.QuantityAvailable > p.Inventory.QuantityReserved);

    public static ProductDto ToDto(Product p) => new(
        p.Id, p.Name, p.Slug, p.Sku, p.Description, p.ProductType, p.Status,
        p.MetalType, p.Purity, p.GrossWeightGrams, p.NetWeightGrams, p.MetalRatePerGramAtListing,
        p.Size, p.SizeUnit,
        p.MetalValue, p.MakingCharges, p.MakingChargesArePercentage, p.GemstoneValue, p.WastageCharges,
        p.BasePrice, p.DiscountPercentage,
        p.IsHallmarked, p.HallmarkUniqueId, p.CertificationAuthority,
        p.AverageRating, p.ReviewCount,
        p.SellerId, p.Seller.BusinessName,
        p.CategoryId, p.Category.Name,
        p.Inventory?.QuantityAvailable ?? 0,
        p.Images.OrderBy(i => i.DisplayOrder).Select(i => new ProductImageDto(i.Id, i.Url, i.AltText, i.DisplayOrder, i.IsPrimary)).ToList(),
        p.Gemstones.Select(g => new ProductGemstoneDto(g.Id, g.GemstoneType, g.WeightCarats, g.ClarityGrade, g.ColorGrade, g.CutGrade, g.Quantity, g.Value)).ToList(),
        p.Certificates.Select(c => new ProductCertificateDto(c.Id, c.CertificateType, c.IssuingAuthority, c.CertificateNumber, c.FileUrl)).ToList());
}

using JewelryHub.Domain.Enums;

namespace JewelryHub.Application.Features.Catalog.Products.Common;

public record ProductListItemDto(
    Guid Id,
    string Name,
    string Slug,
    MetalType MetalType,
    PurityType Purity,
    decimal BasePrice,
    decimal? DiscountPercentage,
    string? PrimaryImageUrl,
    decimal AverageRating,
    int ReviewCount,
    ProductStatus Status,
    Guid SellerId,
    string SellerBusinessName,
    string CategoryName,
    bool InStock);

public record ProductDto(
    Guid Id,
    string Name,
    string Slug,
    string Sku,
    string? Description,
    ProductType ProductType,
    ProductStatus Status,
    MetalType MetalType,
    PurityType Purity,
    decimal GrossWeightGrams,
    decimal NetWeightGrams,
    decimal MetalRatePerGramAtListing,
    string? Size,
    string? SizeUnit,
    decimal MetalValue,
    decimal MakingCharges,
    bool MakingChargesArePercentage,
    decimal GemstoneValue,
    decimal WastageCharges,
    decimal BasePrice,
    decimal? DiscountPercentage,
    bool IsHallmarked,
    string? HallmarkUniqueId,
    string? CertificationAuthority,
    decimal AverageRating,
    int ReviewCount,
    Guid SellerId,
    string SellerBusinessName,
    Guid CategoryId,
    string CategoryName,
    int QuantityAvailable,
    IReadOnlyList<ProductImageDto> Images,
    IReadOnlyList<ProductGemstoneDto> Gemstones,
    IReadOnlyList<ProductCertificateDto> Certificates);

public record ProductImageDto(Guid Id, string Url, string? AltText, int DisplayOrder, bool IsPrimary);

public record ProductGemstoneDto(
    Guid Id,
    string GemstoneType,
    decimal WeightCarats,
    string? ClarityGrade,
    string? ColorGrade,
    string? CutGrade,
    int Quantity,
    decimal Value);

public record ProductCertificateDto(Guid Id, string CertificateType, string IssuingAuthority, string? CertificateNumber, string FileUrl);

namespace JewelryHub.Application.Features.Catalog.Products.Common;

public record CreateProductGemstoneRequest(
    string GemstoneType,
    decimal WeightCarats,
    string? ClarityGrade,
    string? ColorGrade,
    string? CutGrade,
    int Quantity,
    decimal Value);

public record CreateProductImageRequest(string Url, string? AltText, int DisplayOrder, bool IsPrimary);

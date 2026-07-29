namespace JewelryHub.Application.Features.Catalog.Categories.Common;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? IconUrl,
    Guid? ParentCategoryId,
    int DisplayOrder,
    bool IsActive);

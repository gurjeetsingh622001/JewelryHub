using JewelryHub.Domain.Common;

namespace JewelryHub.Domain.Catalog;

/// <summary>
/// Self-referencing category tree (e.g. Jewelry > Rings > Engagement Rings)
/// managed by admins. Kept separate from ProductType (an enum) because
/// categories are merchandising taxonomy that changes often, while
/// ProductType is a stable structural attribute of the item itself.
/// </summary>
public class Category : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!; // URL-friendly, unique
    public string? Description { get; set; }
    public string? IconUrl { get; set; }

    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

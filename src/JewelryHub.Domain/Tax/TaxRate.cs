using JewelryHub.Domain.Catalog;
using JewelryHub.Domain.Common;
using JewelryHub.Domain.Enums;

namespace JewelryHub.Domain.Tax;

/// <summary>
/// An admin-configurable tax rule. Rates are looked up at checkout time
/// (Strategy pattern in the Application layer selects CGST+SGST vs IGST
/// based on comparing the seller's and the shipping address's state),
/// never hardcoded — required per spec since GST rules and gold/jewelry
/// tax rates change periodically.
/// </summary>
public class TaxRate : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = default!; // "GST on Gold Jewelry 3%"
    public TaxComponentType ComponentType { get; set; }
    public decimal RatePercentage { get; set; }

    public TaxApplicability Applicability { get; set; } = TaxApplicability.AllProducts;
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public MetalType? MetalType { get; set; }

    public DateTime EffectiveFromUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveToUtc { get; set; } // null = open-ended; superseding rates get a new row rather than editing history
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

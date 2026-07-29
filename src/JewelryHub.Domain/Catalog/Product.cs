using JewelryHub.Domain.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Sellers;

namespace JewelryHub.Domain.Catalog;

/// <summary>
/// A jewelry listing. Pricing is modeled as explicit components
/// (metal value, making charges, gemstone value) rather than one flat
/// price field, because that breakdown is standard practice in the
/// jewelry trade and customers/invoices expect to see it itemized —
/// it also lets making charges be edited independently when gold rates move.
/// </summary>
public class Product : AuditableEntity, ISoftDelete
{
    public Guid SellerId { get; set; }
    public Seller Seller { get; set; } = default!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Sku { get; set; } = default!; // seller-scoped unique code
    public string? Description { get; set; }

    public ProductType ProductType { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    // --- Metal & weight ---
    public MetalType MetalType { get; set; }
    public PurityType Purity { get; set; }
    public decimal GrossWeightGrams { get; set; }
    public decimal NetWeightGrams { get; set; } // gross minus stone weight
    public decimal MetalRatePerGramAtListing { get; set; } // snapshot of the gold/silver rate used to price this item

    // --- Size / dimensions (interpretation depends on ProductType, e.g. ring size vs chain length) ---
    public string? Size { get; set; }
    public string? SizeUnit { get; set; } // "US", "cm", "inch"

    // --- Pricing breakdown ---
    public decimal MetalValue { get; set; }        // NetWeightGrams * MetalRatePerGramAtListing
    public decimal MakingCharges { get; set; }
    public bool MakingChargesArePercentage { get; set; } // true => MakingCharges is a % of MetalValue, false => flat amount
    public decimal GemstoneValue { get; set; }      // sum of ProductGemstones values, denormalized for fast read
    public decimal WastageCharges { get; set; }
    public decimal BasePrice { get; set; }          // MetalValue + MakingCharges + GemstoneValue + WastageCharges (tax applied at order time, not baked in)
    public decimal? DiscountPercentage { get; set; }

    // --- Hallmark / certification ---
    public bool IsHallmarked { get; set; }
    public string? HallmarkUniqueId { get; set; } // BIS HUID for gold jewelry in India
    public string? CertificationAuthority { get; set; } // "BIS", "IGI", "GIA"

    public int ViewCount { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductCertificate> Certificates { get; set; } = new List<ProductCertificate>();
    public ICollection<ProductGemstone> Gemstones { get; set; } = new List<ProductGemstone>();
    public Inventory? Inventory { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

/// <summary>A product can carry more than one stone (e.g. a ring with a center diamond plus side stones).</summary>
public class ProductGemstone : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public string GemstoneType { get; set; } = default!; // "Diamond", "Ruby", "Emerald", "Sapphire"
    public decimal WeightCarats { get; set; }
    public string? ClarityGrade { get; set; }
    public string? ColorGrade { get; set; }
    public string? CutGrade { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal Value { get; set; }
}

/// <summary>Product photo; ordering matters for gallery display, one is flagged primary for listings/thumbnails.</summary>
public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public string Url { get; set; } = default!;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}

/// <summary>Scanned certificate (hallmark certificate, gemstone grading report, etc.) attached for buyer trust.</summary>
public class ProductCertificate : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public string CertificateType { get; set; } = default!; // "Hallmark", "Diamond Grading Report"
    public string IssuingAuthority { get; set; } = default!;
    public string? CertificateNumber { get; set; }
    public string FileUrl { get; set; } = default!;
    public DateTime? IssuedDate { get; set; }
}

/// <summary>
/// Stock tracking, 1:1 with Product. Kept separate from Product so
/// high-frequency stock writes (decrement on order, restock) don't
/// contend with/invalidate caches of largely-static product listing data.
/// </summary>
public class Inventory : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public int QuantityAvailable { get; set; }
    public int QuantityReserved { get; set; } // held for unpaid/processing carts & orders
    public int ReorderThreshold { get; set; }
    public bool TrackInventory { get; set; } = true; // false for made-to-order/one-of-a-kind pieces
}

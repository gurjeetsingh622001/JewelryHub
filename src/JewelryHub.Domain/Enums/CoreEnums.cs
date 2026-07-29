namespace JewelryHub.Domain.Enums;

/// <summary>
/// Verification lifecycle for a seller's business/KYC submission.
/// Drives what a seller is allowed to do (a Rejected/PendingApproval
/// seller cannot list products).
/// </summary>
public enum SellerVerificationStatus
{
    PendingApproval = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    Suspended = 4
}

/// <summary>Status of an individual KYC/business document uploaded by a seller.</summary>
public enum DocumentVerificationStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2
}

/// <summary>Precious metal a jewelry product is primarily made of.</summary>
public enum MetalType
{
    Gold = 0,
    Silver = 1,
    Platinum = 2,
    Palladium = 3,
    Other = 4
}

/// <summary>
/// Standard purity gradings used across the jewelry trade. Kept as an
/// enum (rather than a free-text field) so filtering/search stays reliable,
/// while MetalType determines which values are actually valid for a product
/// (validated in the Application layer, not the enum itself).
/// </summary>
public enum PurityType
{
    K9 = 0,
    K14 = 1,
    K18 = 2,
    K20 = 3,
    K22 = 4,
    K24 = 5,
    Silver925 = 6,   // Sterling silver
    Silver999 = 7,   // Fine silver
    Platinum950 = 8,
    Platinum900 = 9
}

/// <summary>High-level jewelry category grouping, separate from the seller-managed Category tree.</summary>
public enum ProductType
{
    Ring = 0,
    Necklace = 1,
    Earring = 2,
    Bracelet = 3,
    Bangle = 4,
    Pendant = 5,
    Chain = 6,
    Anklet = 7,
    Nosepin = 8,
    Coin = 9,
    Other = 10
}

/// <summary>Approval state of a product listing before it becomes publicly visible.</summary>
public enum ProductStatus
{
    Draft = 0,
    PendingReview = 1,
    Active = 2,
    OutOfStock = 3,
    Rejected = 4,
    Archived = 5
}

namespace JewelryHub.Domain.Enums;

/// <summary>Lifecycle of a customer order. Transitions are enforced in the Domain/Application layer, not by the enum itself.</summary>
public enum OrderStatus
{
    PendingPayment = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    ReturnRequested = 6,
    Returned = 7,
    Refunded = 8
}

public enum PaymentStatus
{
    Pending = 0,
    Authorized = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4,
    PartiallyRefunded = 5
}

public enum PaymentMethod
{
    CreditCard = 0,
    DebitCard = 1,
    Upi = 2,
    NetBanking = 3,
    Wallet = 4,
    CashOnDelivery = 5
}

public enum ShipmentStatus
{
    Pending = 0,
    Packed = 1,
    Shipped = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Failed = 5,
    ReturnedToSeller = 6
}

/// <summary>
/// Indian GST tax components. Modeled explicitly (rather than a generic
/// "tax name" string) because CGST/SGST vs IGST applicability depends on
/// buyer/seller state, which is business logic the Application layer needs
/// to reason about, not just display.
/// </summary>
public enum TaxComponentType
{
    CGST = 0,
    SGST = 1,
    IGST = 2,
    CESS = 3
}

/// <summary>What a configurable tax rule applies to; lets admins scope rates precisely.</summary>
public enum TaxApplicability
{
    AllProducts = 0,
    CategorySpecific = 1,
    MetalTypeSpecific = 2
}

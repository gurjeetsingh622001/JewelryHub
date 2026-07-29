using JewelryHub.Domain.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Orders;

namespace JewelryHub.Domain.Payments;

/// <summary>
/// A payment attempt/transaction against an order. Modeled 1-to-many with
/// Order (not 1:1) because retries, partial captures, and refunds are all
/// separate transactions that must remain individually auditable.
/// </summary>
public class Payment : AuditableEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";

    public string? GatewayProvider { get; set; } // "Razorpay", "Stripe" — adapter pattern in Infrastructure abstracts the actual SDK
    public string? GatewayTransactionId { get; set; }
    public string? GatewayReferenceId { get; set; }
    public string? FailureReason { get; set; }

    public DateTime? PaidAtUtc { get; set; }
    public decimal RefundedAmount { get; set; }
}

using JewelryHub.Domain.Enums;

namespace JewelryHub.Application.Common.Interfaces;

public record TaxLineResult(Guid TaxRateId, TaxComponentType ComponentType, decimal RatePercentageApplied, decimal TaxableAmount, decimal TaxAmount);

/// <summary>
/// Resolves which configured TaxRate rows apply to a line item and how
/// much tax they produce. Never hardcodes a rate — every number here
/// traces back to an admin-configured TaxRate row (per spec: "Tax rates
/// must be configurable rather than hard-coded").
/// </summary>
public interface ITaxCalculator
{
    Task<IReadOnlyList<TaxLineResult>> CalculateAsync(
        Guid categoryId, MetalType metalType, string sellerState, string buyerState, decimal taxableAmount,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Computes the shipping charge for one seller's line items within an
/// order. Each seller ships independently (see the note on Order), so
/// CreateOrderCommand groups items by SellerId and sums one call per
/// group into Order.ShippingCharges. A pure function, not async — unlike
/// tax there's no admin-configured rate table behind it yet (see
/// docs/PROJECT_STATUS.md); swapping in a DB-backed implementation later
/// only requires a new IShippingCalculator, not a change to CreateOrder.
/// </summary>
public interface IShippingCalculator
{
    decimal Calculate(decimal sellerSubtotal, decimal totalWeightGrams, bool isInterState);
}

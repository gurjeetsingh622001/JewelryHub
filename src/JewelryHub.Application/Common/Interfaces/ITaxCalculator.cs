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

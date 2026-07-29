using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Tax;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Common.Services;

/// <summary>
/// Concrete strategy for GST-style taxation. A different ITaxCalculator
/// implementation could be swapped in (e.g. for a future non-India market)
/// without touching CreateOrder — that's the point of keeping this behind
/// an interface rather than inlining the logic into the checkout handler.
/// </summary>
public class GstTaxCalculator : ITaxCalculator
{
    private readonly IUnitOfWork _unitOfWork;

    public GstTaxCalculator(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<TaxLineResult>> CalculateAsync(
        Guid categoryId, MetalType metalType, string sellerState, string buyerState, decimal taxableAmount,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var candidates = await _unitOfWork.TaxRates.Query()
            .Where(t => t.IsActive && t.EffectiveFromUtc <= now && (t.EffectiveToUtc == null || t.EffectiveToUtc >= now))
            .Where(t => t.Applicability == TaxApplicability.AllProducts
                     || (t.Applicability == TaxApplicability.CategorySpecific && t.CategoryId == categoryId)
                     || (t.Applicability == TaxApplicability.MetalTypeSpecific && t.MetalType == metalType))
            .ToListAsync(cancellationToken);

        var isInterState = !string.Equals(sellerState.Trim(), buyerState.Trim(), StringComparison.OrdinalIgnoreCase);

        // Only one rate per component type is applied — if both an
        // AllProducts and a CategorySpecific rate exist for the same
        // component, the more specific one wins (CategorySpecific/
        // MetalTypeSpecific > AllProducts), matching how a shop owner
        // would expect an override to behave.
        var applicableComponents = isInterState
            ? new[] { TaxComponentType.IGST, TaxComponentType.CESS }
            : new[] { TaxComponentType.CGST, TaxComponentType.SGST, TaxComponentType.CESS };

        var chosenRates = candidates
            .Where(t => applicableComponents.Contains(t.ComponentType))
            .GroupBy(t => t.ComponentType)
            .Select(g => g.OrderByDescending(Specificity).First())
            .ToList();

        return chosenRates
            .Select(rate =>
            {
                var taxAmount = Math.Round(taxableAmount * rate.RatePercentage / 100m, 2);
                return new TaxLineResult(rate.Id, rate.ComponentType, rate.RatePercentage, taxableAmount, taxAmount);
            })
            .ToList();
    }

    private static int Specificity(TaxRate rate) => rate.Applicability switch
    {
        TaxApplicability.MetalTypeSpecific => 2,
        TaxApplicability.CategorySpecific => 2,
        TaxApplicability.AllProducts => 1,
        _ => 0,
    };
}

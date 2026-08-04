using JewelryHub.Application.Common.Interfaces;

namespace JewelryHub.Application.Common.Services;

/// <summary>
/// Flat base rate + an inter-state surcharge (jewelry couriers charge more
/// for cross-state insured shipments) + a per-gram surcharge past a small
/// weight allowance (most jewelry pieces are light; heavier pieces like
/// bridal sets cost more to ship securely). Waived entirely above a
/// free-shipping subtotal threshold, same incentive most marketplaces use.
/// </summary>
public class FlatRateShippingCalculator : IShippingCalculator
{
    private const decimal FreeShippingThreshold = 5000m;
    private const decimal BaseRate = 99m;
    private const decimal InterStateSurcharge = 50m;
    private const decimal WeightAllowanceGrams = 100m;
    private const decimal RatePerGramOverAllowance = 2m;

    public decimal Calculate(decimal sellerSubtotal, decimal totalWeightGrams, bool isInterState)
    {
        if (sellerSubtotal >= FreeShippingThreshold)
        {
            return 0m;
        }

        var charge = BaseRate;

        if (isInterState)
        {
            charge += InterStateSurcharge;
        }

        if (totalWeightGrams > WeightAllowanceGrams)
        {
            charge += (totalWeightGrams - WeightAllowanceGrams) * RatePerGramOverAllowance;
        }

        return Math.Round(charge, 2);
    }
}

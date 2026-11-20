using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Billing;
using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Pricing;

/// <summary>Повна ціна за прайсом, без знижок.</summary>
public sealed class StandardPricingPolicy : IPricingPolicy
{
    /// <inheritdoc/>
    public decimal PriceOf(VisitItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return Math.Round(item.Amount, PricingRules.MoneyDecimalPlaces);
    }
}

using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Pricing;

/// <summary>Знижка з фіксованою ставкою на кожну послугу.</summary>
public sealed class DiscountPricingPolicy : IPricingPolicy
{
    /// <summary>Найбільша дозволена ставка знижки.</summary>
    public const decimal MaxRate = 0.5m;

    private readonly decimal _rate;

    /// <summary>Створює політику зі ставкою знижки.</summary>
    /// <param name="rate">Частка знижки від 0 до <see cref="MaxRate"/>, наприклад 0.05m.</param>
    /// <exception cref="ArgumentOutOfRangeException">Ставка від'ємна або більша за <see cref="MaxRate"/>.</exception>
    public DiscountPricingPolicy(decimal rate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rate);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rate, MaxRate);
        _rate = rate;
    }

    /// <inheritdoc/>
    public decimal PriceOf(VisitItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return Money.Round(item.Amount * (1m - _rate));
    }
}

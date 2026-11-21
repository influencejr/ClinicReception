namespace ClinicReception.Legacy;

/// <summary>Знижка на прийом залежно від виду пацієнта.</summary>
public sealed class DiscountPolicy
{
    // порядок важливий: діє перше правило, що підійшло (як у колишньому if / else if)
    private readonly IReadOnlyList<IDiscountRule> _rules = new IDiscountRule[]
    {
        new VipDiscountRule(),
        new StaffDiscountRule(),
        new LoyalDiscountRule(),
    };

    /// <summary>Обчислює знижку, обмежену <see cref="PricingRules.MaxDiscount"/>.</summary>
    /// <param name="subtotal">Сума послуг без знижки, грн.</param>
    /// <param name="patient">Пацієнт: вид і кількість попередніх візитів.</param>
    /// <returns>Знижка, грн.</returns>
    public decimal For(decimal subtotal, Patient patient)
    {
        foreach (IDiscountRule rule in _rules)
        {
            if (rule.AppliesTo(patient))
            {
                return Math.Min(subtotal * rule.Rate, PricingRules.MaxDiscount);
            }
        }

        return 0m;
    }
}

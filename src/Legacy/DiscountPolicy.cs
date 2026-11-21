namespace ClinicReception.Legacy;

/// <summary>Знижка на прийом залежно від виду пацієнта.</summary>
public sealed class DiscountPolicy
{
    /// <summary>Обчислює знижку, обмежену <see cref="PricingRules.MaxDiscount"/>.</summary>
    /// <param name="subtotal">Сума послуг без знижки, грн.</param>
    /// <param name="kind">Вид пацієнта.</param>
    /// <param name="visits">Кількість попередніх візитів.</param>
    /// <returns>Знижка, грн.</returns>
    public decimal For(decimal subtotal, PatientKind kind, int visits)
    {
        decimal rate = 0m;
        if (kind == PatientKind.Vip)
        {
            rate = PricingRules.VipRate;
        }
        else if (kind == PatientKind.Staff)
        {
            rate = PricingRules.StaffRate;
        }
        else if (visits > PricingRules.LoyalVisits)
        {
            rate = PricingRules.LoyalRate;
        }

        return Math.Min(subtotal * rate, PricingRules.MaxDiscount);
    }
}

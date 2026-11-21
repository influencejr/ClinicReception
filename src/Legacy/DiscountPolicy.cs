namespace ClinicReception.Legacy;

/// <summary>Знижка на прийом залежно від виду пацієнта.</summary>
public sealed class DiscountPolicy
{
    /// <summary>Обчислює знижку, обмежену <see cref="PricingRules.MaxDiscount"/>.</summary>
    /// <param name="subtotal">Сума послуг без знижки, грн.</param>
    /// <param name="patient">Пацієнт: вид і кількість попередніх візитів.</param>
    /// <returns>Знижка, грн.</returns>
    public decimal For(decimal subtotal, Patient patient)
    {
        decimal rate = 0m;
        if (patient.Kind == PatientKind.Vip)
        {
            rate = PricingRules.VipRate;
        }
        else if (patient.Kind == PatientKind.Staff)
        {
            rate = PricingRules.StaffRate;
        }
        else if (patient.VisitCount > PricingRules.LoyalVisits)
        {
            rate = PricingRules.LoyalRate;
        }

        return Math.Min(subtotal * rate, PricingRules.MaxDiscount);
    }
}

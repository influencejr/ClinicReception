namespace ClinicReception.Legacy;

/// <summary>Одне правило знижки: кому воно підходить і яка ставка.</summary>
public interface IDiscountRule
{
    /// <summary>Ставка знижки, частка від суми.</summary>
    decimal Rate { get; }

    /// <summary>Чи діє правило для пацієнта.</summary>
    /// <param name="patient">Пацієнт.</param>
    /// <returns>true, якщо пацієнт має право на цю знижку.</returns>
    bool AppliesTo(Patient patient);
}

/// <summary>Знижка VIP-пацієнта.</summary>
public sealed class VipDiscountRule : IDiscountRule
{
    /// <inheritdoc/>
    public decimal Rate => PricingRules.VipRate;

    /// <inheritdoc/>
    public bool AppliesTo(Patient patient) => patient.Kind == PatientKind.Vip;
}

/// <summary>Знижка працівника клініки.</summary>
public sealed class StaffDiscountRule : IDiscountRule
{
    /// <inheritdoc/>
    public decimal Rate => PricingRules.StaffRate;

    /// <inheritdoc/>
    public bool AppliesTo(Patient patient) => patient.Kind == PatientKind.Staff;
}

/// <summary>Знижка постійного пацієнта.</summary>
public sealed class LoyalDiscountRule : IDiscountRule
{
    /// <inheritdoc/>
    public decimal Rate => PricingRules.LoyalRate;

    /// <inheritdoc/>
    public bool AppliesTo(Patient patient) => patient.IsLoyal;
}

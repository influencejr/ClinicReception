using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Billing;

/// <summary>
/// Розрахунок вартості візиту, розкладений на окремі кроки.
/// Усі методи класу — чисті функції: залежать лише від аргументів і нічого не змінюють.
/// </summary>
public static class VisitTotalCalculator
{
    /// <summary>Обчислює підсумок візиту та стан, у який він переходить.</summary>
    /// <param name="request">Коректний запит на розрахунок.</param>
    /// <returns>Сума до сплати, заокруглена до копійок, і новий стан візиту.</returns>
    /// <exception cref="ArgumentException">Запит порушує передумови (див. VisitGuards).</exception>
    public static TotalResult Calculate(TotalRequest request)
    {
        VisitGuards.EnsureRequestValid(request);
        decimal total = SubtotalOf(request.Items);
        total -= RegularPatientDiscountOf(total, request.Patient.IsRegular);
        total -= VolumeDiscountOf(total);
        total -= PromoDiscountOf(total, request.PromoCode);
        total -= SundayDiscountOf(total, request.VisitDate);
        total += RegistrationFeeOf(total, request.RegistrationFee);
        total = request.Status == VisitStatus.Cancelled
            ? 0m
            : Money.Round(Math.Max(total, 0m));
        return new TotalResult(total, NextStatusOf(request.Status, total));
    }

    /// <summary>Обчислює суму послуг візиту зі знижкою за курс процедур.</summary>
    /// <param name="items">Непорожній перелік коректних послуг.</param>
    /// <returns>Сума за всіма послугами, грн.</returns>
    public static decimal SubtotalOf(IReadOnlyList<VisitItem> items)
    {
        VisitGuards.EnsureItemsValid(items);
        decimal subtotal = 0m;
        foreach (VisitItem item in items)
        {
            decimal amount = item.Quantity * item.UnitPrice;
            if (item.Quantity >= PricingRules.BulkQuantity)
            {
                amount -= amount * PricingRules.BulkRate;
            }

            subtotal += amount;
        }

        return subtotal;
    }

    /// <summary>Обчислює знижку постійному пацієнтові.</summary>
    /// <param name="amount">Сума, від якої рахується знижка, грн.</param>
    /// <param name="isRegularPatient">Ознака постійного пацієнта.</param>
    /// <returns>Розмір знижки, грн; 0 для нового пацієнта.</returns>
    public static decimal RegularPatientDiscountOf(decimal amount, bool isRegularPatient)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return isRegularPatient ? amount * PricingRules.RegularPatientRate : 0m;
    }

    /// <summary>Обчислює знижку за обсягом візиту.</summary>
    /// <param name="amount">Сума після попередніх знижок, грн.</param>
    /// <returns>
    /// Розмір знижки, грн, не більший за <see cref="PricingRules.MaxVolumeDiscount"/>;
    /// 0, якщо сума не перевищує нижнього порогу.
    /// </returns>
    public static decimal VolumeDiscountOf(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return Math.Min(amount * VolumeRateOf(amount), PricingRules.MaxVolumeDiscount);
    }

    /// <summary>Обчислює знижку за промокодом.</summary>
    /// <param name="amount">Сума після попередніх знижок, грн.</param>
    /// <param name="promoCode">Промокод або null, якщо його не вказано.</param>
    /// <returns>Розмір знижки, грн; 0 для невідомого чи відсутнього коду.</returns>
    public static decimal PromoDiscountOf(decimal amount, string? promoCode)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return promoCode switch
        {
            PricingRules.CheckupPromoCode => amount * PricingRules.CheckupPromoRate,
            PricingRules.FixedPromoCode when amount > PricingRules.FixedPromoFrom
                => PricingRules.FixedPromoAmount,
            _ => 0m,
        };
    }

    /// <summary>Обчислює знижку за прийом у неділю.</summary>
    /// <param name="amount">Сума після попередніх знижок, грн.</param>
    /// <param name="visitDate">Дата прийому.</param>
    /// <returns>Розмір знижки, грн; 0 для робочих днів і суботи.</returns>
    public static decimal SundayDiscountOf(decimal amount, DateOnly visitDate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return visitDate.DayOfWeek == DayOfWeek.Sunday ? amount * PricingRules.SundayRate : 0m;
    }

    /// <summary>Визначає реєстраційний збір, який додається до невеликого візиту.</summary>
    /// <param name="amount">Сума після всіх знижок, грн.</param>
    /// <param name="registrationFee">Розмір збору за прайсом, грн, невід'ємний.</param>
    /// <returns>Збір, грн; 0, якщо сума досягає порогу безкоштовної реєстрації.</returns>
    public static decimal RegistrationFeeOf(decimal amount, decimal registrationFee)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(registrationFee);
        return amount < PricingRules.FreeRegistrationFrom ? registrationFee : 0m;
    }

    /// <summary>Визначає стан, у який переходить візит після розрахунку.</summary>
    /// <param name="current">Поточний стан візиту.</param>
    /// <param name="total">Підсумкова сума візиту, грн.</param>
    /// <returns>Новий стан; для решти випадків — поточний.</returns>
    public static VisitStatus NextStatusOf(VisitStatus current, decimal total) => current switch
    {
        VisitStatus.Scheduled when total > 0m => VisitStatus.Confirmed,
        VisitStatus.Confirmed => VisitStatus.Completed,
        _ => current,
    };

    /// <summary>Визначає ставку знижки за обсягом за порогами суми.</summary>
    /// <param name="amount">Невід'ємна сума після попередніх знижок, грн.</param>
    /// <returns>Частка знижки: велика, середня або 0.</returns>
    private static decimal VolumeRateOf(decimal amount) => amount switch
    {
        > PricingRules.LargeVisitFrom => PricingRules.LargeVisitRate,
        > PricingRules.MediumVisitFrom => PricingRules.MediumVisitRate,
        _ => 0m,
    };
}

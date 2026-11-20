namespace ClinicReception.Core.Billing;

/// <summary>
/// Правила ціноутворення візиту: пороги, ставки знижок і промокоди.
/// Ставки задано частками знижки (0.05m = 5 %), а не множниками.
/// </summary>
public static class PricingRules
{
    /// <summary>Кількість одиниць послуги, з якої діє знижка за курс процедур.</summary>
    public const int BulkQuantity = 10;

    /// <summary>Частка знижки за курс процедур.</summary>
    public const decimal BulkRate = 0.05m;

    /// <summary>Частка знижки постійному пацієнтові.</summary>
    public const decimal RegularPatientRate = 0.07m;

    /// <summary>Сума візиту, понад яку діє велика знижка за обсягом, грн.</summary>
    public const decimal LargeVisitFrom = 5000m;

    /// <summary>Частка великої знижки за обсягом.</summary>
    public const decimal LargeVisitRate = 0.10m;

    /// <summary>Сума візиту, понад яку діє середня знижка за обсягом, грн.</summary>
    public const decimal MediumVisitFrom = 2000m;

    /// <summary>Частка середньої знижки за обсягом.</summary>
    public const decimal MediumVisitRate = 0.05m;

    /// <summary>Частка знижки за прийом у неділю.</summary>
    public const decimal SundayRate = 0.02m;

    /// <summary>Частка знижки за промокодом профілактичного огляду.</summary>
    public const decimal CheckupPromoRate = 0.10m;

    /// <summary>Фіксована знижка за промокодом, грн.</summary>
    public const decimal FixedPromoAmount = 200m;

    /// <summary>Сума візиту, понад яку діє фіксована знижка за промокодом, грн.</summary>
    public const decimal FixedPromoFrom = 1000m;

    /// <summary>Сума візиту, з якої реєстраційний збір не стягується, грн.</summary>
    public const decimal FreeRegistrationFrom = 1000m;

    /// <summary>Кількість знаків після коми в підсумку: заокруглення до копійок.</summary>
    public const int MoneyDecimalPlaces = 2;

    /// <summary>Промокод профілактичного огляду.</summary>
    public const string CheckupPromoCode = "CHECKUP10";

    /// <summary>Промокод фіксованої знижки.</summary>
    public const string FixedPromoCode = "MINUS200";
}

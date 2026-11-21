namespace ClinicReception.Legacy;

/// <summary>Правила ціни прийому: ставки знижок, межі й вартість доставки.</summary>
public static class PricingRules
{
    /// <summary>Знижка VIP-пацієнта.</summary>
    public const decimal VipRate = 0.15m;

    /// <summary>Знижка працівника клініки.</summary>
    public const decimal StaffRate = 0.30m;

    /// <summary>Знижка постійного пацієнта.</summary>
    public const decimal LoyalRate = 0.05m;

    /// <summary>Найбільша знижка, грн.</summary>
    public const decimal MaxDiscount = 500m;

    /// <summary>Сума до сплати, від якої доставка безкоштовна, грн.</summary>
    public const decimal FreeShipFrom = 1000m;

    /// <summary>Вартість доставки, грн.</summary>
    public const decimal ShipCost = 60m;

    /// <summary>Кількість візитів, після якої пацієнт вважається постійним.</summary>
    public const int LoyalVisits = 10;
}

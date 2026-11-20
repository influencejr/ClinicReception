namespace ClinicReception.Core.Domain;

/// <summary>Послуга в межах прийому: код, кількість, ціна й тривалість.</summary>
public sealed class VisitItem
{
    /// <summary>Код послуги за прайсом клініки.</summary>
    public string ServiceCode { get; init; } = string.Empty;

    /// <summary>Кількість одиниць послуги.</summary>
    public int Quantity { get; init; }

    /// <summary>Ціна за одиницю, грн.</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>Тривалість однієї одиниці послуги, хв.</summary>
    public int DurationMinutes { get; init; }

    /// <summary>Сума за рядком: кількість × ціна, грн.</summary>
    public decimal Amount => Quantity * UnitPrice;
}

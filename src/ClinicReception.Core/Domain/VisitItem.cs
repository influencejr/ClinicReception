namespace ClinicReception.Core.Domain;

/// <summary>
/// Послуга в межах прийому: код, кількість, ціна й тривалість.
/// Незмінна: після створення позицію зіпсувати неможливо.
/// </summary>
public sealed record VisitItem
{
    /// <summary>Створює позицію, перевіряючи всі її інваріанти.</summary>
    /// <param name="serviceCode">Код послуги за прайсом клініки.</param>
    /// <param name="quantity">Кількість одиниць послуги, більша за нуль.</param>
    /// <param name="unitPrice">Ціна за одиницю, грн, не від'ємна.</param>
    /// <param name="durationMinutes">Тривалість однієї одиниці послуги, хв, більша за нуль.</param>
    /// <exception cref="ArgumentException">Код послуги порожній.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Кількість, ціна або тривалість поза межами.</exception>
    public VisitItem(string serviceCode, int quantity, decimal unitPrice, int durationMinutes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceCode);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(durationMinutes);

        ServiceCode = serviceCode;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DurationMinutes = durationMinutes;
    }

    /// <summary>Код послуги за прайсом клініки.</summary>
    public string ServiceCode { get; }

    /// <summary>Кількість одиниць послуги.</summary>
    public int Quantity { get; }

    /// <summary>Ціна за одиницю, грн.</summary>
    public decimal UnitPrice { get; }

    /// <summary>Тривалість однієї одиниці послуги, хв.</summary>
    public int DurationMinutes { get; }

    /// <summary>Сума за рядком: кількість × ціна, грн.</summary>
    public decimal Amount => Quantity * UnitPrice;

    /// <summary>Тривалість рядка: кількість × тривалість одиниці, хв.</summary>
    public int TotalMinutes => Quantity * DurationMinutes;
}

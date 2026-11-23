namespace ClinicReception.Infrastructure.Storage;

/// <summary>Послуга запису у файлі appointments.json.</summary>
public sealed record StoredItem
{
    /// <summary>Код послуги.</summary>
    public required string ServiceCode { get; init; }

    /// <summary>Кількість.</summary>
    public required int Quantity { get; init; }

    /// <summary>Ціна за одиницю, грн.</summary>
    public required decimal UnitPrice { get; init; }

    /// <summary>Тривалість одиниці, хв.</summary>
    public required int DurationMinutes { get; init; }
}

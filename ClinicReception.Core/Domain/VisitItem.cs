namespace ClinicReception.Core.Domain;

/// <summary>Послуга в межах прийому: код, кількість, ціна й тривалість.</summary>
public sealed class VisitItem
{
    public string ServiceCode { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }

    public int DurationMinutes { get; init; }

    public decimal Amount => Quantity * UnitPrice;
}

namespace ClinicReception.Core.Domain;

/// <summary>Запис пацієнта на прийом до лікаря.</summary>
public sealed class Appointment
{
    private readonly List<VisitItem> _items = new();

    public int Id { get; init; }
    public int PatientId { get; init; }
    public string DoctorCode { get; init; } = string.Empty;
    public DateOnly VisitDate { get; init; }
    public TimeOnly StartTime { get; init; }
    public VisitStatus Status { get; set; } = VisitStatus.Scheduled;
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyList<VisitItem> Items => _items;

    /// <summary>Додає послугу до запису.</summary>
    public void AddItem(VisitItem item) => _items.Add(item);

    /// <summary>Обчислює вартість прийому.</summary>
    public decimal Total()
    {
        decimal sum = 0m;
        foreach (VisitItem item in _items)
        {
            sum += item.Amount;
        }

        return sum;
    }

    /// <summary>Обчислює загальну тривалість прийому у хвилинах.</summary>
    public int TotalMinutes()
    {
        int minutes = 0;
        foreach (VisitItem item in _items)
        {
            minutes += item.Quantity * item.DurationMinutes;
        }

        return minutes;
    }
}

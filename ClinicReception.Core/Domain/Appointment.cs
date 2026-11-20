namespace ClinicReception.Core.Domain;

/// <summary>Запис пацієнта на прийом до лікаря.</summary>
public sealed class Appointment
{
    private readonly List<VisitItem> _items = new();

    /// <summary>Номер запису на прийом.</summary>
    public int Id { get; init; }

    /// <summary>Ідентифікатор пацієнта, якого записано.</summary>
    public int PatientId { get; init; }

    /// <summary>Код лікаря, до якого записано пацієнта.</summary>
    public string DoctorCode { get; init; } = string.Empty;

    /// <summary>Дата прийому.</summary>
    public DateOnly VisitDate { get; init; }

    /// <summary>Час початку прийому.</summary>
    public TimeOnly StartTime { get; init; }

    /// <summary>Поточний стан запису.</summary>
    public VisitStatus Status { get; set; } = VisitStatus.Scheduled;

    /// <summary>Момент створення запису в реєстратурі.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Послуги, які отримає пацієнт під час прийому.</summary>
    public IReadOnlyList<VisitItem> Items => _items;

    /// <summary>Додає послугу до запису.</summary>
    /// <param name="item">Послуга, яку отримає пацієнт під час прийому.</param>
    public void AddItem(VisitItem item) => _items.Add(item);

    /// <summary>Обчислює вартість прийому.</summary>
    /// <returns>Сума за всіма послугами запису, грн.</returns>
    public decimal Total()
    {
        decimal sum = 0m;
        foreach (VisitItem item in _items)
        {
            sum += item.Amount;
        }

        return sum;
    }

    /// <summary>Обчислює загальну тривалість прийому.</summary>
    /// <returns>Тривалість усіх послуг запису, хв.</returns>
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

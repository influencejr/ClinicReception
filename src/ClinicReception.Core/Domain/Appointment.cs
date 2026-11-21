using ClinicReception.Core.Errors;

namespace ClinicReception.Core.Domain;

/// <summary>
/// Запис пацієнта на прийом до лікаря. Агрегат: перелік послуг і стан
/// змінюються лише через методи, які перевіряють правила з docs/model.md.
/// </summary>
public sealed class Appointment
{
    /// <summary>Дозволені переходи стану: пара «поточний стан — новий стан».</summary>
    private static readonly HashSet<(VisitStatus From, VisitStatus To)> AllowedTransitions = new()
    {
        (VisitStatus.Scheduled, VisitStatus.Confirmed),
        (VisitStatus.Scheduled, VisitStatus.Cancelled),
        (VisitStatus.Confirmed, VisitStatus.Completed),
        (VisitStatus.Confirmed, VisitStatus.Missed),
        (VisitStatus.Confirmed, VisitStatus.Cancelled),
    };

    private readonly List<VisitItem> _items = new();

    /// <summary>Створює запис у стані <see cref="VisitStatus.Scheduled"/>.</summary>
    /// <param name="id">Номер запису, більший за нуль.</param>
    /// <param name="patientId">Ідентифікатор пацієнта, більший за нуль.</param>
    /// <param name="doctorCode">Код лікаря в довіднику.</param>
    /// <param name="visitDate">Дата прийому.</param>
    /// <param name="startTime">Час початку прийому.</param>
    /// <exception cref="ArgumentOutOfRangeException">Номер запису або пацієнта не додатний.</exception>
    /// <exception cref="ArgumentException">Код лікаря порожній.</exception>
    public Appointment(int id, int patientId, string doctorCode, DateOnly visitDate, TimeOnly startTime)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(patientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(doctorCode);

        Id = id;
        PatientId = patientId;
        DoctorCode = doctorCode;
        VisitDate = visitDate;
        StartTime = startTime;
        Status = VisitStatus.Scheduled;
    }

    /// <summary>Номер запису на прийом.</summary>
    public int Id { get; }

    /// <summary>Ідентифікатор пацієнта, якого записано.</summary>
    public int PatientId { get; }

    /// <summary>Код лікаря, до якого записано пацієнта.</summary>
    public string DoctorCode { get; }

    /// <summary>Дата прийому.</summary>
    public DateOnly VisitDate { get; }

    /// <summary>Час початку прийому.</summary>
    public TimeOnly StartTime { get; }

    /// <summary>Поточний стан запису; змінюється лише методами-переходами.</summary>
    public VisitStatus Status { get; private set; }

    /// <summary>Послуги запису; додати послугу можна лише через <see cref="AddItem"/>.</summary>
    public IReadOnlyList<VisitItem> Items => _items;

    /// <summary>Чи закрито запис: прийом відбувся, пацієнт не прийшов або запис скасовано.</summary>
    public bool IsClosed => Status is VisitStatus.Completed or VisitStatus.Missed or VisitStatus.Cancelled;

    /// <summary>Вартість послуг за прайсом, без знижок, грн.</summary>
    public decimal Subtotal => _items.Sum(item => item.Amount);

    /// <summary>Загальна тривалість усіх послуг запису, хв.</summary>
    public int TotalMinutes => _items.Sum(item => item.TotalMinutes);

    /// <summary>Додає послугу до відкритого запису.</summary>
    /// <param name="item">Послуга, яку отримає пацієнт під час прийому.</param>
    /// <exception cref="ArgumentNullException">Послугу не передано.</exception>
    /// <exception cref="DomainRuleException">Запис уже закрито.</exception>
    public void AddItem(VisitItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (IsClosed)
        {
            throw new DomainRuleException("appointment.editable", $"Запис № {Id} закрито ({Status}), послуги додавати не можна");
        }

        _items.Add(item);
    }

    /// <summary>Підтверджує запис після дзвінка пацієнтові.</summary>
    /// <exception cref="DomainRuleException">Запис порожній або перехід заборонено.</exception>
    public void Confirm()
    {
        if (_items.Count == 0)
        {
            throw new DomainRuleException("appointment.items", $"Запис № {Id} не містить жодної послуги, підтверджувати нічого");
        }

        MoveTo(VisitStatus.Confirmed);
    }

    /// <summary>Позначає, що прийом відбувся.</summary>
    /// <exception cref="DomainRuleException">Запис не підтверджено.</exception>
    public void Complete() => MoveTo(VisitStatus.Completed);

    /// <summary>Позначає, що пацієнт не прийшов на прийом.</summary>
    /// <exception cref="DomainRuleException">Запис не підтверджено.</exception>
    public void MarkMissed() => MoveTo(VisitStatus.Missed);

    /// <summary>Скасовує запис до прийому.</summary>
    /// <exception cref="DomainRuleException">Запис уже закрито.</exception>
    public void Cancel() => MoveTo(VisitStatus.Cancelled);

    private void MoveTo(VisitStatus target)
    {
        if (!AllowedTransitions.Contains((Status, target)))
        {
            throw new DomainRuleException("appointment.transition", $"Запис № {Id}: перехід {Status} -> {target} заборонено");
        }

        Status = target;
    }
}

using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Reporting;

/// <summary>Дані звіту «завантаження лікаря за день» (UC-05).</summary>
public sealed record DoctorDayLoad
{
    private const decimal PercentScale = 100m;

    /// <summary>Лікар.</summary>
    public required Doctor Doctor { get; init; }

    /// <summary>Дата звіту.</summary>
    public required DateOnly VisitDate { get; init; }

    /// <summary>Нескасовані прийоми за часом початку.</summary>
    public required IReadOnlyList<LoadRow> Visits { get; init; }

    /// <summary>Проміжки робочого часу без прийомів.</summary>
    public required IReadOnlyList<TimeSlot> FreeWindows { get; init; }

    /// <summary>Робочий час лікаря, хв.</summary>
    public int WorkingMinutes => Doctor.WorkingHours.DurationMinutes;

    /// <summary>Зайнятий час, хв.</summary>
    public int BusyMinutes => Visits.Sum(v => v.Slot.DurationMinutes);

    /// <summary>Завантаження, відсотки з одним знаком після коми.</summary>
    public decimal LoadPercent => Math.Round(PercentScale * BusyMinutes / WorkingMinutes, 1, MidpointRounding.AwayFromZero);
}

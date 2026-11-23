namespace ClinicReception.Core.Domain;

/// <summary>Умови пошуку вільного слота в розкладі лікаря.</summary>
public sealed record SlotSearch
{
    /// <summary>Створює умови пошуку.</summary>
    /// <param name="workingHours">Робочий час лікаря.</param>
    /// <param name="stepMinutes">Крок сітки слотів, хв; слот починається лише на кроці сітки.</param>
    /// <param name="durationMinutes">Потрібна тривалість прийому, хв.</param>
    /// <param name="earliest">Найраніший допустимий початок, наприклад поточний час для сьогоднішнього дня.</param>
    /// <exception cref="ArgumentOutOfRangeException">Крок або тривалість не додатні.</exception>
    public SlotSearch(TimeSlot workingHours, int stepMinutes, int durationMinutes, TimeOnly earliest)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stepMinutes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(durationMinutes);
        WorkingHours = workingHours;
        StepMinutes = stepMinutes;
        DurationMinutes = durationMinutes;
        Earliest = earliest;
    }

    /// <summary>Робочий час лікаря.</summary>
    public TimeSlot WorkingHours { get; }

    /// <summary>Крок сітки слотів, хв.</summary>
    public int StepMinutes { get; }

    /// <summary>Потрібна тривалість прийому, хв.</summary>
    public int DurationMinutes { get; }

    /// <summary>Найраніший допустимий початок.</summary>
    public TimeOnly Earliest { get; }
}

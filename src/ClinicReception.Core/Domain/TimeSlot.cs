namespace ClinicReception.Core.Domain;

/// <summary>
/// Інтервал часу прийому [StartTime; EndTime): початок входить в інтервал, кінець ні (X-01),
/// тож прийоми 09:00-09:20 і 09:20-09:40 не перетинаються.
/// </summary>
public readonly record struct TimeSlot
{
    /// <summary>Створює інтервал.</summary>
    /// <param name="startTime">Початок, входить в інтервал.</param>
    /// <param name="endTime">Кінець, не входить в інтервал.</param>
    /// <exception cref="ArgumentException">Кінець не пізніше за початок.</exception>
    public TimeSlot(TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("Кінець інтервалу має бути пізніше за початок.", nameof(endTime));
        }

        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>Початок інтервалу.</summary>
    public TimeOnly StartTime { get; }

    /// <summary>Кінець інтервалу, який уже не входить у нього.</summary>
    public TimeOnly EndTime { get; }

    /// <summary>Тривалість, хв.</summary>
    public int DurationMinutes => MinutesOf(EndTime) - MinutesOf(StartTime);

    /// <summary>Створює інтервал заданої тривалості. Стану не змінює.</summary>
    /// <param name="startTime">Початок.</param>
    /// <param name="durationMinutes">Тривалість, хв, більша за нуль.</param>
    /// <returns>Інтервал [startTime; startTime + durationMinutes).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Тривалість не додатна.</exception>
    public static TimeSlot Of(TimeOnly startTime, int durationMinutes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(durationMinutes);
        return new TimeSlot(startTime, startTime.AddMinutes(durationMinutes));
    }

    /// <summary>Хвилини від початку доби. Стану не змінює.</summary>
    /// <param name="time">Час доби.</param>
    /// <returns>Кількість цілих хвилин від 00:00.</returns>
    public static int MinutesOf(TimeOnly time) => (int)time.ToTimeSpan().TotalMinutes;

    /// <summary>Чи перетинається інтервал з іншим (X-03): суміжні інтервали не перетинаються.</summary>
    /// <param name="other">Інший інтервал.</param>
    /// <returns>true, якщо є спільна хвилина.</returns>
    public bool Overlaps(TimeSlot other) => StartTime <= other.EndTime && other.StartTime <= EndTime;
}

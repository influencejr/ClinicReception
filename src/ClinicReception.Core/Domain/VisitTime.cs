namespace ClinicReception.Core.Domain;

/// <summary>Час прийому: дата й інтервал у межах цієї дати (X-05: кінець обчислюється з тривалості).</summary>
/// <param name="VisitDate">Дата прийому.</param>
/// <param name="Slot">Інтервал прийому.</param>
public sealed record VisitTime(DateOnly VisitDate, TimeSlot Slot)
{
    /// <summary>Створює час прийому з початку й тривалості. Стану не змінює.</summary>
    /// <param name="visitDate">Дата прийому.</param>
    /// <param name="startTime">Початок прийому.</param>
    /// <param name="durationMinutes">Тривалість, хв.</param>
    /// <returns>Час прийому.</returns>
    public static VisitTime At(DateOnly visitDate, TimeOnly startTime, int durationMinutes = ScheduleRules.DefaultVisitMinutes) =>
        new(visitDate, TimeSlot.Of(startTime, durationMinutes));
}

namespace ClinicReception.Core.Domain;

/// <summary>
/// Профільне правило варіанта: добір першого вільного слота без перетину інтервалів.
/// Слот починається на кроці сітки від початку робочого дня, не раніше заданого часу,
/// цілком уміщується в робочий час і не перетинається з жодним зайнятим інтервалом.
/// </summary>
public static class FreeSlotFinder
{
    /// <summary>Знаходить перший вільний слот. Чиста функція, стану не змінює.</summary>
    /// <param name="search">Умови пошуку.</param>
    /// <param name="busy">Зайняті інтервали лікаря й пацієнта на цю дату.</param>
    /// <returns>Вільний слот або null, якщо до кінця робочого дня вільного немає.</returns>
    public static TimeSlot? FindFirst(SlotSearch search, IReadOnlyCollection<TimeSlot> busy)
    {
        ArgumentNullException.ThrowIfNull(search);
        ArgumentNullException.ThrowIfNull(busy);
        int dayEnd = TimeSlot.MinutesOf(search.WorkingHours.EndTime);
        for (int start = FirstCandidate(search); start + search.DurationMinutes <= dayEnd; start += search.StepMinutes)
        {
            TimeSlot candidate = TimeSlot.Of(TimeOnly.MinValue.AddMinutes(start), search.DurationMinutes);
            if (!busy.Any(candidate.Overlaps))
            {
                return candidate;
            }
        }

        return null;
    }

    // перший крок сітки, не раніший за Earliest
    private static int FirstCandidate(SlotSearch search)
    {
        int dayStart = TimeSlot.MinutesOf(search.WorkingHours.StartTime);
        int earliest = TimeSlot.MinutesOf(search.Earliest);
        if (earliest <= dayStart)
        {
            return dayStart;
        }

        int stepsToEarliest = (earliest - dayStart + search.StepMinutes - 1) / search.StepMinutes;
        return dayStart + (stepsToEarliest * search.StepMinutes);
    }
}

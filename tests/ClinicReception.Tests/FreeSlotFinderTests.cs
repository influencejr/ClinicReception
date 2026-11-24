using System.Globalization;
using ClinicReception.Core.Domain;

namespace ClinicReception.Tests;

// Таблиця рішень профільного правила (docs/spec.md, розділ 1.4): робочий час 08:00-12:00,
// крок сітки 20 хв. Кожен рядок InlineData - рядок таблиці.
public class FreeSlotFinderTests
{
    private static readonly TimeSlot Morning = new(new TimeOnly(8, 0), new TimeOnly(12, 0));

    [Theory]
    [InlineData("", "08:00", 20, "08:00")]
    [InlineData("08:00-08:20", "08:00", 20, "08:20")]
    [InlineData("08:10-08:30", "08:00", 20, "08:40")]
    [InlineData("08:00-09:00", "08:00", 40, "09:00")]
    [InlineData("08:20-08:40", "08:00", 40, "08:40")]
    [InlineData("", "09:05", 20, "09:20")]
    [InlineData("08:00-11:40", "08:00", 20, "11:40")]
    [InlineData("08:00-11:40", "08:00", 40, "")]
    [InlineData("08:00-12:00", "08:00", 20, "")]
    [InlineData("", "12:30", 20, "")]
    [InlineData("08:00-08:20;08:20-08:40;09:00-09:20", "08:00", 20, "08:40")]
    public void FindFirst_DecisionTableRow_ReturnsExpectedSlot(string busy, string earliest, int minutes, string expected)
    {
        SlotSearch search = new(Morning, 20, minutes, T(earliest));

        TimeSlot? slot = FreeSlotFinder.FindFirst(search, ParseSlots(busy));

        Assert.Equal(expected, slot is null ? string.Empty : slot.Value.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void FindFirst_FoundSlot_HasRequestedDuration()
    {
        TimeSlot? slot = FreeSlotFinder.FindFirst(new SlotSearch(Morning, 20, 45, new TimeOnly(8, 0)), Array.Empty<TimeSlot>());

        Assert.Equal(45, slot!.Value.DurationMinutes);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(20, -5)]
    public void SlotSearch_NonPositiveStepOrDuration_Throws(int step, int minutes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SlotSearch(Morning, step, minutes, new TimeOnly(8, 0)));
    }

    [Theory]
    [InlineData("09:00", "09:20", "09:20", "09:40", false)]
    [InlineData("09:00", "09:20", "09:10", "09:30", true)]
    [InlineData("09:00", "10:00", "09:20", "09:40", true)]
    public void Overlaps_TwoSlots_HalfOpenIntervals(string s1, string e1, string s2, string e2, bool expected)
    {
        TimeSlot first = new(T(s1), T(e1));
        TimeSlot second = new(T(s2), T(e2));

        Assert.Equal(expected, first.Overlaps(second));
        Assert.Equal(expected, second.Overlaps(first));
    }

    [Fact]
    public void TimeSlot_EndNotAfterStart_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TimeSlot(new TimeOnly(9, 0), new TimeOnly(9, 0)));
    }

    private static List<TimeSlot> ParseSlots(string text) =>
        text.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('-'))
            .Select(p => new TimeSlot(T(p[0]), T(p[1])))
            .ToList();

    private static TimeOnly T(string text) => TimeOnly.Parse(text, CultureInfo.InvariantCulture);
}

using System.Globalization;
using ClinicReception.Core.Domain;

namespace ClinicReception.Tests;

public class VisitTests
{
    [Fact]
    public void CalculateTotal_NoInsurance_Returns2196()
    {
        Visit visit = SampleVisit();

        decimal total = visit.CalculateTotal(false);

        Assert.Equal(2196.0m, total);
    }

    [Fact]
    public void CalculateTotal_WithInsurance_Returns1976_40()
    {
        Visit visit = SampleVisit();

        decimal total = visit.CalculateTotal(true);

        Assert.Equal(1976.40m, total);
    }

    [Fact]
    public void CalculateTotal_LargeVisit_AppliesLargeVisitDiscount()
    {
        Visit visit = new("V-2001", "Петренко");
        visit.AddLine("SURG-01", 1, 6000m);

        decimal total = visit.CalculateTotal(false);

        Assert.Equal(6120m, total);
    }

    [Fact]
    public void CalculateTotal_ElevenLines_SubtractsBulkDiscount()
    {
        Visit visit = new("V-2002", "Коваль");
        for (int line = 0; line < 11; line++)
        {
            visit.AddLine("TEST-01", 1, 10m);
        }

        decimal total = visit.CalculateTotal(false);

        Assert.Equal(12m, total);
    }

    [Fact]
    public void TryChangeStatus_ScheduledToConfirmed_ReturnsTrue()
    {
        Visit visit = SampleVisit();

        bool changed = visit.TryChangeStatus(VisitStatus.Confirmed);

        Assert.True(changed);
        Assert.Equal(VisitStatus.Confirmed, visit.Status);
    }

    [Fact]
    public void TryChangeStatus_ScheduledToCompleted_ReturnsFalse()
    {
        Visit visit = SampleVisit();

        bool changed = visit.TryChangeStatus(VisitStatus.Completed);

        Assert.False(changed);
        Assert.Equal(VisitStatus.Scheduled, visit.Status);
    }

    [Fact]
    public void TryChangeStatus_ConfirmedToCompleted_ReturnsTrue()
    {
        Visit visit = SampleVisit();
        visit.TryChangeStatus(VisitStatus.Confirmed);

        bool changed = visit.TryChangeStatus(VisitStatus.Completed);

        Assert.True(changed);
        Assert.Equal(VisitStatus.Completed, visit.Status);
    }

    [Fact]
    public void TryChangeStatus_ScheduledToCancelled_ReturnsTrue()
    {
        Visit visit = SampleVisit();

        bool changed = visit.TryChangeStatus(VisitStatus.Cancelled);

        Assert.True(changed);
        Assert.Equal(VisitStatus.Cancelled, visit.Status);
    }

    [Fact]
    public void IsValid_FilledVisit_ReturnsTrue()
    {
        Assert.True(SampleVisit().IsValid());
    }

    [Fact]
    public void IsValid_NoLines_ReturnsFalse()
    {
        Assert.False(new Visit("V-2003", "Іваненко").IsValid());
    }

    [Fact]
    public void IsValid_ShortPatientName_ReturnsFalse()
    {
        Visit visit = new("V-2004", "Ли");
        visit.AddLine("CONS-01", 1, 650m);

        Assert.False(visit.IsValid());
    }

    [Fact]
    public void FindById_ExistingId_ReturnsVisit()
    {
        Visit wanted = new("V-3002", "Петренко");
        List<Visit> visits = new() { new Visit("V-3001", "Іваненко"), wanted };

        Assert.Same(wanted, Visit.FindById(visits, "V-3002"));
    }

    [Fact]
    public void FindById_UnknownId_ReturnsNull()
    {
        List<Visit> visits = new() { new Visit("V-3001", "Іваненко") };

        Assert.Null(Visit.FindById(visits, "V-9999"));
    }

    [Fact]
    public void BuildReport_UkrainianCulture_ReturnsExactReport()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("uk-UA");
        try
        {
            string report = SampleVisit().BuildReport();

            Assert.Equal(
                "Послуга: PHYS-01; кількість: 3; ціна: 250; сума: 750\n"
                + "Послуга: INJ-01; кількість: 12; ціна: 90; сума: 1080\n"
                + "Разом: 2196,0\n",
                report);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Constructor_EmptyId_ThrowsArgument()
    {
        Assert.Throws<ArgumentException>(() => new Visit(" ", "Іваненко"));
    }

    [Fact]
    public void AddLine_ZeroQuantity_ThrowsOutOfRange()
    {
        Visit visit = new("V-4001", "Іваненко");

        Assert.Throws<ArgumentOutOfRangeException>(() => visit.AddLine("CONS-01", 0, 650m));
    }

    [Fact]
    public void AddLine_NegativePrice_ThrowsOutOfRange()
    {
        Visit visit = new("V-4002", "Іваненко");

        Assert.Throws<ArgumentOutOfRangeException>(() => visit.AddLine("CONS-01", 1, -1m));
    }

    private static Visit SampleVisit()
    {
        Visit visit = new("V-1001", "Іваненко");
        visit.AddLine("PHYS-01", 3, 250m);
        visit.AddLine("INJ-01", 12, 90m);
        return visit;
    }
}

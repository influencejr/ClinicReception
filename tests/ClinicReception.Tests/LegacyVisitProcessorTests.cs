using ClinicReception.Core.Domain;
using ClinicReception.Core.Legacy;

namespace ClinicReception.Tests;

public class LegacyVisitProcessorTests
{
    private static DateOnly Tuesday => new(2026, 3, 10);

    [Fact]
    public void Process_RegularPatient_Returns1292()
    {
        List<VisitItem> items = new()
        {
            new VisitItem { ServiceCode = "PHYS-01", Quantity = 12, UnitPrice = 100m },
            new VisitItem { ServiceCode = "CONS-01", Quantity = 1, UnitPrice = 250m },
        };

        decimal total = LegacyVisitProcessor.Process(
            1,
            "Іваненко",
            "i@ex.com",
            true,
            items,
            Tuesday,
            string.Empty,
            "Scheduled",
            false,
            60m,
            out string next);

        Assert.Equal(1292.70m, total);
        Assert.Equal("Confirmed", next);
    }

    [Fact]
    public void Process_SmallVisit_AddsRegistrationFee()
    {
        List<VisitItem> items = new()
        {
            new VisitItem { ServiceCode = "MASS-01", Quantity = 2, UnitPrice = 200m },
        };

        decimal total = LegacyVisitProcessor.Process(
            2,
            "Петренко",
            "p@ex.com",
            false,
            items,
            Tuesday,
            "CHECKUP10",
            "Confirmed",
            false,
            60m,
            out string next);

        Assert.Equal(420m, total);
        Assert.Equal("Completed", next);
    }

    [Fact]
    public void Process_NoItems_ReturnsMinusOne()
    {
        decimal total = LegacyVisitProcessor.Process(
            3,
            "Коваль",
            "k@ex.com",
            false,
            new List<VisitItem>(),
            Tuesday,
            string.Empty,
            "Scheduled",
            false,
            60m,
            out _);

        Assert.Equal(-1m, total);
    }
}

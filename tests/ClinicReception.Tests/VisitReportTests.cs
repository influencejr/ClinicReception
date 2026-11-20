using ClinicReception.Core.Billing;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Reporting;

namespace ClinicReception.Tests;

public class VisitReportTests
{
    [Fact]
    public void TextOf_CalculatedVisit_ContainsTotalLine()
    {
        TotalRequest request = BuildSampleRequest();
        TotalResult result = VisitTotalCalculator.Calculate(request);

        string text = VisitReport.TextOf(request, result);

        Assert.Contains("Разом: 1292.70", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Print_Report_WritesReportToOutput()
    {
        using StringWriter writer = new();
        VisitReportPrinter printer = new(writer);

        printer.Print("Разом: 10.00");

        Assert.Contains("10.00", writer.ToString(), StringComparison.Ordinal);
    }

    private static TotalRequest BuildSampleRequest() => new(
        VisitId: 1,
        Patient: new PatientInfo("Іваненко", "i@ex.com", IsRegular: true),
        Items: new List<VisitItem>
        {
            new() { ServiceCode = "PHYS-01", Quantity = 12, UnitPrice = 100m },
            new() { ServiceCode = "CONS-01", Quantity = 1, UnitPrice = 250m },
        },
        VisitDate: new DateOnly(2026, 3, 10),
        PromoCode: null,
        Status: VisitStatus.Scheduled,
        RegistrationFee: 60m);
}

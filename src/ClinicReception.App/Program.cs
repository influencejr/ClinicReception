using System.Text;
using ClinicReception.Core.Billing;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Reporting;

Console.OutputEncoding = Encoding.UTF8;

TotalRequest request = BuildSampleRequest();
TotalResult result = VisitTotalCalculator.Calculate(request);
string report = VisitReport.TextOf(request, result);
new VisitReportPrinter(Console.Out).Print(report);

static TotalRequest BuildSampleRequest() => new(
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

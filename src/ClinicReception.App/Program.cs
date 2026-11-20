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
        new("PHYS-01", 12, 100m, 30),
        new("CONS-01", 1, 250m, 20),
    },
    VisitDate: new DateOnly(2026, 3, 10),
    PromoCode: null,
    Status: VisitStatus.Scheduled,
    RegistrationFee: 60m);

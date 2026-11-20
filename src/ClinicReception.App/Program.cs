using System.Text;
using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Billing;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Pricing;
using ClinicReception.Core.Reporting;
using ClinicReception.Core.Services;
using ClinicReception.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

// ЛР 3: розрахунок вартості візиту
TotalRequest request = BuildSampleRequest();
TotalResult result = VisitTotalCalculator.Calculate(request);
string report = VisitReport.TextOf(request, result);
new VisitReportPrinter(Console.Out).Print(report);

// ЛР 4: композиційний корінь - єдине місце, де створюються конкретні реалізації
InMemoryAppointmentRepository repository = new();
IPricingPolicy pricing = new DiscountPricingPolicy(0.05m);
AppointmentService service = new(repository, pricing);

Appointment appointment = new(1, 10, "DOC-THER-01", new DateOnly(2026, 3, 10), new TimeOnly(9, 30));
appointment.AddItem(new VisitItem("CONS-01", 1, 650m, 20));
appointment.AddItem(new VisitItem("ECG-01", 1, 350m, 15));
service.Book(appointment);

Console.WriteLine();
Console.WriteLine($"Сума: {service.TotalOf(appointment):0.00}");

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

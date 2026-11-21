using System.Text;
using ClinicReception.App.Input;
using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Billing;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Errors;
using ClinicReception.Core.Notifications;
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
const decimal RegularPatientRate = 0.05m;
const decimal InsuredPatientRate = 0.10m;
const decimal StaffRate = 0.30m;

// таблиця «тип пацієнта - політика» замість ланцюга if у логіці (OCP);
// новий тип пацієнта - новий рядок тут або новий клас політики
Dictionary<string, IPricingPolicy> policies = new()
{
    ["regular"] = new DiscountPricingPolicy(RegularPatientRate),
    ["insured"] = new DiscountPricingPolicy(InsuredPatientRate),
    ["staff"] = new DiscountPricingPolicy(StaffRate),
};

string patientType = "regular";
IPricingPolicy pricing = policies.TryGetValue(patientType, out IPricingPolicy? found)
    ? found
    : new StandardPricingPolicy();
TextWriterNotifier notifier = new(Console.Out);
AppointmentService service = new(repository, pricing, notifier);

Patient patient = new(10, "Іваненко Олена Петрівна", "o.ivanenko@example.com") { IsRegular = true };
Appointment appointment = new(1, 10, "DOC-THER-01", new DateOnly(2026, 3, 10), new TimeOnly(9, 30));
appointment.AddItem(new VisitItem("CONS-01", 1, 650m, 20));
appointment.AddItem(new VisitItem("ECG-01", 1, 350m, 15));
Console.WriteLine();
service.Book(appointment, patient);
Console.WriteLine($"Сума: {service.TotalOf(appointment):0.00}");

// ЛР 5: послуги вводяться з консолі й проходять через барикаду VisitItemParser
Console.WriteLine();
Console.WriteLine($"Додавання послуг до запису № {appointment.Id}: код кількість ціна хвилини, порожній рядок - завершити");
while (true)
{
    Console.Write("Послуга: ");
    string? line = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(line))
    {
        break;
    }

    Result<VisitItem> parsed = VisitItemParser.Parse(line);
    if (!parsed.IsSuccess)
    {
        Console.WriteLine($"Помилка вводу: {parsed.Error}");
        continue;
    }

    try
    {
        service.AddItem(appointment.Id, parsed.Value!);
        Console.WriteLine($"Додано {parsed.Value!.ServiceCode}, сума запису {service.TotalOf(appointment):0.00} грн");
    }
    catch (DomainRuleException ex)
    {
        Console.WriteLine($"Не виконано: {ex.Message}");
    }
}

Console.WriteLine();
Console.Write(AppointmentCsvReport.BuildCsv(repository.GetAll()));

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

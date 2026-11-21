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

// верхній обробник - єдине місце, де дозволено catch (Exception)
try
{
    Run();
    return 0;
}
catch (DomainRuleException ex)
{
    Console.Error.WriteLine($"Правило {ex.Rule}: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine("Внутрішня помилка, роботу завершено.");
    Console.Error.WriteLine(ex);
    return 2;
}

static void Run()
{
    // ЛР 3: розрахунок вартості візиту
    TotalRequest request = BuildSampleRequest();
    TotalResult result = VisitTotalCalculator.Calculate(request);
    string report = VisitReport.TextOf(request, result);
    new VisitReportPrinter(Console.Out).Print(report);

    // ЛР 4: композиційний корінь - єдине місце, де створюються конкретні реалізації
    InMemoryAppointmentRepository repository = new();
    AppointmentService service = new(repository, PricingFor("regular"), new TextWriterNotifier(Console.Out));

    Patient patient = new(10, "Іваненко Олена Петрівна", "o.ivanenko@example.com") { IsRegular = true };
    Appointment appointment = new(1, 10, "DOC-THER-01", new DateOnly(2026, 3, 10), new TimeOnly(9, 30));
    appointment.AddItem(new VisitItem("CONS-01", 1, 650m, 20));
    appointment.AddItem(new VisitItem("ECG-01", 1, 350m, 15));
    Console.WriteLine();
    service.Book(appointment, patient);
    Console.WriteLine($"Сума: {service.TotalOf(appointment):0.00}");

    // ЛР 5: послуги вводяться з консолі й проходять через барикаду VisitItemParser
    Console.WriteLine();
    ReadItems(service, appointment.Id);
    Console.WriteLine();
    Console.Write(AppointmentCsvReport.BuildCsv(repository.GetAll()));
}

static IPricingPolicy PricingFor(string patientType)
{
    // таблиця «тип пацієнта - політика» замість ланцюга if у логіці (OCP)
    Dictionary<string, IPricingPolicy> policies = new()
    {
        ["regular"] = new DiscountPricingPolicy(0.05m),
        ["insured"] = new DiscountPricingPolicy(0.10m),
        ["staff"] = new DiscountPricingPolicy(0.30m),
    };
    return policies.TryGetValue(patientType, out IPricingPolicy? found) ? found : new StandardPricingPolicy();
}

static void ReadItems(AppointmentService service, int appointmentId)
{
    Console.WriteLine($"Додавання послуг до запису № {appointmentId}: код кількість ціна хвилини, порожній рядок - завершити");
    while (true)
    {
        Console.Write("Послуга: ");
        string? line = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        Result<VisitItem> parsed = VisitItemParser.Parse(line);
        if (!parsed.IsSuccess)
        {
            Console.WriteLine($"Помилка вводу: {parsed.Error}");
            continue;
        }

        try
        {
            service.AddItem(appointmentId, parsed.Value!);
            Console.WriteLine($"Додано {parsed.Value!.ServiceCode}, сума запису {service.TotalOf(service.Get(appointmentId)):0.00} грн");
        }
        catch (DomainRuleException ex)
        {
            Console.WriteLine($"Не виконано: {ex.Message}");
        }
    }
}

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

using System.Text;
using ClinicReception.App.Input;
using ClinicReception.App.Notifications;
using ClinicReception.Core;
using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Billing;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Errors;
using ClinicReception.Core.Notifications;
using ClinicReception.Core.Pricing;
using ClinicReception.Core.Reporting;
using ClinicReception.Core.Services;
using ClinicReception.Core.Storage;
using Microsoft.Extensions.Logging;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine($"ClinicReception v{VersionInfo.Current()}");

// журнал іде в stderr, діалог з користувачем - у stdout, тож їх можна розділити
using ILoggerFactory factory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.TimestampFormat = "HH:mm:ss ";
        options.IncludeScopes = true;
        options.SingleLine = true;
    });
    builder.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
    builder.SetMinimumLevel(LogLevel.Debug);
});
ILogger log = factory.CreateLogger("ClinicReception.App");

// верхній обробник - єдине місце, де дозволено catch (Exception), див. GlobalSuppressions.cs
try
{
    Run(factory);
    return 0;
}
catch (DomainRuleException ex)
{
    log.LogWarning("Правило {Rule}: {Message}", ex.Rule, ex.Message);
    return 1;
}
catch (IOException ex)
{
    // FileAuditLog не відкрився: файл тримає інша копія програми
    log.LogError(ex, "Журнал аудиту недоступний");
    Console.Error.WriteLine("Журнал аудиту зайнятий, закрийте інші копії програми.");
    return 3;
}
catch (Exception ex)
{
    log.LogCritical(ex, "Непередбачений збій, роботу завершено");
    Console.Error.WriteLine("Внутрішня помилка, подробиці в журналі.");
    return 2;
}

static void Run(ILoggerFactory factory)
{
    // using гарантує, що файл аудиту закриється й тоді, коли далі вилетить виняток
    using FileAuditLog audit = new("audit.log");

    // ЛР 3: розрахунок вартості візиту
    TotalRequest request = BuildSampleRequest();
    TotalResult result = VisitTotalCalculator.Calculate(request);
    string report = VisitReport.TextOf(request, result);
    new VisitReportPrinter(Console.Out).Print(report);

    // ЛР 4: композиційний корінь - єдине місце, де створюються конкретні реалізації
    InMemoryAppointmentRepository repository = new();
    ILogger<AppointmentService> serviceLog = factory.CreateLogger<AppointmentService>();
    AppointmentService service = new(repository, PricingFor("regular"), new TextWriterNotifier(Console.Out), serviceLog);

    Patient patient = new(10, "Іваненко Олена Петрівна", "o.ivanenko@example.com")
    {
        IsRegular = true,
        Phone = "+380671234567",
    };
    Appointment appointment = new(1, 10, "DOC-THER-01", new DateOnly(2026, 3, 10), new TimeOnly(9, 30));
    appointment.AddItem(new VisitItem("CONS-01", 1, 650m, 20));
    appointment.AddItem(new VisitItem("ECG-01", 1, 350m, 15));
    Console.WriteLine();
    service.Book(appointment, patient);
    audit.Write("book", appointment.Id);
    Console.WriteLine($"Сума: {service.TotalOf(appointment):0.00}");

    // ЛР 5: поштовий сервер недоступний - запис має зберегтися, а збій потрапити в журнал
    AppointmentService offline = new(repository, PricingFor("insured"), new OfflineNotifier(), serviceLog);
    Patient second = new(11, "Петренко Андрій Іванович", "a.petrenko@example.com") { HasInsurance = true };
    Appointment secondVisit = new(2, 11, "DOC-CARD-02", new DateOnly(2026, 3, 10), new TimeOnly(11, 0));
    secondVisit.AddItem(new VisitItem("CONS-02", 1, 800m, 30));
    offline.Book(secondVisit, second);
    audit.Write("book", secondVisit.Id);

    // ЛР 5: послуги вводяться з консолі й проходять через барикаду VisitItemParser
    Console.WriteLine();
    ReadItems(service, audit, appointment.Id);
    Console.WriteLine();
    Console.Write(AppointmentCsvReport.BuildCsv(repository.GetAll()));
    ExportReport(Path.Combine("reports", "appointments.csv"), repository.GetAll(), factory.CreateLogger("ClinicReception.App"));
}

static void ExportReport(string path, IReadOnlyList<Appointment> appointments, ILogger log)
{
    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using StreamWriter writer = new(path);
        writer.Write(AppointmentCsvReport.BuildCsv(appointments));
        Console.WriteLine($"Звіт збережено у {path}");
    }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
    {
        // звіт - не головна операція: повідомляємо й працюємо далі
        log.LogError(ex, "Звіт не збережено у {Path}", path);
        Console.WriteLine($"Звіт не збережено у {path}");
    }
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

static void ReadItems(AppointmentService service, FileAuditLog audit, int appointmentId)
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
            audit.Write("add-item", appointmentId);
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

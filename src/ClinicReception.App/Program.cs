using System.Text;
using System.Text.Json;
using ClinicReception.App;
using ClinicReception.App.Input;
using ClinicReception.Core;
using ClinicReception.Core.Errors;
using ClinicReception.Core.Notifications;
using ClinicReception.Core.Pricing;
using ClinicReception.Core.Services;
using ClinicReception.Infrastructure.Logging;
using ClinicReception.Infrastructure.Storage;
using Microsoft.Extensions.Logging;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine($"ClinicReception v{VersionInfo.Current()}");
string dataDirectory = args.Length > 0 ? args[0] : "data";
using ILoggerFactory logs = LoggerFactory.Create(builder => builder
    .AddProvider(new FileLoggerProvider(Path.Combine(dataDirectory, "logs", "clinic.log"), TimeProvider.System))
    .SetMinimumLevel(LogLevel.Information));

// композиційний корінь: єдине місце, де створюються конкретні реалізації
try
{
    JsonClinicStore store = new(dataDirectory);
    InMemoryAppointmentRepository repository = new();
    store.LoadAppointments(repository);
    InMemoryClinicDirectory directory = store.LoadDirectory();
    AppointmentService appointments = new(
        repository, new StandardPricingPolicy(), new TextWriterNotifier(Console.Out), logs.CreateLogger<AppointmentService>());
    ScheduleService schedule = new(repository, directory, appointments, TimeProvider.System);
    ScheduleQueries queries = new(repository, directory);
    ConsoleIo io = new(Console.In, Console.Out);
    new ReceptionMenu(io, schedule, queries, logs.CreateLogger<ReceptionMenu>()).Run();
    store.SaveAppointments(repository.GetAll());
    Console.WriteLine($"Записи збережено у {store.AppointmentsPath}");
    return 0;
}
catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException or DomainRuleException)
{
    // збій або пошкоджений файл даних - не помилка програми: пояснюємо, що сталося, і завершуємо з кодом 2
    logs.CreateLogger("ClinicReception.App").LogError(ex, "Дані в теці {Directory} недоступні або пошкоджені", dataDirectory);
    Console.Error.WriteLine($"Не вдалося прочитати чи зберегти дані в теці {dataDirectory}: {ex.Message}");
    return 2;
}

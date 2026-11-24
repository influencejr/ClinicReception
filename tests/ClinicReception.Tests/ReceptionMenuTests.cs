using ClinicReception.App;
using ClinicReception.App.Input;
using ClinicReception.Core.Notifications;
using ClinicReception.Core.Pricing;
using ClinicReception.Core.Services;
using ClinicReception.Tests.Doubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicReception.Tests;

// Меню рецепції від вводу до виводу: замість консолі - рядки.
public class ReceptionMenuTests
{
    [Fact]
    public void Run_InvalidDateThenValid_AsksAgainAndBooks()
    {
        string output = Run("1\n1\nDOC-THER-01\n2026-11-24\n24.11.2026\n20\n0\n");

        Assert.Contains("Помилка вводу: Дата у форматі дд.мм.рррр", output, StringComparison.Ordinal);
        Assert.Contains("Записано: № 1, 24.11.2026 08:00-08:20, лікар DOC-THER-01", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_RuleViolation_ShowsReasonAndContinues()
    {
        string output = Run("4\n99\n1\n9\n0\n");

        Assert.Contains("Не виконано: Запис № 99 не знайдено", output, StringComparison.Ordinal);
        Assert.Contains("Немає такого пункту", output, StringComparison.Ordinal);
    }

    private static string Run(string input)
    {
        FakeAppointmentRepository repository = new();
        FakeClinicDirectory directory = new();
        FixedClock clock = new(new DateTimeOffset(2026, 11, 23, 9, 5, 0, TimeSpan.Zero));
        AppointmentService appointments = new(
            repository, new StandardPricingPolicy(), new TextWriterNotifier(TextWriter.Null), NullLogger<AppointmentService>.Instance);
        StringWriter output = new();
        ReceptionMenu menu = new(
            new ConsoleIo(new StringReader(input), output),
            new ScheduleService(repository, directory, appointments, clock),
            new ScheduleQueries(repository, directory),
            NullLogger<ReceptionMenu>.Instance);

        menu.Run();

        return output.ToString();
    }
}

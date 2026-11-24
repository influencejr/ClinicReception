using System.Globalization;
using ClinicReception.App.Input;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Errors;
using ClinicReception.Core.Reporting;
using ClinicReception.Core.Services;
using Microsoft.Extensions.Logging;

namespace ClinicReception.App;

/// <summary>
/// Меню рецепції: кожен пункт - сценарій зі специфікації. Кожен сценарій виконується
/// в області журналу з його ідентифікатором; відхилена операція - Warning, збій файлу - Error.
/// </summary>
public sealed class ReceptionMenu
{
    private readonly ConsoleIo _io;
    private readonly ScheduleService _schedule;
    private readonly ScheduleQueries _queries;
    private readonly ILogger<ReceptionMenu> _log;
    private readonly Dictionary<string, (string Scenario, string Title, Action Run)> _items;

    /// <summary>Створює меню.</summary>
    /// <param name="io">Діалог з реєстратором.</param>
    /// <param name="schedule">Сценарії, що змінюють розклад.</param>
    /// <param name="queries">Пошук і звіти.</param>
    /// <param name="log">Журнал сценаріїв.</param>
    /// <exception cref="ArgumentNullException">Залежність не передано.</exception>
    public ReceptionMenu(ConsoleIo io, ScheduleService schedule, ScheduleQueries queries, ILogger<ReceptionMenu> log)
    {
        _io = io ?? throw new ArgumentNullException(nameof(io));
        _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _items = new()
        {
            ["1"] = ("UC-01", "Записати пацієнта", Book),
            ["2"] = ("UC-02", "Перенести запис", Reschedule),
            ["3"] = ("UC-02", "Скасувати запис", Cancel),
            ["4"] = ("UC-03", "Змінити стан прийому", ChangeStatus),
            ["5"] = ("UC-04", "Знайти записи", Find),
            ["6"] = ("UC-05", "Завантаження лікаря за день", ShowLoad),
            ["7"] = ("UC-06", "Додати послуги до запису", AddServices),
            ["8"] = ("UC-00", "Довідник лікарів", ShowDoctors),
        };
    }

    /// <summary>Показує меню, доки реєстратор не обере 0 або ввід не закінчиться.</summary>
    public void Run()
    {
        while (true)
        {
            _io.Say(string.Empty);
            foreach (KeyValuePair<string, (string Scenario, string Title, Action Run)> item in _items)
            {
                _io.Say($"{item.Key} - {item.Value.Title}");
            }

            _io.Say("0 - Зберегти й вийти");
            string? choice = _io.Ask("Пункт меню");
            if (choice is null || choice == "0")
            {
                return;
            }

            if (_items.TryGetValue(choice, out (string Scenario, string Title, Action Run) entry))
            {
                RunScenario(entry.Scenario, entry.Title, entry.Run);
            }
            else
            {
                _io.Say("Немає такого пункту, оберіть число з меню");
            }
        }
    }

    private static string Describe(Appointment a) =>
        string.Create(CultureInfo.InvariantCulture, $"{a.VisitDate:dd.MM.yyyy} {a.StartTime:HH:mm}-{a.EndTime:HH:mm}, лікар {a.DoctorCode}");

    private void RunScenario(string scenario, string title, Action run)
    {
        using IDisposable? scope = _log.BeginScope(scenario);
        _log.LogInformation("Початок: {Title}", title);
        try
        {
            run();
            _log.LogInformation("Завершено: {Title}", title);
        }
        catch (DomainRuleException ex)
        {
            _log.LogWarning("Відхилено за правилом {Rule}: {Message}", ex.Rule, ex.Message);
            _io.Say("Не виконано: " + ex.Message);
        }
        catch (IOException ex)
        {
            _log.LogError(ex, "Збій файлу під час сценарію {Title}", title);
            _io.Say("Не виконано: файл недоступний, спробуйте ще раз");
        }
    }

    private void Book()
    {
        if (!_io.TryAsk("Номер пацієнта", InputParser.PositiveNumber, out int patientId)
            || !_io.TryAsk("Код лікаря", InputParser.DoctorCode, out string doctorCode)
            || !_io.TryAsk("Дата (дд.мм.рррр)", InputParser.Date, out DateOnly date)
            || !_io.TryAsk("Тривалість, хв", InputParser.Minutes, out int minutes))
        {
            return;
        }

        Appointment a = _schedule.Book(new BookingRequest(patientId, doctorCode, date, minutes));
        _io.Say($"Записано: № {a.Id}, {Describe(a)}");
    }

    private void Reschedule()
    {
        if (_io.TryAsk("Номер запису", InputParser.PositiveNumber, out int id)
            && _io.TryAsk("Нова дата (дд.мм.рррр)", InputParser.Date, out DateOnly date))
        {
            Appointment a = _schedule.Reschedule(id, date);
            _io.Say($"Перенесено: № {a.Id}, {Describe(a)}");
        }
    }

    private void Cancel()
    {
        if (_io.TryAsk("Номер запису", InputParser.PositiveNumber, out int id))
        {
            string reason = _io.Ask("Причина (необов'язково)") is { Length: > 0 } text ? text : "без причини";
            _schedule.Cancel(id, reason);
            _io.Say($"Запис № {id} скасовано");
        }
    }

    private void ChangeStatus()
    {
        if (_io.TryAsk("Номер запису", InputParser.PositiveNumber, out int id)
            && _io.TryAsk("Подія: 1 - підтвердити, 2 - прийом відбувся, 3 - неявка", InputParser.Action, out StatusAction action))
        {
            Appointment a = _schedule.ChangeStatus(id, action);
            _io.Say($"Запис № {a.Id}: стан {a.Status}");
        }
    }

    private void Find()
    {
        _io.Say("Порожня відповідь - критерій не застосовується");
        AppointmentQuery query = new()
        {
            PatientId = _io.TryAsk("Номер пацієнта", InputParser.PositiveNumber, out int patientId) ? patientId : null,
            DoctorCode = _io.TryAsk("Код лікаря", InputParser.DoctorCode, out string code) ? code : null,
            From = _io.TryAsk("З дати", InputParser.Date, out DateOnly from) ? from : null,
            To = _io.TryAsk("По дату", InputParser.Date, out DateOnly to) ? to : null,
        };
        IReadOnlyList<Appointment> found = _queries.Find(query);
        foreach (Appointment a in found)
        {
            _io.Say($"№ {a.Id,-4} {Describe(a)}, пацієнт № {a.PatientId}, {a.Status}");
        }

        _io.Say(found.Count == 0 ? "Нічого не знайдено" : $"Знайдено: {found.Count}");
    }

    private void ShowLoad()
    {
        if (_io.TryAsk("Код лікаря", InputParser.DoctorCode, out string code)
            && _io.TryAsk("Дата (дд.мм.рррр)", InputParser.Date, out DateOnly date))
        {
            _io.Say(DoctorLoadReport.TextOf(_queries.LoadOf(code, date)));
        }
    }

    private void AddServices()
    {
        if (!_io.TryAsk("Номер запису", InputParser.PositiveNumber, out int id))
        {
            return;
        }

        _io.Say("Послуга одним рядком: код кількість ціна хвилини, наприклад LAB-02 1 180,50 10");
        while (_io.TryAsk("Послуга", VisitItemParser.Parse, out VisitItem item))
        {
            decimal total = _schedule.AddService(id, item);
            _io.Say(string.Create(CultureInfo.InvariantCulture, $"Додано {item.ServiceCode}, до сплати {total:0.00} грн"));
        }
    }

    private void ShowDoctors()
    {
        foreach (Doctor d in _queries.Doctors())
        {
            _io.Say($"{d.Code,-12} {d.FullName,-32} {d.Specialty,-14} {d.WorkStart:HH:mm}-{d.WorkEnd:HH:mm}, крок {d.SlotMinutes} хв");
        }
    }
}

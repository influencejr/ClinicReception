using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Errors;
using ClinicReception.Core.Reporting;

namespace ClinicReception.Core.Services;

/// <summary>Запити до розкладу, що нічого не змінюють: пошук записів (UC-04) і завантаження лікаря (UC-05).</summary>
public sealed class ScheduleQueries
{
    private readonly IAppointmentRepository _repository;
    private readonly IClinicDirectory _directory;

    /// <summary>Створює сервіс запитів.</summary>
    /// <param name="repository">Сховище записів.</param>
    /// <param name="directory">Довідники лікарів і пацієнтів.</param>
    /// <exception cref="ArgumentNullException">Залежність не передано.</exception>
    public ScheduleQueries(IAppointmentRepository repository, IClinicDirectory directory)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    /// <summary>Знаходить записи за критеріями. Стану не змінює.</summary>
    /// <param name="query">Критерії; незаповнені не обмежують результат.</param>
    /// <returns>Записи за датою й часом початку; порожній список, якщо нічого не знайдено.</returns>
    /// <exception cref="ArgumentNullException">Критерії не передано.</exception>
    public IReadOnlyList<Appointment> Find(AppointmentQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetAll()
            .Where(a => MatchesPeople(a, query) && MatchesPeriod(a, query))
            .OrderBy(a => a.VisitDate)
            .ThenBy(a => a.StartTime)
            .ToList();
    }

    /// <summary>Лікарі довідника за кодом. Стану не змінює.</summary>
    /// <returns>Усі лікарі.</returns>
    public IReadOnlyList<Doctor> Doctors() => _directory.Doctors;

    /// <summary>Будує дані звіту «завантаження лікаря за день». Стану не змінює.</summary>
    /// <param name="doctorCode">Код лікаря.</param>
    /// <param name="visitDate">Дата.</param>
    /// <returns>Прийоми, зайнятий час і вільні проміжки.</returns>
    /// <exception cref="DomainRuleException">Лікаря немає в довіднику.</exception>
    public DoctorDayLoad LoadOf(string doctorCode, DateOnly visitDate)
    {
        Doctor doctor = _directory.FindDoctor(doctorCode)
            ?? throw new DomainRuleException("doctor.exists", $"Лікаря з кодом {doctorCode} немає в довіднику");
        List<LoadRow> rows = Find(new AppointmentQuery { DoctorCode = doctor.Code, From = visitDate, To = visitDate })
            .Where(a => a.Status != VisitStatus.Cancelled)
            .Select(a => new LoadRow(a.Id, a.Slot, _directory.FindPatient(a.PatientId)?.FullName ?? "?", a.Status))
            .ToList();
        return new DoctorDayLoad
        {
            Doctor = doctor,
            VisitDate = visitDate,
            Visits = rows,
            FreeWindows = FreeWindowsOf(doctor.WorkingHours, rows.Select(r => r.Slot).ToList()),
        };
    }

    private static bool MatchesPeople(Appointment a, AppointmentQuery q) =>
        (q.PatientId is null || a.PatientId == q.PatientId)
        && (q.DoctorCode is null || string.Equals(a.DoctorCode, q.DoctorCode, StringComparison.OrdinalIgnoreCase));

    private static bool MatchesPeriod(Appointment a, AppointmentQuery q) =>
        (q.From is null || a.VisitDate >= q.From)
        && (q.To is null || a.VisitDate <= q.To)
        && (q.Status is null || a.Status == q.Status);

    // проміжки робочого часу між прийомами; прийоми впорядковано за початком
    private static List<TimeSlot> FreeWindowsOf(TimeSlot workingHours, List<TimeSlot> busy)
    {
        List<TimeSlot> windows = new();
        TimeOnly cursor = workingHours.StartTime;
        foreach (TimeSlot slot in busy)
        {
            if (slot.StartTime > cursor)
            {
                windows.Add(new TimeSlot(cursor, slot.StartTime));
            }

            cursor = slot.EndTime > cursor ? slot.EndTime : cursor;
        }

        if (cursor < workingHours.EndTime)
        {
            windows.Add(new TimeSlot(cursor, workingHours.EndTime));
        }

        return windows;
    }
}

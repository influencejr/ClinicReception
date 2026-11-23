using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Errors;

namespace ClinicReception.Core.Services;

/// <summary>
/// Сценарії розкладу: записати пацієнта (UC-01), перенести або скасувати запис (UC-02)
/// і змінити стан прийому (UC-03). Вхідні дані вже перевірено барикадою застосунку,
/// тут перевіряються лише правила предметної області.
/// </summary>
public sealed class ScheduleService
{
    private readonly IAppointmentRepository _repository;
    private readonly IClinicDirectory _directory;
    private readonly AppointmentService _appointments;
    private readonly TimeProvider _clock;

    /// <summary>Створює сервіс розкладу.</summary>
    /// <param name="repository">Сховище записів.</param>
    /// <param name="directory">Довідники лікарів і пацієнтів.</param>
    /// <param name="appointments">Сервіс записів: збереження, сповіщення, журнал.</param>
    /// <param name="clock">Джерело поточного часу.</param>
    /// <exception cref="ArgumentNullException">Залежність не передано.</exception>
    public ScheduleService(
        IAppointmentRepository repository,
        IClinicDirectory directory,
        AppointmentService appointments,
        TimeProvider clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        _appointments = appointments ?? throw new ArgumentNullException(nameof(appointments));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>Записує пацієнта на перший вільний слот лікаря в обрану дату (UC-01).</summary>
    /// <param name="request">Пацієнт, лікар, дата й тривалість.</param>
    /// <returns>Створений запис у стані Scheduled з консультацією лікаря.</returns>
    /// <exception cref="ArgumentNullException">Запит не передано.</exception>
    /// <exception cref="DomainRuleException">
    /// Пацієнта чи лікаря немає, дата в минулому або вільного слота немає.
    /// </exception>
    public Appointment Book(BookingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Patient patient = _directory.FindPatient(request.PatientId)
            ?? throw new DomainRuleException("patient.exists", $"Пацієнта № {request.PatientId} немає в довіднику");
        Doctor doctor = DoctorOf(request.DoctorCode);
        TimeSlot slot = FindSlot(doctor, request, exceptId: null);

        int id = _repository.GetAll().Select(a => a.Id).DefaultIfEmpty(0).Max() + 1;
        Appointment appointment = new(id, patient.Id, doctor.Code, new VisitTime(request.VisitDate, slot));
        appointment.AddItem(new VisitItem(ScheduleRules.ConsultationCode, 1, doctor.ConsultationFee, slot.DurationMinutes));
        _appointments.Book(appointment, patient);
        return appointment;
    }

    /// <summary>Переносить запис на першу вільну годину іншої дати тієї самої тривалості (UC-02).</summary>
    /// <param name="appointmentId">Номер запису.</param>
    /// <param name="newDate">Нова дата.</param>
    /// <returns>Перенесений запис.</returns>
    /// <exception cref="DomainRuleException">Запису немає, він закритий, дата в минулому або вільного слота немає.</exception>
    public Appointment Reschedule(int appointmentId, DateOnly newDate)
    {
        Appointment appointment = _appointments.Get(appointmentId);
        Doctor doctor = DoctorOf(appointment.DoctorCode);
        BookingRequest request = new(appointment.PatientId, doctor.Code, newDate, appointment.Slot.DurationMinutes);
        TimeSlot slot = FindSlot(doctor, request, appointment.Id);
        appointment.Reschedule(new VisitTime(newDate, slot));
        return appointment;
    }

    /// <summary>Скасовує запис (UC-02, альтернатива).</summary>
    /// <param name="appointmentId">Номер запису.</param>
    /// <param name="reason">Причина скасування для журналу.</param>
    /// <exception cref="DomainRuleException">Запису немає або він уже закритий.</exception>
    public void Cancel(int appointmentId, string reason) => _appointments.Cancel(appointmentId, reason);

    /// <summary>Змінює стан запису за подією (UC-03).</summary>
    /// <param name="appointmentId">Номер запису.</param>
    /// <param name="action">Подія.</param>
    /// <returns>Запис після зміни стану.</returns>
    /// <exception cref="DomainRuleException">Перехід заборонено або неявку позначають до початку прийому.</exception>
    public Appointment ChangeStatus(int appointmentId, StatusAction action)
    {
        Appointment appointment = _appointments.Get(appointmentId);
        switch (action)
        {
            case StatusAction.Confirm:
                _appointments.Confirm(appointmentId);
                break;
            case StatusAction.Complete:
                appointment.Complete();
                break;
            case StatusAction.MarkMissed:
                EnsureStarted(appointment);
                appointment.MarkMissed();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, "Невідома подія");
        }

        return appointment;
    }

    /// <summary>Додає послугу до відкритого запису (UC-06).</summary>
    /// <param name="appointmentId">Номер запису.</param>
    /// <param name="item">Перевірена барикадою послуга.</param>
    /// <returns>Сума до сплати після додавання, грн.</returns>
    /// <exception cref="DomainRuleException">Запису немає або він закритий.</exception>
    public decimal AddService(int appointmentId, VisitItem item)
    {
        _appointments.AddItem(appointmentId, item);
        return _appointments.TotalOf(_appointments.Get(appointmentId));
    }

    private Doctor DoctorOf(string code) =>
        _directory.FindDoctor(code)
            ?? throw new DomainRuleException("doctor.exists", $"Лікаря з кодом {code} немає в довіднику");

    // перший вільний слот лікаря, що не перетинається ні з його записами, ні з записами пацієнта
    private TimeSlot FindSlot(Doctor doctor, BookingRequest request, int? exceptId)
    {
        (DateOnly today, TimeOnly time) = LocalNow();
        if (request.VisitDate < today)
        {
            throw new DomainRuleException("appointment.date", $"Дата {request.VisitDate:dd.MM.yyyy} уже минула");
        }

        TimeOnly earliest = request.VisitDate == today ? time : doctor.WorkStart;
        List<TimeSlot> busy = _repository.GetAll()
            .Where(a => a.Id != exceptId && a.VisitDate == request.VisitDate && a.Status != VisitStatus.Cancelled)
            .Where(a => a.DoctorCode == doctor.Code || a.PatientId == request.PatientId)
            .Select(a => a.Slot)
            .ToList();
        SlotSearch search = new(doctor.WorkingHours, doctor.SlotMinutes, request.DurationMinutes, earliest);
        return FreeSlotFinder.FindFirst(search, busy)
            ?? throw new DomainRuleException("slot.free", $"У лікаря {doctor.Code} немає вільних {request.DurationMinutes} хв на {request.VisitDate:dd.MM.yyyy}");
    }

    private void EnsureStarted(Appointment appointment)
    {
        (DateOnly today, TimeOnly time) = LocalNow();
        bool started = today > appointment.VisitDate || (today == appointment.VisitDate && time >= appointment.StartTime);
        if (!started)
        {
            throw new DomainRuleException("appointment.missed", $"Запис № {appointment.Id}: неявку можна позначити лише після початку прийому");
        }
    }

    private (DateOnly Today, TimeOnly Time) LocalNow()
    {
        DateTimeOffset now = _clock.GetLocalNow();
        return (DateOnly.FromDateTime(now.DateTime), TimeOnly.FromDateTime(now.DateTime));
    }
}

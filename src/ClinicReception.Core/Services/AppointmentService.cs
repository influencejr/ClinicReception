using System.Globalization;
using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Errors;
using Microsoft.Extensions.Logging;

namespace ClinicReception.Core.Services;

/// <summary>
/// Прикладний сервіс реєстратури: записує пацієнтів і рахує вартість прийому.
/// Залежності отримує в конструкторі, сам їх не створює.
/// </summary>
public sealed class AppointmentService
{
    /// <summary>Сума, понад яку діє знижка за великий чек, грн.</summary>
    public const decimal LargeVisitFrom = 1000m;

    /// <summary>Фіксована знижка за великий чек, грн.</summary>
    public const decimal LargeVisitDiscount = 50m;

    private readonly IAppointmentRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;
    private readonly ILogger<AppointmentService> _log;

    /// <summary>Створює сервіс зі сховищем, політикою ціноутворення, сповіщувачем і журналом.</summary>
    /// <param name="repository">Сховище записів.</param>
    /// <param name="pricing">Політика ціноутворення.</param>
    /// <param name="notifier">Сповіщення пацієнта про запис.</param>
    /// <param name="log">Журнал операцій сервісу.</param>
    /// <exception cref="ArgumentNullException">Одну із залежностей не передано.</exception>
    public AppointmentService(
        IAppointmentRepository repository,
        IPricingPolicy pricing,
        INotifier notifier,
        ILogger<AppointmentService> log)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    /// <summary>Записує пацієнта на прийом: зберігає запис і сповіщає пацієнта.</summary>
    /// <param name="appointment">Новий запис.</param>
    /// <param name="patient">Пацієнт, якому належить запис.</param>
    /// <exception cref="ArgumentNullException">Запис або пацієнта не передано.</exception>
    /// <exception cref="ArgumentException">Запис належить іншому пацієнтові.</exception>
    public void Book(Appointment appointment, Patient patient)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(patient);
        using IDisposable? scope = _log.BeginScope("Appointment:{AppointmentId}", appointment.Id);
        _log.LogDebug(
            "Початок запису пацієнта {PatientId} до лікаря {DoctorCode} на {VisitDate:dd.MM.yyyy} {StartTime:HH:mm}",
            patient.Id,
            appointment.DoctorCode,
            appointment.VisitDate,
            appointment.StartTime);
        if (appointment.PatientId != patient.Id)
        {
            throw new ArgumentException($"Запис № {appointment.Id} належить іншому пацієнтові.", nameof(patient));
        }

        _repository.Add(appointment);
        decimal total = TotalOf(appointment);
        _log.LogInformation("Пацієнта {PatientId} записано, до сплати {Total:0.00} грн", patient.Id, total);

        if (patient.Phone is null)
        {
            _log.LogWarning("У пацієнта {PatientId} немає телефону, сповіщення лише листом", patient.Id);
        }

        string message = string.Create(
            CultureInfo.InvariantCulture,
            $"запис № {appointment.Id} на {appointment.VisitDate:dd.MM.yyyy} о {appointment.StartTime:HH:mm}, до сплати {total:0.00} грн");
        try
        {
            _notifier.Notify(patient.Email, message);
        }
        catch (IOException ex)
        {
            // запис уже збережено, тож збій пошти його не скасовує, лише потрапляє в журнал
            _log.LogError(ex, "Лист пацієнтові {PatientId} не надіслано, запис збережено", patient.Id);
        }
    }

    /// <summary>Додає перевірену послугу до збереженого запису.</summary>
    /// <param name="appointmentId">Номер запису.</param>
    /// <param name="item">Послуга, вже розібрана й перевірена на межі застосунку.</param>
    /// <exception cref="ArgumentNullException">Послугу не передано.</exception>
    /// <exception cref="DomainRuleException">Запису немає або його вже закрито.</exception>
    public void AddItem(int appointmentId, VisitItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using IDisposable? scope = _log.BeginScope("Appointment:{AppointmentId}", appointmentId);
        Get(appointmentId).AddItem(item);
        _log.LogDebug("Додано послугу {ServiceCode} x {Quantity}", item.ServiceCode, item.Quantity);
    }

    /// <summary>Підтверджує запис після дзвінка пацієнтові.</summary>
    /// <param name="appointmentId">Номер запису.</param>
    /// <exception cref="DomainRuleException">Запису немає, він порожній або вже не в стані Scheduled.</exception>
    public void Confirm(int appointmentId)
    {
        using IDisposable? scope = _log.BeginScope("Appointment:{AppointmentId}", appointmentId);
        Get(appointmentId).Confirm();
        _log.LogInformation("Запис підтверджено");
    }

    /// <summary>Повертає збережений запис за номером.</summary>
    /// <param name="appointmentId">Номер запису.</param>
    /// <returns>Знайдений запис.</returns>
    /// <exception cref="DomainRuleException">Запису з таким номером немає.</exception>
    public Appointment Get(int appointmentId) =>
        _repository.GetById(appointmentId)
            ?? throw new DomainRuleException("appointment.exists", $"Запис № {appointmentId} не знайдено");

    /// <summary>Скасовує запис до прийому.</summary>
    /// <param name="appointmentId">Номер запису.</param>
    /// <param name="reason">Причина скасування.</param>
    /// <exception cref="ArgumentException">Причину не вказано.</exception>
    /// <exception cref="DomainRuleException">Запису немає або його вже закрито.</exception>
    public void Cancel(int appointmentId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        using IDisposable? scope = _log.BeginScope("Appointment:{AppointmentId}", appointmentId);
        Get(appointmentId).Cancel();
        _log.LogInformation("Запис скасовано, причина: {Reason}", reason);
    }

    /// <summary>
    /// Обчислює вартість запису: ціни послуг за політикою, а якщо сума більша
    /// за <see cref="LargeVisitFrom"/>, мінус <see cref="LargeVisitDiscount"/>.
    /// </summary>
    /// <param name="appointment">Запис на прийом.</param>
    /// <returns>Сума до сплати, грн.</returns>
    /// <exception cref="ArgumentNullException">Запис не передано.</exception>
    public decimal TotalOf(Appointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        decimal total = 0m;
        foreach (VisitItem item in appointment.Items)
        {
            total += _pricing.PriceOf(item);
        }

        return total > LargeVisitFrom ? total - LargeVisitDiscount : total;
    }
}

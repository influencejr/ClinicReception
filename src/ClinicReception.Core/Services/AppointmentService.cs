using System.Globalization;
using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Services;

/// <summary>
/// Прикладний сервіс реєстратури: записує пацієнтів і рахує вартість прийому.
/// Залежності отримує в конструкторі, сам їх не створює.
/// </summary>
public sealed class AppointmentService
{
    private readonly IAppointmentRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;

    /// <summary>Створює сервіс зі сховищем, політикою ціноутворення та сповіщувачем.</summary>
    /// <param name="repository">Сховище записів.</param>
    /// <param name="pricing">Політика ціноутворення.</param>
    /// <param name="notifier">Сповіщення пацієнта про запис.</param>
    /// <exception cref="ArgumentNullException">Одну із залежностей не передано.</exception>
    public AppointmentService(IAppointmentRepository repository, IPricingPolicy pricing, INotifier notifier)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
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
        if (appointment.PatientId != patient.Id)
        {
            throw new ArgumentException($"Запис № {appointment.Id} належить іншому пацієнтові.", nameof(patient));
        }

        _repository.Add(appointment);
        string message = string.Create(
            CultureInfo.InvariantCulture,
            $"запис № {appointment.Id} на {appointment.VisitDate:dd.MM.yyyy} о {appointment.StartTime:HH:mm}, до сплати {TotalOf(appointment):0.00} грн");
        _notifier.Notify(patient.Email, message);
    }

    /// <summary>Обчислює вартість запису за політикою ціноутворення.</summary>
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

        return total;
    }
}

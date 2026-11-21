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
    /// <summary>Сума, понад яку діє знижка за великий чек, грн.</summary>
    public const decimal LargeVisitFrom = 1000m;

    /// <summary>Фіксована знижка за великий чек, грн.</summary>
    public const decimal LargeVisitDiscount = 50m;

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

    /// <summary>Додає послугу до збереженого запису.</summary>
    /// <param name="appointmentId">Номер запису.</param>
    /// <param name="serviceCode">Код послуги.</param>
    /// <param name="quantity">Кількість, як її ввели в консолі.</param>
    /// <param name="price">Ціна за одиницю, як її ввели в консолі.</param>
    /// <param name="minutes">Тривалість однієї одиниці, хв, як її ввели в консолі.</param>
    public void AddItem(int appointmentId, string serviceCode, string quantity, string price, string minutes)
    {
        Appointment appointment = _repository.GetById(appointmentId)!;
        int q = int.Parse(quantity, CultureInfo.CurrentCulture);
        decimal p = decimal.Parse(price, CultureInfo.CurrentCulture);
        int m = int.Parse(minutes, CultureInfo.CurrentCulture);
        appointment.AddItem(new VisitItem(serviceCode, q, p, m));
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

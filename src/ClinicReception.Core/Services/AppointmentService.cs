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

    /// <summary>Створює сервіс зі сховищем і політикою ціноутворення.</summary>
    /// <param name="repository">Сховище записів.</param>
    /// <param name="pricing">Політика ціноутворення.</param>
    /// <exception cref="ArgumentNullException">Одну із залежностей не передано.</exception>
    public AppointmentService(IAppointmentRepository repository, IPricingPolicy pricing)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
    }

    /// <summary>Записує пацієнта на прийом: зберігає запис у сховищі.</summary>
    /// <param name="appointment">Новий запис.</param>
    /// <exception cref="ArgumentNullException">Запис не передано.</exception>
    public void Book(Appointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        _repository.Add(appointment);
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

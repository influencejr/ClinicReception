using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Domain;

namespace ClinicReception.Infrastructure.Storage;

/// <summary>Сховище записів у пам'яті: словник за номером запису.</summary>
public sealed class InMemoryAppointmentRepository : IAppointmentRepository
{
    private readonly Dictionary<int, Appointment> _appointments = new();

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Запис не передано.</exception>
    /// <exception cref="InvalidOperationException">Запис із таким номером уже є.</exception>
    public void Add(Appointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        if (!_appointments.TryAdd(appointment.Id, appointment))
        {
            throw new InvalidOperationException($"Запис № {appointment.Id} уже збережено.");
        }
    }

    /// <inheritdoc/>
    public Appointment? GetById(int id) => _appointments.GetValueOrDefault(id);

    /// <inheritdoc/>
    public IReadOnlyList<Appointment> GetAll() => _appointments.Values.ToList();
}

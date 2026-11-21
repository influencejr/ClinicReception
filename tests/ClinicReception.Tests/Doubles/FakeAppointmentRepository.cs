using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Domain;

namespace ClinicReception.Tests.Doubles;

// фейк: спрощене, але робоче сховище на словнику в пам'яті
internal sealed class FakeAppointmentRepository : IAppointmentRepository
{
    private readonly Dictionary<int, Appointment> _items = new();

    public int AddCalls { get; private set; }

    public void Add(Appointment appointment)
    {
        _items[appointment.Id] = appointment;
        AddCalls++;
    }

    public Appointment? GetById(int id) =>
        _items.TryGetValue(id, out Appointment? found)
            ? found
            : null;

    public IReadOnlyList<Appointment> GetAll() => _items.Values.ToList();
}

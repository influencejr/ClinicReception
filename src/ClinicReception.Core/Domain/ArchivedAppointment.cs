namespace ClinicReception.Core.Domain;

/// <summary>
/// Архівний запис. Не є записом на прийом (не успадковує <see cref="Appointment"/>),
/// а лише показує його дані: методу додавання послуг тут немає взагалі.
/// </summary>
public sealed class ArchivedAppointment
{
    private readonly Appointment _appointment;

    /// <summary>Переносить закритий запис в архів.</summary>
    /// <param name="appointment">Закритий запис на прийом.</param>
    /// <param name="archivedAt">Дата перенесення в архів.</param>
    /// <exception cref="ArgumentNullException">Запис не передано.</exception>
    /// <exception cref="InvalidOperationException">Запис ще не закрито.</exception>
    public ArchivedAppointment(Appointment appointment, DateOnly archivedAt)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        if (!appointment.IsClosed)
        {
            throw new InvalidOperationException($"Запис № {appointment.Id} ще відкритий ({appointment.Status}).");
        }

        _appointment = appointment;
        ArchivedAt = archivedAt;
    }

    /// <summary>Дата перенесення в архів.</summary>
    public DateOnly ArchivedAt { get; }

    /// <summary>Номер запису.</summary>
    public int Id => _appointment.Id;

    /// <summary>Остаточний стан запису.</summary>
    public VisitStatus Status => _appointment.Status;

    /// <summary>Дата прийому.</summary>
    public DateOnly VisitDate => _appointment.VisitDate;

    /// <summary>Послуги запису лише для читання.</summary>
    public IReadOnlyList<VisitItem> Items => _appointment.Items;
}

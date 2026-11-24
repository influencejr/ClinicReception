using ClinicReception.Core.Domain;

namespace ClinicReception.Infrastructure.Storage;

/// <summary>Запис на прийом у файлі appointments.json.</summary>
public sealed record StoredAppointment
{
    /// <summary>Номер запису.</summary>
    public required int Id { get; init; }

    /// <summary>Номер пацієнта.</summary>
    public required int PatientId { get; init; }

    /// <summary>Код лікаря.</summary>
    public required string DoctorCode { get; init; }

    /// <summary>Дата прийому.</summary>
    public required DateOnly VisitDate { get; init; }

    /// <summary>Початок прийому.</summary>
    public required TimeOnly StartTime { get; init; }

    /// <summary>Тривалість, хв.</summary>
    public required int DurationMinutes { get; init; }

    /// <summary>Стан запису.</summary>
    public required VisitStatus Status { get; init; }

    /// <summary>Послуги.</summary>
    public required IReadOnlyList<StoredItem> Items { get; init; }
}

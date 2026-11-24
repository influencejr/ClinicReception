using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Services;

/// <summary>Критерії пошуку записів (UC-04). Незаповнений критерій не обмежує результат.</summary>
public sealed record AppointmentQuery
{
    /// <summary>Номер пацієнта.</summary>
    public int? PatientId { get; init; }

    /// <summary>Код лікаря.</summary>
    public string? DoctorCode { get; init; }

    /// <summary>Перша дата періоду, включно.</summary>
    public DateOnly? From { get; init; }

    /// <summary>Остання дата періоду, включно.</summary>
    public DateOnly? To { get; init; }

    /// <summary>Стан запису.</summary>
    public VisitStatus? Status { get; init; }
}

namespace ClinicReception.Infrastructure.Storage;

/// <summary>Пацієнт у файлі patients.json.</summary>
public sealed record StoredPatient
{
    /// <summary>Номер пацієнта.</summary>
    public required int Id { get; init; }

    /// <summary>Прізвище, ім'я та по батькові.</summary>
    public required string FullName { get; init; }

    /// <summary>Електронна пошта.</summary>
    public required string Email { get; init; }

    /// <summary>Телефон або null.</summary>
    public string? Phone { get; init; }

    /// <summary>Постійний пацієнт.</summary>
    public bool IsRegular { get; init; }

    /// <summary>Є страховий поліс.</summary>
    public bool HasInsurance { get; init; }
}

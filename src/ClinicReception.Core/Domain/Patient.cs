namespace ClinicReception.Core.Domain;

/// <summary>Пацієнт клініки.</summary>
public sealed class Patient
{
    /// <summary>Ідентифікатор пацієнта.</summary>
    public int Id { get; init; }

    /// <summary>Прізвище, ім’я та по батькові пацієнта.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Контактний телефон пацієнта.</summary>
    public string Phone { get; init; } = string.Empty;

    /// <summary>Ознака пацієнта зі страховим полісом.</summary>
    public bool HasInsurance { get; init; }
}

namespace ClinicReception.Core.Domain;

/// <summary>Пацієнт клініки.</summary>
public sealed class Patient
{
    public int Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public bool HasInsurance { get; init; }
}

namespace ClinicReception.Core.Domain;

/// <summary>Лікар, до якого записують пацієнтів.</summary>
public sealed class Doctor
{
    public string Code { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Specialty { get; init; } = string.Empty;
    public decimal ConsultationFee { get; init; }
    public int SlotMinutes { get; init; }
}

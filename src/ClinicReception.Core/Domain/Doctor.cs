namespace ClinicReception.Core.Domain;

/// <summary>Лікар, до якого записують пацієнтів.</summary>
public sealed class Doctor
{
    /// <summary>Код лікаря в довіднику клініки.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Прізвище, ім’я та по батькові лікаря.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Спеціальність лікаря.</summary>
    public string Specialty { get; init; } = string.Empty;

    /// <summary>Вартість консультації, грн.</summary>
    public decimal ConsultationFee { get; init; }

    /// <summary>Тривалість одного слоту прийому, хв.</summary>
    public int SlotMinutes { get; init; }
}

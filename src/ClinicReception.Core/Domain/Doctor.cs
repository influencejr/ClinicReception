namespace ClinicReception.Core.Domain;

/// <summary>
/// Лікар, до якого записують пацієнтів (довідник об'єктів обліку).
/// </summary>
public sealed record Doctor
{
    /// <summary>Створює запис довідника лікарів, перевіряючи його інваріанти.</summary>
    /// <param name="code">Код лікаря в довіднику клініки.</param>
    /// <param name="fullName">Прізвище, ім'я та по батькові лікаря.</param>
    /// <param name="consultationFee">Вартість консультації, грн, більша за нуль.</param>
    /// <param name="slotMinutes">Тривалість одного слоту прийому, хв, більша за нуль.</param>
    /// <exception cref="ArgumentException">Код або ім'я порожні.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Вартість чи тривалість слоту не додатні.</exception>
    public Doctor(string code, string fullName, decimal consultationFee, int slotMinutes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(consultationFee);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(slotMinutes);

        Code = code;
        FullName = fullName;
        ConsultationFee = consultationFee;
        SlotMinutes = slotMinutes;
    }

    /// <summary>Код лікаря в довіднику клініки.</summary>
    public string Code { get; }

    /// <summary>Прізвище, ім'я та по батькові лікаря.</summary>
    public string FullName { get; }

    /// <summary>Спеціальність лікаря; порожня, якщо не вказана.</summary>
    public string Specialty { get; init; } = string.Empty;

    /// <summary>Вартість консультації, грн.</summary>
    public decimal ConsultationFee { get; }

    /// <summary>Тривалість одного слоту прийому, хв.</summary>
    public int SlotMinutes { get; }
}

namespace ClinicReception.Infrastructure.Storage;

/// <summary>Лікар у файлі doctors.json. Окремий тип, щоб домен не залежав від формату JSON.</summary>
public sealed record StoredDoctor
{
    /// <summary>Код лікаря.</summary>
    public required string Code { get; init; }

    /// <summary>Прізвище, ім'я та по батькові.</summary>
    public required string FullName { get; init; }

    /// <summary>Спеціальність.</summary>
    public required string Specialty { get; init; }

    /// <summary>Вартість консультації, грн.</summary>
    public required decimal ConsultationFee { get; init; }

    /// <summary>Крок сітки розкладу, хв.</summary>
    public required int SlotMinutes { get; init; }

    /// <summary>Початок робочого дня.</summary>
    public required TimeOnly WorkStart { get; init; }

    /// <summary>Кінець робочого дня.</summary>
    public required TimeOnly WorkEnd { get; init; }
}

namespace ClinicReception.Core.Domain;

/// <summary>Сталі розкладу прийому (X-04).</summary>
public static class ScheduleRules
{
    /// <summary>Тривалість прийому за замовчуванням, хв.</summary>
    public const int DefaultVisitMinutes = 20;

    /// <summary>Код послуги-консультації, яку отримує кожен новий запис.</summary>
    public const string ConsultationCode = "CONS";

    /// <summary>Початок робочого дня лікаря за замовчуванням.</summary>
    public static readonly TimeOnly DefaultWorkStart = new(8, 0);

    /// <summary>Кінець робочого дня лікаря за замовчуванням; о цій хвилині прийом уже не триває.</summary>
    public static readonly TimeOnly DefaultWorkEnd = new(14, 0);
}

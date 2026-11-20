using System.Globalization;

namespace ClinicReception.Core.Domain;

/// <summary>
/// Візит пацієнта до клініки з переліком наданих послуг.
/// </summary>
public class Visit
{
    /// <summary>Стан: візит заплановано.</summary>
    public const int StatusScheduled = 0;

    /// <summary>Стан: пацієнт підтвердив візит.</summary>
    public const int StatusConfirmed = 1;

    /// <summary>Стан: візит завершено.</summary>
    public const int StatusCompleted = 2;

    /// <summary>Стан: візит скасовано.</summary>
    public const int StatusCancelled = 3;

    /// <summary>Ставка податку на додану вартість.</summary>
    private const decimal VatRate = 0.2m;

    /// <summary>Поріг суми для знижки застрахованому пацієнту, грн.</summary>
    private const decimal InsuranceDiscountThreshold = 1000m;

    /// <summary>Частка знижки застрахованому пацієнту.</summary>
    private const decimal InsuranceDiscountRate = 0.10m;

    /// <summary>Поріг суми для знижки за великий чек, грн.</summary>
    private const decimal LargeVisitThreshold = 5000m;

    /// <summary>Частка знижки за великий чек.</summary>
    private const decimal LargeVisitDiscountRate = 0.15m;

    /// <summary>Кількість рядків послуг, понад яку діє фіксована знижка.</summary>
    private const int BulkLineCount = 10;

    /// <summary>Фіксована знижка за велику кількість послуг, грн.</summary>
    private const decimal BulkDiscountAmount = 100m;

    /// <summary>Межа кількості рядків послуг у візиті (не включно).</summary>
    private const int MaxLineCount = 100;

    /// <summary>Найкоротше допустиме прізвище пацієнта, символів.</summary>
    private const int MinPatientNameLength = 3;

    /// <summary>Дозволені переходи стану: пара «поточний стан — новий стан».</summary>
    private static readonly HashSet<(int From, int To)> AllowedTransitions = new()
    {
        (StatusScheduled, StatusConfirmed),
        (StatusConfirmed, StatusCompleted),
        (StatusScheduled, StatusCancelled),
    };

    private readonly List<string[]> _lines = new List<string[]>();

    /// <summary>
    /// Створює візит у початковому стані.
    /// </summary>
    /// <param name="id">Номер візиту, наприклад «V-1001».</param>
    /// <param name="patientName">Прізвище пацієнта.</param>
    public Visit(string id, string patientName)
    {
        Id = id;
        PatientName = patientName;
        CreatedAt = DateTimeOffset.Now;
    }

    /// <summary>
    /// Номер візиту.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Прізвище пацієнта, записаного на візит.
    /// </summary>
    public string PatientName { get; private set; }

    /// <summary>
    /// Код стану візиту.
    /// </summary>
    public int Status { get; private set; } = StatusScheduled;

    /// <summary>
    /// Момент створення візиту.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    private bool HasId => !string.IsNullOrEmpty(Id);

    private bool HasPatientName =>
        PatientName is not null && PatientName.Length >= MinPatientNameLength;

    private bool HasAllowedLineCount => _lines.Count > 0 && _lines.Count < MaxLineCount;

    private bool HasKnownStatus => Status >= StatusScheduled && Status <= StatusCancelled;

    /// <summary>
    /// Шукає візит за номером.
    /// </summary>
    /// <param name="visits">Список візитів, у якому виконується пошук.</param>
    /// <param name="visitId">Номер шуканого візиту.</param>
    /// <returns>Знайдений візит або null, якщо візиту з таким номером немає.</returns>
    public static Visit? FindById(List<Visit> visits, string visitId)
    {
        for (int i = 0; i < visits.Count; i++)
        {
            if (visits[i].Id == visitId)
            {
                return visits[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Додає до візиту рядок із наданою послугою.
    /// </summary>
    /// <param name="serviceCode">Код послуги за прайсом клініки.</param>
    /// <param name="quantity">Кількість одиниць послуги.</param>
    /// <param name="unitPrice">Ціна за одиницю без ПДВ, грн.</param>
    public void AddLine(string serviceCode, int quantity, decimal unitPrice)
    {
        string[] line = new string[3];
        line[0] = serviceCode;
        line[1] = quantity.ToString(CultureInfo.CurrentCulture);
        line[2] = unitPrice.ToString(CultureInfo.CurrentCulture);
        _lines.Add(line);
    }

    /// <summary>
    /// Обчислює вартість візиту з урахуванням знижок і ПДВ.
    /// </summary>
    /// <param name="hasInsurance">Ознака пацієнта зі страховим полісом.</param>
    /// <returns>Сума до сплати, грн, заокруглена до двох знаків.</returns>
    public decimal CalculateTotal(bool hasInsurance)
    {
        decimal total = DiscountedOf(SubtotalOf(), hasInsurance);
        if (_lines.Count > BulkLineCount)
        {
            total -= BulkDiscountAmount;
        }

        total = Math.Max(total, 0m);

        // ПДВ нараховується на суму вже після всіх знижок
        total += total * VatRate;
        return Math.Round(total, 2);
    }

    /// <summary>
    /// Змінює стан візиту, якщо перехід дозволений правилами реєстратури.
    /// </summary>
    /// <param name="newStatus">Код цільового стану візиту (константа Status…).</param>
    /// <returns>
    /// true, якщо перехід виконано; false, якщо він заборонений
    /// і стан візиту лишився попереднім.
    /// </returns>
    public bool TryChangeStatus(int newStatus)
    {
        if (!AllowedTransitions.Contains((Status, newStatus)))
        {
            return false;
        }

        Status = newStatus;
        return true;
    }

    /// <summary>
    /// Перевіряє, чи заповнено візит достатньо для обліку.
    /// </summary>
    /// <returns>true, якщо номер, пацієнт, послуги й стан коректні.</returns>
    public bool IsValid() => HasId && HasPatientName && HasAllowedLineCount && HasKnownStatus;

    /// <summary>
    /// Формує текстовий звіт про послуги візиту.
    /// </summary>
    /// <returns>Рядки з послугами та підсумок без страхової знижки.</returns>
    public string BuildReport()
    {
        string report = string.Empty;
        for (int i = 0; i < _lines.Count; i++)
        {
            int quantity = int.Parse(_lines[i][1], CultureInfo.CurrentCulture);
            decimal unitPrice = decimal.Parse(_lines[i][2], CultureInfo.CurrentCulture);
            decimal lineAmount = quantity * unitPrice;
            report = report
                + "Послуга: " + _lines[i][0]
                + "; кількість: " + _lines[i][1]
                + "; ціна: " + _lines[i][2]
                + "; сума: " + lineAmount + "\n";
        }

        report = report + "Разом: " + CalculateTotal(false) + "\n";
        return report;
    }

    /// <summary>Застосовує одну зі знижок: страхову або за великий чек.</summary>
    /// <param name="total">Сума послуг без знижок, грн.</param>
    /// <param name="hasInsurance">Ознака пацієнта зі страховим полісом.</param>
    /// <returns>Сума після знижки, грн.</returns>
    private static decimal DiscountedOf(decimal total, bool hasInsurance)
    {
        // знижки не підсумовуються: страхова має пріоритет над знижкою за великий чек
        if (hasInsurance && total > InsuranceDiscountThreshold)
        {
            return total * (1m - InsuranceDiscountRate);
        }

        if (total > LargeVisitThreshold)
        {
            return total * (1m - LargeVisitDiscountRate);
        }

        return total;
    }

    /// <summary>Обчислює суму послуг візиту без знижок і ПДВ.</summary>
    /// <returns>Сума добутків кількості на ціну за всіма рядками, грн.</returns>
    private decimal SubtotalOf()
    {
        decimal subtotal = 0;
        foreach (string[] line in _lines)
        {
            int quantity = int.Parse(line[1], CultureInfo.CurrentCulture);
            decimal unitPrice = decimal.Parse(line[2], CultureInfo.CurrentCulture);
            subtotal += quantity * unitPrice;
        }

        return subtotal;
    }
}

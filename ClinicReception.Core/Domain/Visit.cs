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

    /// <summary>
    /// Шукає візит за номером.
    /// </summary>
    /// <param name="visits">Список візитів, у якому виконується пошук.</param>
    /// <param name="visitId">Номер шуканого візиту.</param>
    /// <returns>Знайдений візит або null, якщо візиту з таким номером немає.</returns>
    public static Visit FindById(List<Visit> visits, string visitId)
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
        line[1] = quantity.ToString();
        line[2] = unitPrice.ToString();
        _lines.Add(line);
    }

    /// <summary>
    /// Обчислює вартість візиту з урахуванням знижок і ПДВ.
    /// </summary>
    /// <param name="hasInsurance">Ознака пацієнта зі страховим полісом.</param>
    /// <returns>Сума до сплати, грн, заокруглена до двох знаків.</returns>
    public decimal CalculateTotal(bool hasInsurance)
    {
        decimal total = 0;
        int lineCount = 0;
        for (int i = 0; i < _lines.Count; i++)
        {
            int quantity = int.Parse(_lines[i][1]);
            decimal unitPrice = decimal.Parse(_lines[i][2]);
            total += quantity * unitPrice;
            lineCount++;
        }

        // знижки не підсумовуються: страхова має пріоритет над знижкою за великий чек
        if (hasInsurance && total > InsuranceDiscountThreshold)
        {
            total *= 1m - InsuranceDiscountRate;
        }
        else if (total > LargeVisitThreshold)
        {
            total *= 1m - LargeVisitDiscountRate;
        }

        if (lineCount > BulkLineCount)
        {
            total -= BulkDiscountAmount;
        }

        if (total < 0)
        {
            total = 0;
        }

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
        if (Status == StatusScheduled && newStatus == StatusConfirmed)
        {
            Status = StatusConfirmed;
            return true;
        }

        if (Status == StatusConfirmed && newStatus == StatusCompleted)
        {
            Status = StatusCompleted;
            return true;
        }

        if (Status == StatusScheduled && newStatus == StatusCancelled)
        {
            Status = StatusCancelled;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Перевіряє, чи заповнено візит достатньо для обліку.
    /// </summary>
    /// <returns>true, якщо номер, пацієнт, послуги й стан коректні.</returns>
    public bool IsValid()
    {
        if (Id is not null && Id.Length > 0
            && PatientName is not null
            && PatientName.Length >= MinPatientNameLength
            && _lines.Count > 0
            && _lines.Count < MaxLineCount
            && Status >= StatusScheduled
            && Status <= StatusCancelled)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Формує текстовий звіт про послуги візиту.
    /// </summary>
    /// <returns>Рядки з послугами та підсумок без страхової знижки.</returns>
    public string BuildReport()
    {
        string report = string.Empty;
        for (int i = 0; i < _lines.Count; i++)
        {
            decimal lineAmount = int.Parse(_lines[i][1]) * decimal.Parse(_lines[i][2]);
            report = report
                + "Послуга: " + _lines[i][0]
                + "; кількість: " + _lines[i][1]
                + "; ціна: " + _lines[i][2]
                + "; сума: " + lineAmount + "\n";
        }

        report = report + "Разом: " + CalculateTotal(false) + "\n";
        return report;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicReception.Core.Domain;

/// <summary>
/// Візит пацієнта до клініки з переліком наданих послуг.
/// </summary>
public class Visit
{
    private readonly List<string[]> _lines = new List<string[]>();

    // public string prim;   // примітка, поки не треба

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
    public int Status { get; private set; } = 0; // 0-новий,1-підтв,2-заверш,3-скасов

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
            total = total + (quantity * unitPrice);
            lineCount = lineCount + 1;
        }

        // if (sum1 > 500) { sum1 = sum1 - 50; } // стара знижка
        // знижки не підсумовуються: страхова має пріоритет над знижкою за великий чек
        if (hasInsurance == true && total > 1000)
        {
            total = total * 0.9m;
        }
        else if (total > 5000)
        {
            total = total * 0.85m;
        }
        else
        {
            total = total;
        }

        if (lineCount > 10)
        {
            total = total - 100;
        }

        if (total < 0)
        {
            total = 0;
        }

        // ПДВ нараховується на суму вже після всіх знижок
        total = total + (total * 0.2m);
        return Math.Round(total, 2);
    }

    /// <summary>
    /// Змінює стан візиту, якщо перехід дозволений правилами реєстратури.
    /// </summary>
    /// <param name="newStatus">Код цільового стану візиту.</param>
    /// <returns>
    /// true, якщо перехід виконано; false, якщо він заборонений
    /// і стан візиту лишився попереднім.
    /// </returns>
    public bool TryChangeStatus(int newStatus)
    {
        if (Status == 0 && newStatus == 1)
        {
            Status = 1;
            return true;
        }

        if (Status == 1 && newStatus == 2)
        {
            Status = 2;
            return true;
        }

        if (Status == 0 && newStatus == 3)
        {
            Status = 3;
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
            && PatientName.Length > 2
            && _lines.Count > 0
            && _lines.Count < 100
            && Status >= 0
            && Status <= 3)
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

using System.Globalization;
using System.Text;
using ClinicReception.Core.Billing;
using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Reporting;

/// <summary>Формування текстового звіту про розрахований візит.</summary>
public static class VisitReport
{
    /// <summary>
    /// Будує текст звіту. Чиста функція: числа форматуються інваріантною культурою,
    /// тому результат не залежить від налаштувань операційної системи.
    /// </summary>
    /// <param name="request">Запит, за яким рахувався візит.</param>
    /// <param name="result">Результат розрахунку.</param>
    /// <returns>Багаторядковий текст звіту.</returns>
    public static string TextOf(TotalRequest request, TotalResult result)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(result);
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();
        text.AppendLine(culture, $"Візит № {request.VisitId}");
        text.AppendLine(culture, $"Пацієнт: {request.Patient.Name}");
        foreach (VisitItem item in request.Items)
        {
            text.AppendLine(culture, $"{item.ServiceCode} x {item.Quantity}");
        }

        text.AppendLine(culture, $"Разом: {result.Total:F2}");
        text.AppendLine(culture, $"Стан: {result.Status}");
        return text.ToString();
    }
}

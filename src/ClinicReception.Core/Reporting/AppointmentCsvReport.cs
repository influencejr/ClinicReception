using System.Globalization;
using System.Text;
using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Reporting;

/// <summary>
/// CSV-звіт про записи на прийом. Лише форматує текст: на диск не пише
/// й нічого не друкує, це вирішує той, хто викликає.
/// </summary>
public static class AppointmentCsvReport
{
    /// <summary>Рядок заголовків звіту.</summary>
    public const string Header = "id;date;status;subtotal";

    /// <summary>Будує CSV-текст: заголовок і по рядку на кожен запис.</summary>
    /// <param name="appointments">Записи на прийом.</param>
    /// <returns>Текст звіту з роздільником «;» та інваріантною культурою.</returns>
    /// <exception cref="ArgumentNullException">Перелік записів не передано.</exception>
    public static string BuildCsv(IEnumerable<Appointment> appointments)
    {
        ArgumentNullException.ThrowIfNull(appointments);
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();
        text.AppendLine(Header);
        foreach (Appointment appointment in appointments)
        {
            text.AppendLine(culture, $"{appointment.Id};{appointment.VisitDate:yyyy-MM-dd};{appointment.Status};{appointment.Subtotal:F2}");
        }

        return text.ToString();
    }
}

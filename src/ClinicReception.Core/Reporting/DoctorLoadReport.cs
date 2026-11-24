using System.Globalization;
using System.Text;

namespace ClinicReception.Core.Reporting;

/// <summary>Текст звіту «завантаження лікаря за день». Чиста функція, нічого не друкує.</summary>
public static class DoctorLoadReport
{
    /// <summary>Формує текст звіту. Стану не змінює.</summary>
    /// <param name="load">Дані звіту.</param>
    /// <returns>Багаторядковий текст; числа й дати в інваріантній культурі.</returns>
    /// <exception cref="ArgumentNullException">Дані не передано.</exception>
    public static string TextOf(DoctorDayLoad load)
    {
        ArgumentNullException.ThrowIfNull(load);
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();
        text.AppendLine(culture, $"Завантаження лікаря {load.Doctor.Code} ({load.Doctor.FullName}) на {load.VisitDate:dd.MM.yyyy}");
        text.AppendLine(culture, $"Робочий час {load.Doctor.WorkStart:HH:mm}-{load.Doctor.WorkEnd:HH:mm}, {load.WorkingMinutes} хв");
        if (load.Visits.Count == 0)
        {
            text.AppendLine("  записів немає");
        }

        foreach (LoadRow row in load.Visits)
        {
            text.AppendLine(culture, $"  {row.Slot.StartTime:HH:mm}-{row.Slot.EndTime:HH:mm}  № {row.AppointmentId,-4} {row.PatientName,-32} {row.Status}");
        }

        text.AppendLine(culture, $"Зайнято {load.BusyMinutes} хв з {load.WorkingMinutes} ({load.LoadPercent:0.0} %)");
        string windows = string.Join(", ", load.FreeWindows.Select(w => string.Create(culture, $"{w.StartTime:HH:mm}-{w.EndTime:HH:mm}")));
        text.Append("Вільні проміжки: ").Append(windows.Length == 0 ? "немає" : windows);
        return text.ToString();
    }
}

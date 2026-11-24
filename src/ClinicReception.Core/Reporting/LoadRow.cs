using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Reporting;

/// <summary>Рядок звіту завантаження: прийом, пацієнт і стан.</summary>
/// <param name="AppointmentId">Номер запису.</param>
/// <param name="Slot">Інтервал прийому.</param>
/// <param name="PatientName">Ім'я пацієнта або «?», якщо його немає в довіднику.</param>
/// <param name="Status">Стан запису.</param>
public sealed record LoadRow(int AppointmentId, TimeSlot Slot, string PatientName, VisitStatus Status);

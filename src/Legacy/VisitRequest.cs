namespace ClinicReception.Legacy;

/// <summary>
/// Дані запису на прийом, які раніше передавалися в Handle одинадцятьма
/// параметрами. Невикористані createdAt і окремий patientId не перенесено.
/// </summary>
/// <param name="AppointmentId">Номер запису.</param>
/// <param name="Patient">Пацієнт: ім'я, пошта, вид, кількість візитів.</param>
/// <param name="Items">Послуги прийому; null означає, що список не передали.</param>
/// <param name="State">Стан запису.</param>
/// <param name="Currency">Код валюти для звіту.</param>
/// <param name="SendMail">Чи надсилати пацієнтові лист.</param>
public sealed record VisitRequest(
    int AppointmentId,
    Patient Patient,
    IReadOnlyList<ServiceLine>? Items,
    AppointmentState State,
    string Currency,
    bool SendMail);

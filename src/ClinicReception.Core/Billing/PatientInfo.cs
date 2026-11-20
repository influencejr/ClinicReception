namespace ClinicReception.Core.Billing;

/// <summary>Дані пацієнта, потрібні для розрахунку вартості візиту.</summary>
/// <param name="Name">Прізвище пацієнта.</param>
/// <param name="Email">Пошта пацієнта для надсилання рахунку.</param>
/// <param name="IsRegular">Ознака постійного пацієнта, що дає знижку.</param>
public sealed record PatientInfo(
    string Name,
    string Email,
    bool IsRegular);

using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Billing;

/// <summary>Усі вхідні дані розрахунку вартості одного візиту.</summary>
/// <param name="VisitId">Номер візиту.</param>
/// <param name="Patient">Пацієнт, який оплачує візит.</param>
/// <param name="Items">Послуги візиту.</param>
/// <param name="VisitDate">Дата прийому.</param>
/// <param name="PromoCode">Промокод або null, якщо його не вказано.</param>
/// <param name="Status">Поточний стан візиту.</param>
/// <param name="RegistrationFee">Реєстраційний збір за прайсом, грн.</param>
public sealed record TotalRequest(
    int VisitId,
    PatientInfo Patient,
    IReadOnlyList<VisitItem> Items,
    DateOnly VisitDate,
    string? PromoCode,
    VisitStatus Status,
    decimal RegistrationFee);

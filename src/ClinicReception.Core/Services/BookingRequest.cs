namespace ClinicReception.Core.Services;

/// <summary>Запит на запис (UC-01): хто, до якого лікаря, на яку дату і на скільки хвилин.</summary>
/// <param name="PatientId">Номер пацієнта в довіднику.</param>
/// <param name="DoctorCode">Код лікаря в довіднику.</param>
/// <param name="VisitDate">Бажана дата прийому.</param>
/// <param name="DurationMinutes">Тривалість прийому, хв.</param>
public sealed record BookingRequest(int PatientId, string DoctorCode, DateOnly VisitDate, int DurationMinutes);

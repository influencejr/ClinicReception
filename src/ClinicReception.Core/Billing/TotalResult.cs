using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Billing;

/// <summary>Результат розрахунку вартості візиту.</summary>
/// <param name="Total">Сума до сплати, грн, заокруглена до копійок.</param>
/// <param name="Status">Стан, у який переходить візит після розрахунку.</param>
public sealed record TotalResult(
    decimal Total,
    VisitStatus Status);

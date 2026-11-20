using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Abstractions;

/// <summary>Правило ціноутворення для однієї послуги.</summary>
public interface IPricingPolicy
{
    /// <summary>Обчислює ціну рядка послуги за цим правилом.</summary>
    /// <param name="item">Послуга запису.</param>
    /// <returns>Ціна рядка до сплати, грн, заокруглена до копійок.</returns>
    decimal PriceOf(VisitItem item);
}

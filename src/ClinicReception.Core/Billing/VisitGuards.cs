using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Billing;

/// <summary>
/// Перевірки передумов для розрахунку вартості візиту.
/// Кожен метод або нічого не робить, або кидає виняток із назвою порушеного параметра.
/// </summary>
internal static class VisitGuards
{
    /// <summary>Перевіряє заголовок візиту: номер, пацієнта та його пошту.</summary>
    /// <param name="visitId">Номер візиту, більший за нуль.</param>
    /// <param name="patientName">Прізвище пацієнта, непорожнє.</param>
    /// <param name="patientEmail">Пошта пацієнта, містить символ @.</param>
    /// <exception cref="ArgumentOutOfRangeException">Номер візиту не додатний.</exception>
    /// <exception cref="ArgumentException">Ім'я або пошта порожні, пошта без @.</exception>
    public static void EnsureHeaderValid(int visitId, string patientName, string patientEmail)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(visitId);
        ArgumentException.ThrowIfNullOrWhiteSpace(patientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(patientEmail);
        if (!patientEmail.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Пошта пацієнта має містити символ @.", nameof(patientEmail));
        }
    }

    /// <summary>Перевіряє одну послугу візиту.</summary>
    /// <param name="item">Послуга з непорожнім кодом, додатною кількістю й невід'ємною ціною.</param>
    /// <exception cref="ArgumentNullException">Послугу не передано.</exception>
    /// <exception cref="ArgumentException">Код послуги порожній.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Кількість не додатна або ціна від'ємна.</exception>
    public static void EnsureItemValid(VisitItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.ServiceCode);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(item.Quantity, nameof(item.Quantity));
        ArgumentOutOfRangeException.ThrowIfNegative(item.UnitPrice, nameof(item.UnitPrice));
    }

    /// <summary>Перевіряє перелік послуг візиту.</summary>
    /// <param name="items">Непорожній перелік коректних послуг.</param>
    /// <exception cref="ArgumentNullException">Перелік не передано.</exception>
    /// <exception cref="ArgumentException">Перелік порожній.</exception>
    public static void EnsureItemsValid(IReadOnlyList<VisitItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
        {
            throw new ArgumentException("Візит не містить жодної послуги.", nameof(items));
        }

        foreach (VisitItem item in items)
        {
            EnsureItemValid(item);
        }
    }
}

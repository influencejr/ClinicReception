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

    /// <summary>Перевіряє перелік послуг візиту.</summary>
    /// <remarks>
    /// Кожну послугу окремо не перевіряємо: <see cref="VisitItem"/> перевіряє
    /// свої інваріанти в конструкторі, тож некоректна послуга не може існувати.
    /// </remarks>
    /// <param name="items">Непорожній перелік послуг.</param>
    /// <exception cref="ArgumentNullException">Перелік не передано або в ньому є null.</exception>
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
            ArgumentNullException.ThrowIfNull(item, nameof(items));
        }
    }

    /// <summary>Перевіряє запит на розрахунок цілком: пацієнта, послуги та збір.</summary>
    /// <param name="request">Запит на розрахунок вартості візиту.</param>
    /// <exception cref="ArgumentNullException">Запит або пацієнта не передано.</exception>
    /// <exception cref="ArgumentException">Заголовок чи послуги некоректні.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Номер, кількість, ціна чи збір поза межами.</exception>
    public static void EnsureRequestValid(TotalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Patient);
        EnsureHeaderValid(request.VisitId, request.Patient.Name, request.Patient.Email);
        EnsureItemsValid(request.Items);
        ArgumentOutOfRangeException.ThrowIfNegative(request.RegistrationFee);
    }
}

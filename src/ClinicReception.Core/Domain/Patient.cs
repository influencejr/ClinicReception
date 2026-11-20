namespace ClinicReception.Core.Domain;

/// <summary>
/// Пацієнт клініки (довідник учасників). Незмінний: нові дані пацієнта —
/// це новий об'єкт.
/// </summary>
public sealed record Patient
{
    /// <summary>Створює пацієнта, перевіряючи ідентифікатор, ім'я та пошту.</summary>
    /// <param name="id">Ідентифікатор пацієнта, більший за нуль.</param>
    /// <param name="fullName">Прізвище, ім'я та по батькові.</param>
    /// <param name="email">Електронна пошта для сповіщень, містить символ @.</param>
    /// <exception cref="ArgumentOutOfRangeException">Ідентифікатор не додатний.</exception>
    /// <exception cref="ArgumentException">Ім'я або пошта порожні, пошта без @.</exception>
    public Patient(int id, string fullName, string email)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        if (!email.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Пошта пацієнта має містити символ @.", nameof(email));
        }

        Id = id;
        FullName = fullName;
        Email = email;
    }

    /// <summary>Ідентифікатор пацієнта.</summary>
    public int Id { get; }

    /// <summary>Прізвище, ім'я та по батькові пацієнта.</summary>
    public string FullName { get; }

    /// <summary>Електронна пошта для сповіщень про запис.</summary>
    public string Email { get; }

    /// <summary>Контактний телефон; null, якщо пацієнт його не залишив.</summary>
    public string? Phone { get; init; }

    /// <summary>Ознака постійного пацієнта.</summary>
    public bool IsRegular { get; init; }

    /// <summary>Ознака пацієнта зі страховим полісом.</summary>
    public bool HasInsurance { get; init; }
}

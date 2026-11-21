using System.Globalization;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Errors;

namespace ClinicReception.App.Input;

/// <summary>
/// Барикада для послуги запису: перетворює рядки з консолі на перевірений
/// <see cref="VisitItem"/>. Ядро рядків не бачить.
/// </summary>
public static class VisitItemParser
{
    /// <summary>
    /// Розбирає послугу, введену одним рядком: код, кількість, ціна, хвилини
    /// через пробіл, наприклад «LAB-02 1 180,50 10».
    /// </summary>
    /// <param name="line">Рядок з консолі.</param>
    /// <returns>Послуга або пояснення, що введено не так.</returns>
    public static Result<VisitItem> Parse(string? line)
    {
        string[] parts = (line ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
        {
            return Result<VisitItem>.Fail("Потрібно чотири значення через пробіл: код, кількість, ціна, хвилини");
        }

        return Parse(parts[0], parts[1], parts[2], parts[3]);
    }

    /// <summary>
    /// Розбирає введені поля послуги. Дробову частину ціни можна відокремити
    /// і комою, і крапкою, від мови системи це не залежить.
    /// </summary>
    /// <param name="serviceCode">Код послуги.</param>
    /// <param name="quantity">Кількість одиниць.</param>
    /// <param name="price">Ціна за одиницю, грн.</param>
    /// <param name="minutes">Тривалість однієї одиниці, хв.</param>
    /// <returns>Послуга або пояснення, що введено не так.</returns>
    public static Result<VisitItem> Parse(string? serviceCode, string? quantity, string? price, string? minutes)
    {
        if (string.IsNullOrWhiteSpace(serviceCode))
        {
            return Result<VisitItem>.Fail("Код послуги порожній");
        }

        if (!int.TryParse(quantity, NumberStyles.None, CultureInfo.InvariantCulture, out int count) || count <= 0)
        {
            return Result<VisitItem>.Fail("Кількість — ціле число більше 0");
        }

        string normalizedPrice = (price ?? string.Empty).Trim().Replace(',', '.');
        if (!decimal.TryParse(normalizedPrice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal unitPrice))
        {
            return Result<VisitItem>.Fail("Ціна — невід'ємне число, наприклад 650,50");
        }

        if (!int.TryParse(minutes, NumberStyles.None, CultureInfo.InvariantCulture, out int duration) || duration <= 0)
        {
            return Result<VisitItem>.Fail("Тривалість — ціле число хвилин більше 0");
        }

        return Result<VisitItem>.Ok(new VisitItem(serviceCode.Trim().ToUpperInvariant(), count, unitPrice, duration));
    }
}

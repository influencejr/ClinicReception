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
    /// <summary>Найдовший код послуги в прайсі клініки, символів.</summary>
    public const int MaxCodeLength = 20;

    /// <summary>Найбільша кількість однієї послуги в записі.</summary>
    public const int MaxQuantity = 50;

    /// <summary>Найвища ціна однієї послуги, грн.</summary>
    public const decimal MaxPrice = 100_000m;

    /// <summary>Найдовша послуга, хв (зміна лікаря).</summary>
    public const int MaxMinutes = 480;

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
        if (string.IsNullOrWhiteSpace(serviceCode) || serviceCode.Trim().Length > MaxCodeLength)
        {
            return Result<VisitItem>.Fail($"Код послуги — від 1 до {MaxCodeLength} символів");
        }

        if (!int.TryParse(quantity, NumberStyles.None, CultureInfo.InvariantCulture, out int count)
            || count is < 1 or > MaxQuantity)
        {
            return Result<VisitItem>.Fail($"Кількість — ціле число від 1 до {MaxQuantity}");
        }

        string normalizedPrice = (price ?? string.Empty).Trim().Replace(',', '.');
        if (!decimal.TryParse(normalizedPrice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal unitPrice)
            || unitPrice > MaxPrice)
        {
            return Result<VisitItem>.Fail($"Ціна — число від 0 до {MaxPrice:0}, наприклад 650,50");
        }

        if (!int.TryParse(minutes, NumberStyles.None, CultureInfo.InvariantCulture, out int duration)
            || duration is < 1 or > MaxMinutes)
        {
            return Result<VisitItem>.Fail($"Тривалість — ціле число хвилин від 1 до {MaxMinutes}");
        }

        return Result<VisitItem>.Ok(new VisitItem(serviceCode.Trim().ToUpperInvariant(), count, unitPrice, duration));
    }
}

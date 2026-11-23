using System.Globalization;
using ClinicReception.Core.Errors;
using ClinicReception.Core.Services;

namespace ClinicReception.App.Input;

/// <summary>
/// Барикада вводу: рядок з консолі перевіряється тут рівно один раз і далі йде вже
/// як число, дата чи код. Некоректний ввід - не виняток, а пояснення для повторного запиту.
/// </summary>
public static class InputParser
{
    /// <summary>Формат дати, який вводить реєстратор.</summary>
    public const string DateFormat = "dd.MM.yyyy";

    /// <summary>Найдовший код лікаря, символів.</summary>
    public const int MaxCodeLength = 20;

    /// <summary>Розбирає додатне ціле число, наприклад номер пацієнта чи запису.</summary>
    /// <param name="text">Рядок з консолі.</param>
    /// <returns>Число або пояснення помилки.</returns>
    public static Result<int> PositiveNumber(string? text) =>
        int.TryParse(text?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int value) && value > 0
            ? Result<int>.Ok(value)
            : Result<int>.Fail("Потрібне ціле число більше за нуль");

    /// <summary>Розбирає тривалість прийому в хвилинах.</summary>
    /// <param name="text">Рядок з консолі.</param>
    /// <returns>Хвилини від 1 до <see cref="VisitItemParser.MaxMinutes"/> або пояснення помилки.</returns>
    public static Result<int> Minutes(string? text)
    {
        Result<int> number = PositiveNumber(text);
        return number.IsSuccess && number.Value <= VisitItemParser.MaxMinutes
            ? number
            : Result<int>.Fail($"Тривалість - ціле число хвилин від 1 до {VisitItemParser.MaxMinutes}");
    }

    /// <summary>Розбирає дату у форматі дд.мм.рррр.</summary>
    /// <param name="text">Рядок з консолі.</param>
    /// <returns>Дата або пояснення помилки.</returns>
    public static Result<DateOnly> Date(string? text) =>
        DateOnly.TryParseExact(text?.Trim(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date)
            ? Result<DateOnly>.Ok(date)
            : Result<DateOnly>.Fail("Дата у форматі дд.мм.рррр, наприклад 24.11.2026");

    /// <summary>Розбирає код лікаря: непорожній, без пробілів, великими літерами.</summary>
    /// <param name="text">Рядок з консолі.</param>
    /// <returns>Код або пояснення помилки.</returns>
    public static Result<string> DoctorCode(string? text)
    {
        string code = (text ?? string.Empty).Trim().ToUpperInvariant();
        return code.Length is > 0 and <= MaxCodeLength && !code.Contains(' ', StringComparison.Ordinal)
            ? Result<string>.Ok(code)
            : Result<string>.Fail($"Код лікаря - до {MaxCodeLength} символів без пробілів, наприклад DOC-THER-01");
    }

    /// <summary>Розбирає подію зміни стану: 1 - підтвердити, 2 - завершити, 3 - неявка.</summary>
    /// <param name="text">Рядок з консолі.</param>
    /// <returns>Подія або пояснення помилки.</returns>
    public static Result<StatusAction> Action(string? text) => text?.Trim() switch
    {
        "1" => Result<StatusAction>.Ok(StatusAction.Confirm),
        "2" => Result<StatusAction>.Ok(StatusAction.Complete),
        "3" => Result<StatusAction>.Ok(StatusAction.MarkMissed),
        _ => Result<StatusAction>.Fail("Оберіть 1, 2 або 3"),
    };
}

using System.Diagnostics.CodeAnalysis;

namespace ClinicReception.Core.Errors;

/// <summary>
/// Результат операції, яка може очікувано не вдатися: містить або значення,
/// або пояснення помилки для користувача.
/// </summary>
/// <typeparam name="T">Тип значення в разі успіху.</typeparam>
[SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Result<T>.Ok і Result<T>.Fail читаються природніше, ніж окремий негенеричний клас-фабрика.")]
public readonly record struct Result<T>
{
    private Result(bool isSuccess, T? value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Чи вдалася операція.</summary>
    public bool IsSuccess { get; }

    /// <summary>Значення; має сенс лише коли <see cref="IsSuccess"/> дорівнює true.</summary>
    public T? Value { get; }

    /// <summary>Пояснення помилки; порожнє в разі успіху.</summary>
    public string Error { get; }

    /// <summary>Створює успішний результат.</summary>
    /// <param name="value">Отримане значення.</param>
    /// <returns>Результат зі значенням.</returns>
    public static Result<T> Ok(T value) => new(true, value, string.Empty);

    /// <summary>Створює результат з помилкою.</summary>
    /// <param name="error">Пояснення для користувача.</param>
    /// <returns>Результат без значення.</returns>
    public static Result<T> Fail(string error) => new(false, default, error);
}

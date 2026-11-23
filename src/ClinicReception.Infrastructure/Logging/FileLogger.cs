using Microsoft.Extensions.Logging;

namespace ClinicReception.Infrastructure.Logging;

/// <summary>Логер, що передає рядки в <see cref="FileLoggerProvider"/>.</summary>
internal sealed class FileLogger : ILogger
{
    private static readonly AsyncLocal<string?> CurrentScope = new();

    private readonly FileLoggerProvider _provider;
    private readonly string _category;

    /// <summary>Створює логер категорії.</summary>
    /// <param name="provider">Власник файлу журналу.</param>
    /// <param name="category">Категорія логера.</param>
    public FileLogger(FileLoggerProvider provider, string category)
    {
        _provider = provider;
        _category = category;
    }

    /// <summary>Відкриває вкладену область; її текст потрапляє в поле «сценарій».</summary>
    /// <typeparam name="TState">Тип стану області.</typeparam>
    /// <param name="state">Стан області, наприклад «UC-01».</param>
    /// <returns>Об'єкт, що закриває область.</returns>
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
    {
        string? previous = CurrentScope.Value;
        CurrentScope.Value = previous is null ? state.ToString() : previous + " " + state;
        return new ScopeExit(previous);
    }

    /// <summary>Чи пише логер на цьому рівні; мінімальний рівень задає фабрика.</summary>
    /// <param name="logLevel">Рівень.</param>
    /// <returns>true для всіх рівнів, крім None.</returns>
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    /// <summary>Пише рядок журналу; для винятку додає його тип і повідомлення.</summary>
    /// <typeparam name="TState">Тип стану запису.</typeparam>
    /// <param name="logLevel">Рівень.</param>
    /// <param name="eventId">Ідентифікатор події, не використовується.</param>
    /// <param name="state">Стан запису.</param>
    /// <param name="exception">Виняток або null.</param>
    /// <param name="formatter">Функція, що формує текст.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string message = formatter(state, exception);
        string error = exception is null ? string.Empty : $" | {exception.GetType().Name}: {exception.Message}";
        _provider.Write($"{logLevel};{CurrentScope.Value ?? _category};{message}{error}");
    }

    private sealed class ScopeExit : IDisposable
    {
        private readonly string? _previous;

        public ScopeExit(string? previous) => _previous = previous;

        public void Dispose() => CurrentScope.Value = _previous;
    }
}

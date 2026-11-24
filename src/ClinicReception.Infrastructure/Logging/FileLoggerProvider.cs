using Microsoft.Extensions.Logging;

namespace ClinicReception.Infrastructure.Logging;

/// <summary>
/// Журнал застосунку у файлі: рядок «час;рівень;сценарій;повідомлення». Сценарій береться
/// з області журналу (BeginScope), а якщо її немає - з категорії логера.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly StreamWriter _writer;
    private readonly TimeProvider _clock;
    private readonly object _sync = new();

    /// <summary>Відкриває файл журналу для дописування.</summary>
    /// <param name="path">Шлях до файлу; тека створюється, якщо її немає.</param>
    /// <param name="clock">Джерело часу для рядків журналу.</param>
    /// <exception cref="IOException">Файл зайнятий іншим процесом.</exception>
    public FileLoggerProvider(string path, TimeProvider clock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        string? folder = Path.GetDirectoryName(Path.GetFullPath(path));
        if (folder is not null)
        {
            Directory.CreateDirectory(folder);
        }

        _writer = new StreamWriter(path, append: true) { AutoFlush = true };
    }

    /// <summary>Створює логер для категорії.</summary>
    /// <param name="categoryName">Категорія, зазвичай ім'я класу.</param>
    /// <returns>Логер, що пише в спільний файл.</returns>
    public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

    /// <summary>Закриває файл журналу.</summary>
    public void Dispose() => _writer.Dispose();

    /// <summary>Дописує рядок; виклики з різних потоків не перемішуються.</summary>
    /// <param name="line">Готовий рядок без часу.</param>
    internal void Write(string line)
    {
        string stamp = _clock.GetLocalNow().ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        lock (_sync)
        {
            _writer.WriteLine(stamp + ";" + line);
        }
    }
}

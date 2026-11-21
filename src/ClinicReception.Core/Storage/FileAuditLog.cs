using System.Globalization;

namespace ClinicReception.Core.Storage;

/// <summary>
/// Журнал аудиту у файлі: по рядку на операцію (час, операція, номер запису).
/// Тримає файл відкритим, доки його не звільнять через Dispose.
/// </summary>
public sealed class FileAuditLog : IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    /// <summary>Відкриває файл аудиту для дописування.</summary>
    /// <param name="path">Шлях до файлу; якщо файлу немає, його буде створено.</param>
    /// <exception cref="IOException">Файл зайнятий іншим процесом.</exception>
    /// <exception cref="UnauthorizedAccessException">Немає прав на запис.</exception>
    public FileAuditLog(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _writer = new StreamWriter(path, append: true);
    }

    /// <summary>Дописує рядок аудиту.</summary>
    /// <param name="operation">Назва операції, наприклад "book".</param>
    /// <param name="appointmentId">Номер запису, над яким виконано операцію.</param>
    /// <exception cref="ObjectDisposedException">Журнал уже закрито.</exception>
    public void Write(string operation, int appointmentId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        _writer.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{DateTimeOffset.Now:O};{operation};{appointmentId}"));
    }

    /// <summary>Скидає буфер і закриває файл. Повторний виклик нічого не робить.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _writer.Flush();
        _writer.Dispose();
        _disposed = true;
    }
}

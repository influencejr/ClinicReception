using ClinicReception.Core.Abstractions;

namespace ClinicReception.Core.Notifications;

/// <summary>Сповіщувач, що пише повідомлення в переданий <see cref="TextWriter"/>.</summary>
public sealed class TextWriterNotifier : INotifier
{
    private readonly TextWriter _output;

    /// <summary>Створює сповіщувач.</summary>
    /// <param name="output">Куди писати повідомлення: консоль, файл, StringWriter у тесті.</param>
    /// <exception cref="ArgumentNullException">Потік не передано.</exception>
    public TextWriterNotifier(TextWriter output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Адресат або текст порожні.</exception>
    public void Notify(string recipient, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _output.WriteLine($"Лист для {recipient}: {message}");
    }
}

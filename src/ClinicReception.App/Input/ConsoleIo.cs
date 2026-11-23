using ClinicReception.Core.Errors;

namespace ClinicReception.App.Input;

/// <summary>Діалог з реєстратором через потоки вводу й виводу; у тестах замість консолі - рядки.</summary>
public sealed class ConsoleIo
{
    private readonly TextReader _input;
    private readonly TextWriter _output;

    /// <summary>Створює діалог над потоками.</summary>
    /// <param name="input">Звідки читати відповіді.</param>
    /// <param name="output">Куди писати запитання й результати.</param>
    /// <exception cref="ArgumentNullException">Потік не передано.</exception>
    public ConsoleIo(TextReader input, TextWriter output)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>Пише рядок.</summary>
    /// <param name="text">Текст.</param>
    public void Say(string text) => _output.WriteLine(text);

    /// <summary>Питає, доки відповідь не пройде перевірку; порожня відповідь скасовує запитання.</summary>
    /// <typeparam name="T">Тип відповіді.</typeparam>
    /// <param name="prompt">Запитання.</param>
    /// <param name="parse">Перевірка відповіді з барикади <see cref="InputParser"/>.</param>
    /// <param name="value">Перевірена відповідь.</param>
    /// <returns>false, якщо реєстратор відповів порожнім рядком або ввід закінчився.</returns>
    public bool TryAsk<T>(string prompt, Func<string?, Result<T>> parse, out T value)
    {
        ArgumentNullException.ThrowIfNull(parse);
        while (true)
        {
            _output.Write(prompt + ": ");
            string? line = _input.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                value = default!;
                return false;
            }

            Result<T> parsed = parse(line);
            if (parsed.IsSuccess)
            {
                value = parsed.Value!;
                return true;
            }

            _output.WriteLine("Помилка вводу: " + parsed.Error);
        }
    }

    /// <summary>Читає рядок без перевірки, наприклад пункт меню.</summary>
    /// <param name="prompt">Запитання.</param>
    /// <returns>Відповідь без пробілів по краях або null, якщо ввід закінчився.</returns>
    public string? Ask(string prompt)
    {
        _output.Write(prompt + ": ");
        return _input.ReadLine()?.Trim();
    }
}

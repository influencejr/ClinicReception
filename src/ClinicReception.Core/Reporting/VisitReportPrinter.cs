namespace ClinicReception.Core.Reporting;

/// <summary>Виведення готового звіту в переданий потік.</summary>
public sealed class VisitReportPrinter
{
    private readonly TextWriter _output;

    /// <summary>Створює принтер, що пише у вказаний потік.</summary>
    /// <param name="output">Потік виведення: консоль, файл або StringWriter у тестах.</param>
    /// <exception cref="ArgumentNullException">Потік не передано.</exception>
    public VisitReportPrinter(TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _output = output;
    }

    /// <summary>Записує звіт у потік. Команда: нічого не повертає.</summary>
    /// <param name="report">Непорожній текст звіту.</param>
    /// <exception cref="ArgumentException">Текст порожній.</exception>
    public void Print(string report)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(report);
        _output.Write(report);
    }
}

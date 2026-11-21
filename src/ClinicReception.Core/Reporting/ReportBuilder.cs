using System.Text;

namespace ClinicReception.Core.Reporting;

/// <summary>Формує текст простого звіту з готових рядків.</summary>
public sealed class ReportBuilder
{
    /// <summary>Заголовок звіту, у тексті виводиться великими літерами.</summary>
    public string Title { get; init; } = "Звіт";

    /// <summary>Складає звіт: заголовок, потім кожен рядок з нового рядка.</summary>
    /// <param name="rows">Рядки звіту в потрібному порядку.</param>
    /// <returns>Текст звіту.</returns>
    public string Build(IReadOnlyList<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        StringBuilder text = new();
        text.AppendLine(Title.ToUpperInvariant());
        foreach (string row in rows)
        {
            text.AppendLine(row);
        }

        return text.ToString();
    }
}

namespace ClinicReception.Core.Errors;

/// <summary>
/// Порушено правило предметної області: наприклад, послугу додають до
/// закритого запису або запису з таким номером немає.
/// </summary>
public class DomainRuleException : Exception
{
    /// <summary>Створює виняток з ідентифікатором правила та поясненням.</summary>
    /// <param name="rule">Ідентифікатор правила, наприклад "appointment.editable".</param>
    /// <param name="message">Пояснення українською з конкретними значеннями.</param>
    public DomainRuleException(string rule, string message)
        : base(message)
    {
        Rule = rule;
    }

    /// <summary>Створює виняток із внутрішньою причиною.</summary>
    /// <param name="rule">Ідентифікатор правила.</param>
    /// <param name="message">Пояснення українською.</param>
    /// <param name="inner">Виняток, що спричинив порушення.</param>
    public DomainRuleException(string rule, string message, Exception inner)
        : base(message, inner)
    {
        Rule = rule;
    }

    /// <summary>Ідентифікатор порушеного правила, за ним журнал можна фільтрувати.</summary>
    public string Rule { get; }
}

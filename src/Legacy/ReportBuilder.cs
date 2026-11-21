using System.Text;

namespace ClinicReception.Legacy;

/// <summary>Текст звіту про прийом. Формат має збігатися з еталоном посимвольно.</summary>
public sealed class ReportBuilder
{
    /// <summary>Будує звіт: заголовок, пацієнт, рядки послуг, знижка, доставка, разом.</summary>
    /// <param name="request">Дані запису: номер, пацієнт, послуги, валюта.</param>
    /// <param name="discount">Знижка, грн.</param>
    /// <param name="shipping">Доставка, грн.</param>
    /// <param name="total">Разом до сплати, грн.</param>
    /// <returns>Текст звіту з переведенням рядка "\n" після кожного рядка.</returns>
    public string Build(VisitRequest request, decimal discount, decimal shipping, decimal total)
    {
        string currency = request.Currency;
        StringBuilder text = new();
        text.Append("Документ #").Append(request.AppointmentId).Append('\n');
        text.Append("Клієнт: ").Append(request.Patient.Name).Append('\n');
        foreach (ServiceLine item in request.Items!)
        {
            text.Append(Line(item, currency));
        }

        text.Append(Money("Знижка", discount, currency));
        text.Append(Money("Доставка", shipping, currency));
        text.Append(Money("Разом", total, currency));
        return text.ToString();
    }

    private static string Line(ServiceLine item, string currency) =>
        item.Code + " x" + item.Qty + " = " + (item.Qty * item.Price).ToString("0.00") + " " + currency + "\n";

    private static string Money(string label, decimal value, string currency) =>
        label + ": " + value.ToString("0.00") + " " + currency + "\n";
}

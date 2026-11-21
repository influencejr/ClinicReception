using System;
using System.Collections.Generic;

namespace ClinicReception.Legacy;

public enum PatientKind
{
    Regular,
    Vip,
    Staff,
}

public enum AppointmentState
{
    Draft,
    New,
    Paid,
    Closed,
}

public class Patient
{
    public int Id;
    public string Name = "";
    public string Email = "";
    public PatientKind Kind = PatientKind.Regular;
    public int VisitCount;
    public DateTime SinceUtc;
}

public class ServiceLine
{
    public string Code = "";
    public int Qty;
    public decimal Price;
    public string Group = "";
}

public class LegacyAppointmentProcessor
{
    private decimal _tmpSum;
    private decimal _tmpDiscount;
    private readonly List<string> _log = new();
    private readonly DiscountPolicy _discounts = new();
    private readonly ShippingPolicy _shipping = new();

    public string Handle(int appointmentId, int patientId, string patientName,
        string? patientMail, PatientKind patientKind, int patientVisits,
        List<ServiceLine>? items, AppointmentState state, string currency,
        DateTime createdAt, bool sendMail)
    {
        string? error = Validate(items, state, patientMail);
        if (error != null)
        {
            return error;
        }

        _log.Add("ok " + appointmentId);

        _tmpSum = Subtotal(items!);
        _tmpDiscount = _discounts.For(_tmpSum, patientKind, patientVisits);
        decimal ship = _shipping.For(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = "Документ #" + appointmentId + "\n";
        txt += "Клієнт: " + patientName + "\n";
        for (int i = 0; i < items!.Count; i++)
        {
            txt += items[i].Code + " x" + items[i].Qty + " = "
                + (items[i].Qty * items[i].Price).ToString("0.00")
                + " " + currency + "\n";
        }
        txt += "Знижка: " + _tmpDiscount.ToString("0.00") + " " + currency + "\n";
        txt += "Доставка: " + ship.ToString("0.00") + " " + currency + "\n";
        txt += "Разом: " + total.ToString("0.00") + " " + currency + "\n";

        if (sendMail)
        {
            _log.Add("mail -> " + patientMail);
        }
        return txt;
    }

    private static decimal Subtotal(List<ServiceLine> items)
    {
        decimal sum = 0m;
        foreach (ServiceLine item in items)
        {
            sum += item.Qty * item.Price;
        }

        return sum;
    }

    // порядок перевірок той самий, що й у вкладених if: від нього залежить код помилки
    private static string? Validate(List<ServiceLine>? items, AppointmentState state, string? patientMail)
    {
        if (items is null)
        {
            return "ERR: null";
        }

        if (items.Count == 0)
        {
            return "ERR: empty";
        }

        if (state is not (AppointmentState.New or AppointmentState.Paid))
        {
            return "ERR: state";
        }

        if (patientMail is null || !patientMail.Contains("@"))
        {
            return "ERR: mail";
        }

        return null;
    }

    public decimal Preview(PatientKind patientKind, int patientVisits, List<ServiceLine> items)
    {
        decimal subtotal = Subtotal(items);
        decimal payable = subtotal - _discounts.For(subtotal, patientKind, patientVisits);
        return payable + _shipping.For(payable);
    }

    public string DescribeClient(Patient c)
    {
        string s = c.Name.Trim().ToUpper();
        if (c.Kind == PatientKind.Vip) s += " [VIP]";
        if (c.VisitCount > PricingRules.LoyalVisits) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + c.Email.ToLower() + ">";
        int years = DateTime.Now.Year - c.SinceUtc.Year;
        s += " стаж " + years;
        return s;
    }

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}

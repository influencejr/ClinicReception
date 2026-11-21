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

    public bool IsLoyal => VisitCount > PricingRules.LoyalVisits;

    public string Describe()
    {
        string s = Name.Trim().ToUpper();
        if (Kind == PatientKind.Vip) s += " [VIP]";
        if (IsLoyal) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + Email.ToLower() + ">";
        int years = DateTime.Now.Year - SinceUtc.Year;
        s += " стаж " + years;
        return s;
    }
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
    private readonly ReportBuilder _report = new();

    public string Handle(VisitRequest request)
    {
        string? error = Validate(request);
        if (error != null)
        {
            return error;
        }

        _log.Add("ok " + request.AppointmentId);

        _tmpSum = Subtotal(request.Items!);
        _tmpDiscount = _discounts.For(_tmpSum, request.Patient);
        decimal ship = _shipping.For(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;

        if (request.SendMail)
        {
            _log.Add("mail -> " + request.Patient.Email);
        }

        return _report.Build(request, _tmpDiscount, ship, total);
    }

    private static decimal Subtotal(IReadOnlyList<ServiceLine> items)
    {
        decimal sum = 0m;
        foreach (ServiceLine item in items)
        {
            sum += item.Qty * item.Price;
        }

        return sum;
    }

    // порядок перевірок той самий, що й у вкладених if: від нього залежить код помилки
    private static string? Validate(VisitRequest request)
    {
        if (request.Items is null)
        {
            return "ERR: null";
        }

        if (request.Items.Count == 0)
        {
            return "ERR: empty";
        }

        if (request.State is not (AppointmentState.New or AppointmentState.Paid))
        {
            return "ERR: state";
        }

        if (request.Patient.Email is null || !request.Patient.Email.Contains("@"))
        {
            return "ERR: mail";
        }

        return null;
    }

    public decimal Preview(Patient patient, IReadOnlyList<ServiceLine> items)
    {
        decimal subtotal = Subtotal(items);
        decimal payable = subtotal - _discounts.For(subtotal, patient);
        return payable + _shipping.For(payable);
    }

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}

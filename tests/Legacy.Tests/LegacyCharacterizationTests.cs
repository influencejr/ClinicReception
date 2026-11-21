using System.Globalization;
using ClinicReception.Legacy;

namespace ClinicReception.Legacy.Tests;

public class LegacyCharacterizationTests
{
    public LegacyCharacterizationTests()
    {
        // формат чисел у звіті залежить від культури середовища, тому фіксуємо її явно
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Theory]
    [InlineData(null, AppointmentState.New, "a@b.c", "ERR: null")]
    [InlineData("empty", AppointmentState.New, "a@b.c", "ERR: empty")]
    [InlineData("two", AppointmentState.Draft, "a@b.c", "ERR: state")]
    [InlineData("two", AppointmentState.New, "no-mail", "ERR: mail")]
    public void Handle_BadInput_ReturnsErrorCode(string? kindOfItems, AppointmentState state, string mail, string expected)
    {
        List<ServiceLine>? items = kindOfItems switch
        {
            "two" => TwoLines(),
            "empty" => new List<ServiceLine>(),
            _ => null,
        };
        LegacyAppointmentProcessor sut = new();

        string actual = sut.Handle(1001, 7, "Іван", mail, PatientKind.Regular, 0, items, state, "UAH", new DateTime(2026, 3, 1), false);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Handle_VipAppointment_ReturnsExactReport()
    {
        LegacyAppointmentProcessor sut = new();

        string report = sut.Handle(1001, 7, "Іван", "a@b.c", PatientKind.Vip, 3, TwoLines(), AppointmentState.New, "UAH", new DateTime(2026, 3, 1), false);

        string expected = "Документ #1001\n"
            + "Клієнт: Іван\n"
            + "A-1 x2 = 300.00 UAH\n"
            + "B-2 x1 = 400.00 UAH\n"
            + "Знижка: 105.00 UAH\n"
            + "Доставка: 60.00 UAH\n"
            + "Разом: 655.00 UAH\n";
        Assert.Equal(expected, report);
    }

    [Fact]
    public void Handle_SendMail_WritesOkAndMailToLog()
    {
        LegacyAppointmentProcessor sut = new();

        sut.Handle(1001, 7, "Іван", "a@b.c", PatientKind.Regular, 0, TwoLines(), AppointmentState.Paid, "UAH", new DateTime(2026, 3, 1), true);

        Assert.Equal("ok 1001\nmail -> a@b.c\n", sut.DumpLog());
    }

    [Theory]
    [InlineData(PatientKind.Regular, 0, 760)]
    [InlineData(PatientKind.Vip, 0, 655)]
    [InlineData(PatientKind.Staff, 0, 550)]
    [InlineData(PatientKind.Regular, 11, 725)]
    public void Preview_ByKind_ReturnsTotal(PatientKind kind, int visits, decimal expected)
    {
        LegacyAppointmentProcessor sut = new();

        decimal total = sut.Preview(kind, visits, TwoLines());

        Assert.Equal(expected, total);
    }

    [Fact]
    public void Preview_PayableExactly1000_ReturnsTotal()
    {
        List<ServiceLine> items = new()
        {
            new ServiceLine { Code = "MRI-01", Qty = 2, Price = 500m },
        };

        decimal total = new LegacyAppointmentProcessor().Preview(PatientKind.Regular, 0, items);

        Assert.Equal(1000m, total);
    }

    [Fact]
    public void Preview_StaffLargeSum_ReturnsTotal()
    {
        List<ServiceLine> items = new()
        {
            new ServiceLine { Code = "SURG-01", Qty = 1, Price = 2000m },
        };

        decimal total = new LegacyAppointmentProcessor().Preview(PatientKind.Staff, 0, items);

        Assert.Equal(1500m, total);
    }

    [Fact]
    public void DescribeClient_Vip_BuildsCaption()
    {
        Patient patient = new()
        {
            Name = " іван ",
            Email = "IVAN@MAIL.COM",
            Kind = PatientKind.Vip,
            VisitCount = 12,
            SinceUtc = new DateTime(2020, 1, 1),
        };

        // метод залежить від системного годинника, тому очікування обчислюємо, а не вписуємо
        int years = DateTime.Now.Year - 2020;
        string expected = "ІВАН [VIP] [ЛОЯЛЬНИЙ] <ivan@mail.com> стаж " + years;

        string actual = new LegacyAppointmentProcessor().DescribeClient(patient);

        Assert.Equal(expected, actual);
    }

    private static List<ServiceLine> TwoLines() => new()
    {
        new ServiceLine { Code = "A-1", Qty = 2, Price = 150m },
        new ServiceLine { Code = "B-2", Qty = 1, Price = 400m },
    };
}

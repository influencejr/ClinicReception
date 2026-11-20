using ClinicReception.Core.Domain;

namespace ClinicReception.Tests;

public class AppointmentTests
{
    [Fact]
    public void AddItem_Null_ThrowsArgumentNull()
    {
        Appointment appointment = NewAppointment();

        Assert.Throws<ArgumentNullException>(() => appointment.AddItem(null!));
    }

    [Fact]
    public void Subtotal_TwoItems_SumsAmounts()
    {
        Appointment appointment = NewAppointment();
        appointment.AddItem(new VisitItem("CONS-01", 1, 650m, 20));
        appointment.AddItem(new VisitItem("ECG-01", 1, 320.50m, 15));

        Assert.Equal(970.50m, appointment.Subtotal);
    }

    private static Appointment NewAppointment() =>
        new(1, 10, "DOC-THER-01", new DateOnly(2026, 3, 10), new TimeOnly(9, 30));
}

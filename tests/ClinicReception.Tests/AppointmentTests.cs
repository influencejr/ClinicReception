using ClinicReception.Core.Domain;

namespace ClinicReception.Tests;

public class AppointmentTests
{
    [Fact]
    public void AddItem_Null_ThrowsArgumentNull()
    {
        Appointment appointment = new() { Id = 1, PatientId = 10, DoctorCode = "DOC-THER-01" };

        Assert.Throws<ArgumentNullException>(() => appointment.AddItem(null!));
    }

    [Fact]
    public void Total_TwoItems_SumsAmounts()
    {
        Appointment appointment = new() { Id = 1, PatientId = 10, DoctorCode = "DOC-THER-01" };
        appointment.AddItem(new VisitItem { ServiceCode = "CONS-01", Quantity = 1, UnitPrice = 650m });
        appointment.AddItem(new VisitItem { ServiceCode = "ECG-01", Quantity = 1, UnitPrice = 320.50m });

        decimal total = appointment.Total();

        Assert.Equal(970.50m, total);
    }
}

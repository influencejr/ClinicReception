using System.Globalization;
using System.Text;
using ClinicReception.Core.Domain;

Console.OutputEncoding = Encoding.UTF8;

Appointment appointment = new()
{
    Id = 1,
    PatientId = 10,
    DoctorCode = "DOC-THER-01",
    VisitDate = new DateOnly(2026, 11, 20),
    StartTime = new TimeOnly(9, 30),
    CreatedAt = DateTimeOffset.Now,
};

appointment.AddItem(new VisitItem
{
    ServiceCode = "CONS-01", Quantity = 1, UnitPrice = 650.00m, DurationMinutes = 20,
});
appointment.AddItem(new VisitItem
{
    ServiceCode = "ECG-01", Quantity = 1, UnitPrice = 320.50m, DurationMinutes = 15,
});

string total = appointment.Total()
    .ToString("F2", CultureInfo.InvariantCulture);
TimeOnly endTime = appointment.StartTime.AddMinutes(appointment.TotalMinutes());
string date = appointment.VisitDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

Console.WriteLine($"Запис #{appointment.Id}");
Console.WriteLine($"Лікар: {appointment.DoctorCode}");
Console.WriteLine($"Дата: {date}, {appointment.StartTime:HH:mm}–{endTime:HH:mm}");
Console.WriteLine($"Стан: {appointment.Status}");
Console.WriteLine($"Послуг: {appointment.Items.Count}");
Console.WriteLine($"Тривалість: {appointment.TotalMinutes()} хв");
Console.WriteLine($"Сума: {total}");

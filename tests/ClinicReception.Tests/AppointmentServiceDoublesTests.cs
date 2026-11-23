using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Notifications;
using ClinicReception.Core.Services;
using ClinicReception.Tests.Doubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicReception.Tests;

public class AppointmentServiceDoublesTests
{
    private static readonly DateOnly Tuesday = new(2026, 3, 10);

    [Fact]
    public void TotalOf_TwoItems_SumsPricesFromPolicy()
    {
        FakeAppointmentRepository repository = new();
        Appointment appointment = new(7, 10, "DOC-THER-01", VisitTime.At(Tuesday, new TimeOnly(9, 30)));
        appointment.AddItem(new VisitItem("CONS-01", 1, 650m, 20));
        appointment.AddItem(new VisitItem("LAB-02", 2, 180m, 10));
        AppointmentService service = CreateService(repository, new StubPricingPolicy(25m));

        decimal actual = service.TotalOf(appointment);

        Assert.Equal(50m, actual);
    }

    [Fact]
    public void Book_OwnPatient_StoresOnce()
    {
        FakeAppointmentRepository repository = new();
        AppointmentService service = CreateService(repository, new StubPricingPolicy(1m));
        Appointment appointment = new(1, 5, "DOC-THER-01", VisitTime.At(Tuesday, new TimeOnly(10, 0)));
        Patient patient = new(5, "Іваненко Ольга Петрівна", "olha@ex.com");

        service.Book(appointment, patient);

        Assert.NotNull(repository.GetById(1));
        Assert.Equal(1, repository.AddCalls);
    }

    [Fact]
    public void Book_ForeignPatient_ThrowsAndStoresNothing()
    {
        FakeAppointmentRepository repository = new();
        AppointmentService service = CreateService(repository, new StubPricingPolicy(1m));
        Appointment appointment = new(2, 5, "DOC-THER-01", VisitTime.At(Tuesday, new TimeOnly(11, 0)));
        Patient stranger = new(6, "Коваль Андрій Іванович", "koval@ex.com");

        ArgumentException error = Assert.Throws<ArgumentException>(() => service.Book(appointment, stranger));

        Assert.Equal("patient", error.ParamName);
        Assert.Equal(0, repository.AddCalls);
    }

    [Fact]
    public void Book_NotifierFails_StillStoresAppointment()
    {
        FakeAppointmentRepository repository = new();
        AppointmentService service = CreateService(repository, new StubPricingPolicy(1m), new ThrowingNotifier());
        Appointment appointment = new(3, 5, "DOC-THER-01", VisitTime.At(Tuesday, new TimeOnly(12, 0)));
        Patient patient = new(5, "Іваненко Ольга Петрівна", "olha@ex.com");

        Exception? error = Record.Exception(() => service.Book(appointment, patient));

        Assert.Null(error);
        Assert.Equal(1, repository.AddCalls);
    }

    [Fact]
    public void TotalOf_AboveLargeVisit_SubtractsFixedDiscount()
    {
        Appointment appointment = new(4, 10, "DOC-THER-01", VisitTime.At(Tuesday, new TimeOnly(13, 0)));
        appointment.AddItem(new VisitItem("CONS-01", 1, 650m, 20));
        appointment.AddItem(new VisitItem("LAB-02", 2, 180m, 10));
        AppointmentService service = CreateService(new FakeAppointmentRepository(), new StubPricingPolicy(600m));

        decimal actual = service.TotalOf(appointment);

        Assert.Equal(1150m, actual);
    }

    [Fact]
    public void Confirm_AppointmentWithItems_MovesToConfirmed()
    {
        FakeAppointmentRepository repository = new();
        Appointment appointment = new(5, 10, "DOC-THER-01", VisitTime.At(Tuesday, new TimeOnly(14, 0)));
        appointment.AddItem(new VisitItem("CONS-01", 1, 650m, 20));
        repository.Add(appointment);
        AppointmentService service = CreateService(repository, new StubPricingPolicy(1m));

        service.Confirm(5);

        Assert.Equal(VisitStatus.Confirmed, appointment.Status);
    }

    private static AppointmentService CreateService(
        FakeAppointmentRepository repository,
        StubPricingPolicy pricing,
        INotifier? notifier = null) =>
        new(
            repository,
            pricing,
            notifier ?? new TextWriterNotifier(TextWriter.Null),
            NullLogger<AppointmentService>.Instance);
}

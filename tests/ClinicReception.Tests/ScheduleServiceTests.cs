using ClinicReception.Core.Domain;
using ClinicReception.Core.Errors;
using ClinicReception.Core.Notifications;
using ClinicReception.Core.Pricing;
using ClinicReception.Core.Services;
using ClinicReception.Tests.Doubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicReception.Tests;

// Сценарії UC-01...UC-03 на дублерах: фейкове сховище, фейковий довідник і фіксований
// годинник «понеділок, 23.11.2026, 09:05». Терапевт працює 08:00-10:00 з кроком 20 хв.
public class ScheduleServiceTests
{
    private static readonly DateOnly Today = new(2026, 11, 23);
    private static readonly DateOnly Tomorrow = Today.AddDays(1);

    private readonly FakeAppointmentRepository _repository = new();
    private readonly ScheduleService _service;

    public ScheduleServiceTests()
    {
        FixedClock clock = new(new DateTimeOffset(2026, 11, 23, 9, 5, 0, TimeSpan.Zero));
        AppointmentService appointments = new(
            _repository, new StandardPricingPolicy(), new TextWriterNotifier(TextWriter.Null), NullLogger<AppointmentService>.Instance);
        _service = new ScheduleService(_repository, new FakeClinicDirectory(), appointments, clock);
    }

    [Fact]
    public void Book_FreeDay_TakesWorkStartAndAddsConsultation()
    {
        Appointment a = _service.Book(new BookingRequest(1, FakeClinicDirectory.Therapist, Tomorrow, 20));

        Assert.Equal(new TimeOnly(8, 0), a.StartTime);
        Assert.Equal(VisitStatus.Scheduled, a.Status);
        Assert.Equal(450m, Assert.Single(a.Items).UnitPrice);
        Assert.Equal(1, _repository.AddCalls);
    }

    [Fact]
    public void Book_DoctorBusy_TakesNextSlotAndNewNumber()
    {
        Appointment first = _service.Book(new BookingRequest(1, FakeClinicDirectory.Therapist, Tomorrow, 20));

        Appointment second = _service.Book(new BookingRequest(2, FakeClinicDirectory.Therapist, Tomorrow, 20));

        Assert.Equal(new TimeOnly(8, 20), second.StartTime);
        Assert.Equal(first.Id + 1, second.Id);
    }

    [Fact]
    public void Book_PatientBusyWithAnotherDoctor_DoesNotOverlap()
    {
        // пацієнт 1 зайнятий у кардіолога 09:00-09:30, терапевт зайнятий 08:00-09:00
        _service.Book(new BookingRequest(1, FakeClinicDirectory.Cardiologist, Tomorrow, 30));
        _service.Book(new BookingRequest(2, FakeClinicDirectory.Therapist, Tomorrow, 60));

        Appointment a = _service.Book(new BookingRequest(1, FakeClinicDirectory.Therapist, Tomorrow, 20));

        Assert.Equal(new TimeOnly(9, 40), a.StartTime);
    }

    [Fact]
    public void Book_Today_StartsNotEarlierThanNow()
    {
        Appointment a = _service.Book(new BookingRequest(1, FakeClinicDirectory.Therapist, Today, 20));

        Assert.Equal(new TimeOnly(9, 20), a.StartTime);
    }

    [Theory]
    [InlineData(42, FakeClinicDirectory.Therapist, 1, "patient.exists")]
    [InlineData(1, "DOC-NONE-01", 1, "doctor.exists")]
    [InlineData(1, FakeClinicDirectory.Therapist, -1, "appointment.date")]
    public void Book_InvalidRequest_ThrowsDomainRule(int patientId, string doctorCode, int dayOffset, string rule)
    {
        BookingRequest request = new(patientId, doctorCode, Today.AddDays(dayOffset), 20);

        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => _service.Book(request));

        Assert.Equal(rule, ex.Rule);
    }

    [Fact]
    public void Book_NoTimeLeft_ThrowsSlotFree()
    {
        BookingRequest request = new(1, FakeClinicDirectory.Therapist, Tomorrow, 140);

        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => _service.Book(request));

        Assert.Equal("slot.free", ex.Rule);
    }

    [Fact]
    public void Reschedule_ConfirmedVisit_MovesAndNeedsNewConfirmation()
    {
        Appointment a = _service.Book(new BookingRequest(1, FakeClinicDirectory.Therapist, Tomorrow, 20));
        _service.ChangeStatus(a.Id, StatusAction.Confirm);

        _service.Reschedule(a.Id, Tomorrow.AddDays(1));

        Assert.Equal(Tomorrow.AddDays(1), a.VisitDate);
        Assert.Equal(VisitStatus.Scheduled, a.Status);
    }

    [Fact]
    public void Reschedule_SameDay_KeepsOwnSlotFree()
    {
        Appointment a = _service.Book(new BookingRequest(1, FakeClinicDirectory.Therapist, Tomorrow, 20));

        _service.Reschedule(a.Id, Tomorrow);

        Assert.Equal(new TimeOnly(8, 0), a.StartTime);
    }

    [Fact]
    public void Cancel_ScheduledVisit_FreesSlotForOthers()
    {
        Appointment a = _service.Book(new BookingRequest(1, FakeClinicDirectory.Therapist, Tomorrow, 20));

        _service.Cancel(a.Id, "пацієнт захворів");
        Appointment next = _service.Book(new BookingRequest(2, FakeClinicDirectory.Therapist, Tomorrow, 20));

        Assert.Equal(VisitStatus.Cancelled, a.Status);
        Assert.Equal(new TimeOnly(8, 0), next.StartTime);
    }

    [Fact]
    public void ChangeStatus_MissedBeforeVisitStarts_ThrowsDomainRule()
    {
        Appointment a = _service.Book(new BookingRequest(1, FakeClinicDirectory.Therapist, Tomorrow, 20));
        _service.ChangeStatus(a.Id, StatusAction.Confirm);

        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => _service.ChangeStatus(a.Id, StatusAction.MarkMissed));

        Assert.Equal("appointment.missed", ex.Rule);
    }

    [Fact]
    public void AddService_OpenVisit_ReturnsNewTotal()
    {
        Appointment a = _service.Book(new BookingRequest(1, FakeClinicDirectory.Therapist, Tomorrow, 20));

        decimal total = _service.AddService(a.Id, new VisitItem("LAB-02", 1, 180m, 10));

        Assert.Equal(630m, total);
    }
}

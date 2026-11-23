using ClinicReception.Core.Domain;
using ClinicReception.Core.Errors;
using ClinicReception.Core.Notifications;
using ClinicReception.Core.Pricing;
using ClinicReception.Core.Services;
using ClinicReception.Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicReception.Tests;

public class AppointmentServiceTests
{
    private readonly InMemoryAppointmentRepository _repository = new();
    private readonly AppointmentService _service;

    public AppointmentServiceTests()
    {
        _service = new AppointmentService(
            _repository,
            new StandardPricingPolicy(),
            new TextWriterNotifier(TextWriter.Null),
            NullLogger<AppointmentService>.Instance);
    }

    [Fact]
    public void Confirm_UnknownId_ThrowsWithRule()
    {
        DomainRuleException error = Assert.Throws<DomainRuleException>(() => _service.Confirm(7));

        Assert.Equal("appointment.exists", error.Rule);
        Assert.Contains("7", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Confirm_EmptyAppointment_ThrowsWithRule()
    {
        _repository.Add(new Appointment(1, 10, "DOC-THER-01", new DateOnly(2026, 3, 10), new TimeOnly(9, 30)));

        DomainRuleException error = Assert.Throws<DomainRuleException>(() => _service.Confirm(1));

        Assert.Equal("appointment.items", error.Rule);
    }

    [Fact]
    public void AddItem_UnknownId_ThrowsWithRule()
    {
        DomainRuleException error = Assert.Throws<DomainRuleException>(
            () => _service.AddItem(3, new VisitItem("CONS-01", 1, 650m, 20)));

        Assert.Equal("appointment.exists", error.Rule);
    }

    [Fact]
    public void Cancel_TwiceSameAppointment_SecondThrowsTransition()
    {
        _repository.Add(new Appointment(2, 10, "DOC-THER-01", new DateOnly(2026, 3, 10), new TimeOnly(10, 0)));
        _service.Cancel(2, "пацієнт захворів");

        DomainRuleException error = Assert.Throws<DomainRuleException>(() => _service.Cancel(2, "повторно"));

        Assert.Equal("appointment.transition", error.Rule);
    }
}

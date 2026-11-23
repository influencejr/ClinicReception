using ClinicReception.Core.Domain;
using ClinicReception.Core.Errors;
using ClinicReception.Core.Notifications;
using ClinicReception.Core.Pricing;
using ClinicReception.Core.Services;
using ClinicReception.Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicReception.Tests;

public class DomainInvariantTests
{
    [Fact]
    public void VisitItem_ZeroQuantity_ThrowsOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new VisitItem("CONS-01", 0, 250m, 20));
    }

    [Fact]
    public void Complete_NotConfirmed_ThrowsDomainRule()
    {
        Appointment appointment = NewAppointment(1);
        appointment.AddItem(new VisitItem("CONS-01", 1, 250m, 20));

        DomainRuleException error = Assert.Throws<DomainRuleException>(() => appointment.Complete());
        Assert.Equal("appointment.transition", error.Rule);
        Assert.Equal(VisitStatus.Scheduled, appointment.Status);
    }

    [Fact]
    public void TotalOf_TenPercentDiscount_Returns180()
    {
        AppointmentService service = new(
            new InMemoryAppointmentRepository(),
            new DiscountPricingPolicy(0.10m),
            new TextWriterNotifier(TextWriter.Null),
            NullLogger<AppointmentService>.Instance);
        Appointment appointment = NewAppointment(2);
        appointment.AddItem(new VisitItem("CONS-01", 2, 100m, 20));

        Assert.Equal(180m, service.TotalOf(appointment));
    }

    private static Appointment NewAppointment(int id) =>
        new(id, 10, "DOC-THER-01", new DateOnly(2026, 3, 10), new TimeOnly(9, 30));
}

using ClinicReception.Core.Billing;
using ClinicReception.Core.Domain;

namespace ClinicReception.Tests;

public class VisitTotalCalculatorTests
{
    private static DateOnly Tuesday => new(2026, 3, 10);

    [Fact]
    public void Calculate_RegularPatient_Returns1292()
    {
        TotalRequest request = new(
            VisitId: 1,
            Patient: new PatientInfo("Іваненко", "i@ex.com", IsRegular: true),
            Items: new List<VisitItem>
            {
                new("PHYS-01", 12, 100m, 30),
                new("CONS-01", 1, 250m, 20),
            },
            VisitDate: Tuesday,
            PromoCode: null,
            Status: VisitStatus.Scheduled,
            RegistrationFee: 60m);

        TotalResult result = VisitTotalCalculator.Calculate(request);

        Assert.Equal(1292.70m, result.Total);
        Assert.Equal(VisitStatus.Confirmed, result.Status);
    }

    [Fact]
    public void Calculate_SmallVisit_AddsRegistrationFee()
    {
        TotalRequest request = new(
            VisitId: 2,
            Patient: new PatientInfo("Петренко", "p@ex.com", IsRegular: false),
            Items: new List<VisitItem>
            {
                new("MASS-01", 2, 200m, 40),
            },
            VisitDate: Tuesday,
            PromoCode: PricingRules.CheckupPromoCode,
            Status: VisitStatus.Confirmed,
            RegistrationFee: 60m);

        TotalResult result = VisitTotalCalculator.Calculate(request);

        Assert.Equal(420m, result.Total);
        Assert.Equal(VisitStatus.Completed, result.Status);
    }

    [Fact]
    public void Calculate_NoItems_ThrowsArgument()
    {
        TotalRequest request = new(
            VisitId: 3,
            Patient: new PatientInfo("Коваль", "k@ex.com", IsRegular: false),
            Items: new List<VisitItem>(),
            VisitDate: Tuesday,
            PromoCode: null,
            Status: VisitStatus.Scheduled,
            RegistrationFee: 60m);

        Assert.Throws<ArgumentException>(() => VisitTotalCalculator.Calculate(request));
    }
}

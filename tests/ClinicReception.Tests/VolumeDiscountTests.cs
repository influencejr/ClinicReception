using ClinicReception.Core.Billing;

namespace ClinicReception.Tests;

public class VolumeDiscountTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1500, 0)]
    [InlineData(2000, 0)]
    [InlineData(2000.01, 100.0005)]
    [InlineData(3000, 150)]
    [InlineData(5000, 250)]
    [InlineData(5000.01, 500.001)]
    [InlineData(8000, 800)]
    public void VolumeDiscountOf_Boundaries_MatchesTable(double amount, double expected)
    {
        decimal actual = VisitTotalCalculator.VolumeDiscountOf((decimal)amount);

        Assert.Equal((decimal)expected, actual);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1000)]
    public void VolumeDiscountOf_NegativeAmount_ThrowsOutOfRange(double amount)
    {
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(
            () => VisitTotalCalculator.VolumeDiscountOf((decimal)amount));

        Assert.Equal("amount", error.ParamName);
    }

    [Fact]
    public void VolumeDiscountOf_HugeVisit_IsCappedAtMax()
    {
        decimal actual = VisitTotalCalculator.VolumeDiscountOf(20000m);

        Assert.Equal(1000m, actual);
    }
}

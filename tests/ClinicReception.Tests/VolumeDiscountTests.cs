using ClinicReception.Core.Billing;

namespace ClinicReception.Tests;

public class VolumeDiscountTests
{
    [Fact]
    public void VolumeDiscountOf_NegativeAmount_Throws()
    {
        // Arrange
        decimal amount = -0.01m;

        // Act
        Exception? error = Record.Exception(() => VisitTotalCalculator.VolumeDiscountOf(amount));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(error);
    }

    [Fact]
    public void VolumeDiscountOf_Zero_ReturnsZero()
    {
        decimal amount = 0m;

        decimal actual = VisitTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(0m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_ExactlyMediumThreshold_ReturnsZero()
    {
        decimal amount = 2000m;

        decimal actual = VisitTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(0m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_JustAboveMediumThreshold_ReturnsFivePercent()
    {
        decimal amount = 2000.01m;

        decimal actual = VisitTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(100.0005m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_MediumVisit_ReturnsFivePercent()
    {
        decimal amount = 3000m;

        decimal actual = VisitTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(150m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_ExactlyLargeThreshold_ReturnsFivePercent()
    {
        decimal amount = 5000m;

        decimal actual = VisitTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(250m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_JustAboveLargeThreshold_ReturnsTenPercent()
    {
        decimal amount = 5000.01m;

        decimal actual = VisitTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(500.001m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_LargeVisit_ReturnsTenPercent()
    {
        decimal amount = 8000m;

        decimal actual = VisitTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(800m, actual);
    }
}

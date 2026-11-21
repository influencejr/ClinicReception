using ClinicReception.Core.Domain;

namespace ClinicReception.Tests;

public class MoneyTests
{
    [Theory]
    [InlineData(0.125, 0.13)]
    [InlineData(0.135, 0.14)]
    [InlineData(-0.125, -0.13)]
    [InlineData(970.5, 970.5)]
    [InlineData(1292.704, 1292.70)]
    public void Round_Amount_RoundsHalfAwayFromZero(double amount, double expected)
    {
        decimal rounded = Money.Round((decimal)amount);

        Assert.Equal((decimal)expected, rounded);
    }
}

using ClinicReception.Core.Billing;
using ClinicReception.Core.Domain;

namespace ClinicReception.Tests;

public class VisitGuardsTests
{
    [Fact]
    public void EnsureItemsValid_NoItems_ThrowsArgument()
    {
        Assert.Throws<ArgumentException>(
            () => VisitGuards.EnsureItemsValid(new List<VisitItem>()));
    }

    [Fact]
    public void EnsureItemValid_ZeroQuantity_ThrowsOutOfRange()
    {
        VisitItem item = new() { ServiceCode = "CONS-01", Quantity = 0, UnitPrice = 10m };

        Assert.Throws<ArgumentOutOfRangeException>(() => VisitGuards.EnsureItemValid(item));
    }
}

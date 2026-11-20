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
    public void EnsureItemsValid_NullItem_ThrowsArgumentNull()
    {
        List<VisitItem> items = new() { new("CONS-01", 1, 250m, 20), null! };

        Assert.Throws<ArgumentNullException>(() => VisitGuards.EnsureItemsValid(items));
    }
}

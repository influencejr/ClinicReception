using ClinicReception.App.Input;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Errors;

namespace ClinicReception.Tests;

public class VisitItemParserTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("LAB-02 1 180,50")]
    [InlineData("LAB-02 0 180,50 10")]
    [InlineData("LAB-02 -2 180,50 10")]
    [InlineData("LAB-02 51 180,50 10")]
    [InlineData("LAB-02 1 -180,50 10")]
    [InlineData("LAB-02 1 100000,01 10")]
    [InlineData("LAB-02 1 сто 10")]
    [InlineData("LAB-02 1 180,50 481")]
    [InlineData("CODE-LONGER-THAN-20-CH 1 180,50 10")]
    public void Parse_InvalidLine_ReturnsFailure(string line)
    {
        Result<VisitItem> result = VisitItemParser.Parse(line);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotEmpty(result.Error);
    }

    [Theory]
    [InlineData("lab-02 1 180,50 10", 180.50)]
    [InlineData("LAB-02 50 180.50 480", 180.50)]
    [InlineData("LAB-02 1 100000 10", 100000)]
    public void Parse_ValidLine_ReturnsItem(string line, double price)
    {
        Result<VisitItem> result = VisitItemParser.Parse(line);

        Assert.True(result.IsSuccess);
        Assert.Equal("LAB-02", result.Value!.ServiceCode);
        Assert.Equal((decimal)price, result.Value.UnitPrice);
    }
}

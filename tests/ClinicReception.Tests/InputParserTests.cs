using ClinicReception.App.Input;
using ClinicReception.Core.Services;

namespace ClinicReception.Tests;

// Барикада вводу: некоректний рядок дає пояснення, а не виняток.
public class InputParserTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("-3")]
    [InlineData("0")]
    [InlineData("п'ять")]
    public void PositiveNumber_InvalidText_Fails(string? text)
    {
        Assert.False(InputParser.PositiveNumber(text).IsSuccess);
    }

    [Fact]
    public void PositiveNumber_WithSpaces_ParsesNumber()
    {
        Assert.Equal(42, InputParser.PositiveNumber(" 42 ").Value);
    }

    [Theory]
    [InlineData("24.11.2026", true)]
    [InlineData("2026-11-24", false)]
    [InlineData("31.02.2026", false)]
    [InlineData(null, false)]
    public void Date_Text_ParsesOnlyDayMonthYear(string? text, bool ok)
    {
        Assert.Equal(ok, InputParser.Date(text).IsSuccess);
    }

    [Theory]
    [InlineData("doc-ther-01", true)]
    [InlineData("DOC THER", false)]
    [InlineData("   ", false)]
    public void DoctorCode_Text_AcceptsCodeWithoutSpaces(string text, bool ok)
    {
        Assert.Equal(ok, InputParser.DoctorCode(text).IsSuccess);
    }

    [Theory]
    [InlineData("20", true)]
    [InlineData("481", false)]
    public void Minutes_Text_LimitedByLongestService(string text, bool ok)
    {
        Assert.Equal(ok, InputParser.Minutes(text).IsSuccess);
    }

    [Fact]
    public void Action_Three_IsMarkMissed()
    {
        Assert.Equal(StatusAction.MarkMissed, InputParser.Action("3").Value);
    }
}

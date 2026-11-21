using ClinicReception.Core.Reporting;

namespace ClinicReception.Tests;

public class ReportBuilderTests
{
    [Fact]
    public void Build_TwoRows_TitleInUpperCaseThenRows()
    {
        ReportBuilder builder = new() { Title = "Прийом" };

        string text = builder.Build(["CONS-01", "ECG-01"]);

        string[] lines = text.Split(Environment.NewLine);
        Assert.Equal(["ПРИЙОМ", "CONS-01", "ECG-01", string.Empty], lines);
    }

    [Fact]
    public void Build_NullRows_ThrowsArgumentNull()
    {
        ReportBuilder builder = new();

        Assert.Throws<ArgumentNullException>(() => builder.Build(null!));
    }
}

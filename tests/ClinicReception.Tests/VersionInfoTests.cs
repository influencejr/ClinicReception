using ClinicReception.Core;

namespace ClinicReception.Tests;

public class VersionInfoTests
{
    [Fact]
    public void Current_FromBuildProps_IsSemanticVersion()
    {
        string version = VersionInfo.Current();

        Assert.Matches(@"^\d+\.\d+\.\d+$", version);
    }
}

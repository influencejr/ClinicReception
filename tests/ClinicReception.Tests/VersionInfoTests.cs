using ClinicReception.Core;

namespace ClinicReception.Tests;

public class VersionInfoTests
{
    [Fact]
    public void Current_FromBuildProps_IsSemanticVersion()
    {
        string version = VersionInfo.Current();

        // SemVer дозволяє суфікс попередньої версії: 0.9.0-beta.1
        Assert.Matches(@"^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$", version);
    }
}

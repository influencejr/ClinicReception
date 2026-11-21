using System.Reflection;

namespace ClinicReception.Core;

/// <summary>Версія збірки, задана у Directory.Build.props.</summary>
public static class VersionInfo
{
    /// <summary>
    /// Повертає версію без хеша коміту, який SDK дописує після знака «+».
    /// </summary>
    /// <returns>Версія у форматі МАЖОР.МІНОР.ПАТЧ.</returns>
    public static string Current()
    {
        Assembly assembly = typeof(VersionInfo).Assembly;
        AssemblyInformationalVersionAttribute? attribute =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        string raw = attribute?.InformationalVersion ?? "0.0.0";
        int plus = raw.IndexOf('+', StringComparison.Ordinal);
        return plus < 0 ? raw : raw[..plus];
    }
}

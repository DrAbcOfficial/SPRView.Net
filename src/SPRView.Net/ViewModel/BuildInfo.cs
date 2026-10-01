using System.Reflection;

namespace SPRView.Net.ViewModel;

/// <summary>
/// Version and build time of the running binary.
///
/// Both values are written into the assembly as metadata while the build runs,
/// from version.txt and the build clock at the repository root, so they are
/// fixed into the executable rather than read from a file next to it. That
/// matters for the single file NativeAOT releases, which ship no sidecar.
/// </summary>
public static class BuildInfo
{
    /// <summary>Release version, e.g. "2026.10.1".</summary>
    public static string Version { get; } = ReadMetadata("ReleaseVersion", "0.0.0-dev");

    /// <summary>Local time the binary was built, e.g. "2026-10-01 18:25:04".</summary>
    public static string BuildTime { get; } = ReadMetadata("BuildTime", "unknown");

    /// <summary>
    /// Reads one assembly metadata entry.
    ///
    /// GetCustomAttributes is used rather than GetCustomAttribute because the
    /// latter needs a constructed generic type that the trimmer cannot always
    /// prove is reachable, which shows up as a NativeAOT warning.
    /// </summary>
    private static string ReadMetadata(string key, string fallback)
    {
        foreach (object attribute in typeof(BuildInfo).Assembly.GetCustomAttributes(false))
        {
            if (attribute is AssemblyMetadataAttribute metadata &&
                metadata.Key == key &&
                !string.IsNullOrWhiteSpace(metadata.Value))
            {
                return metadata.Value;
            }
        }
        return fallback;
    }
}

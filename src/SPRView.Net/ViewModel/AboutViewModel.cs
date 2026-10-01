namespace SPRView.Net.ViewModel;

/// <summary>
/// What the About dialog shows: the localized labels plus the version and build
/// time of the running binary.
///
/// The two are kept apart on purpose. <see cref="LangViewModel"/> is loaded from
/// the language files at run time, while the build values are baked into the
/// assembly, so folding them into one type would suggest they come from the same
/// place.
/// </summary>
public sealed class AboutViewModel(LangViewModel lang)
{
    /// <summary>Localized captions for the dialog.</summary>
    public LangViewModel Lang { get; } = lang;

    /// <summary>Release version, from version.txt at build time.</summary>
    public string Version { get; } = BuildInfo.Version;

    /// <summary>Build timestamp, from the build machine's clock.</summary>
    public string BuildTime { get; } = BuildInfo.BuildTime;

    /// <summary>Version line as shown, e.g. "Version 2026.10.1".</summary>
    public string VersionLabel => string.Format(Lang.About_Version, Version);

    /// <summary>Build line as shown, e.g. "Built 2026-10-01 18:25:04".</summary>
    public string BuildTimeLabel => string.Format(Lang.About_BuildTime, BuildTime);
}

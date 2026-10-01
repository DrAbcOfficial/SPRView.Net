namespace SPRView.Net.ThumbnailProvider.Windows.Com;

/// <summary>
/// COM class and interface identifiers.
/// </summary>
internal static class ComIds
{
    /// <summary>
    /// Class id of the SPRView thumbnail provider. Kept identical to the
    /// legacy C++ implementation so existing registrations upgrade in place.
    /// </summary>
    public static readonly Guid ClsidSpriteThumbnailProvider =
        new("4D555153-67DE-4350-860D-671B7618B83B");

    public const string ClsidSpriteThumbnailProviderString =
        "{4D555153-67DE-4350-860D-671B7618B83B}";

    public const string ProviderDisplayName = "SPRView Thumbnail Preview";

    public static readonly Guid IidIUnknown = new("00000000-0000-0000-C000-000000000046");
    public static readonly Guid IidIClassFactory = new("00000001-0000-0000-C000-000000000046");

    /// <summary>IThumbnailProvider, the shell thumbnail handler interface.</summary>
    public static readonly Guid IidIThumbnailProvider = new("E357FCCD-A995-4576-B01F-234630154E96");

    /// <summary>IInitializeWithStream.</summary>
    public static readonly Guid IidIInitializeWithStream = new("FC4801A3-2BA9-4A23-8181-64892B0EFD86");

    /// <summary>Registry subkey under .spr that selects the thumbnail handler.</summary>
    public const string ShellExThumbnailHandlerKey =
        @"Software\Classes\.spr\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}";
}

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SPRView.Net.ThumbnailProvider.Windows.Com;

/// <summary>
/// The ISequentialStream subset needed to drain the file. Member signatures
/// must match the COM ABI exactly (ULONG = 4 byte); only declare what is
/// actually called so vtable slots line up.
/// </summary>
[GeneratedComInterface]
[Guid("0000000C-0000-0000-C000-000000000046")]
internal partial interface ISequentialStreamShim
{
    [PreserveSig]
    int Read(nint pv, uint cb, nint pcbRead);
}

/// <summary>Shell initialization contract handing us the file contents.</summary>
[GeneratedComInterface]
[Guid("FC4801A3-2BA9-4A23-8181-64892B0EFD86")]
internal partial interface IInitializeWithStreamShim
{
    [PreserveSig]
    int Initialize(ISequentialStreamShim? stream, uint grfMode);
}

/// <summary>Shell thumbnail request.</summary>
[GeneratedComInterface]
[Guid("E357FCCD-A995-4576-B01F-234630154E96")]
internal partial interface IThumbnailProviderShim
{
    [PreserveSig]
    int GetThumbnail(uint cx, out nint phbmp, out WtsAlphaType pdwAlpha);
}

/// <summary>WTS_ALPHATYPE values understood by the shell.</summary>
internal enum WtsAlphaType
{
    WTSAT_UNKNOWN = 0,
    WTSAT_RGB = 1,
    WTSAT_ARGB = 2
}

/// <summary>Standard COM class factory.</summary>
[GeneratedComInterface]
[Guid("00000001-0000-0000-C000-000000000046")]
internal partial interface IClassFactoryShim
{
    [PreserveSig]
    int CreateInstance(nint punkOuter, Guid riid, out nint ppv);

    [PreserveSig]
    int LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock);
}

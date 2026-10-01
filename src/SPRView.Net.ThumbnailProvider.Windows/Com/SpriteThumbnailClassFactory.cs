using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SPRView.Net.ThumbnailProvider.Windows.Com;

/// <summary>
/// Hands Explorer managed instances of <see cref="SpriteThumbnailProvider"/>.
/// </summary>
[GeneratedComClass]
internal sealed partial class SpriteThumbnailClassFactory : IClassFactoryShim
{
    private const int S_OK = 0;
    private const int CLASS_E_NOAGGREGATION = unchecked((int)0x80040110);
    private const int E_OUTOFMEMORY = unchecked((int)0x8007000E);

    public int CreateInstance(nint punkOuter, Guid riid, out nint ppv)
    {
        ppv = 0;
        if (punkOuter != 0)
            return CLASS_E_NOAGGREGATION;
        try
        {
            nint unknown = ComWrappersSource.Instance.GetOrCreateComInterfaceForObject(
                new SpriteThumbnailProvider(), CreateComInterfaceFlags.None);
            if (riid == ComIds.IidIUnknown)
            {
                ppv = unknown;
                return S_OK;
            }
            int hr = Marshal.QueryInterface(unknown, in riid, out ppv);
            Marshal.Release(unknown);
            return hr;
        }
        catch
        {
            return E_OUTOFMEMORY;
        }
    }

    public int LockServer(bool fLock) => S_OK;
}

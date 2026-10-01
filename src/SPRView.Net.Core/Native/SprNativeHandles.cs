using System.Runtime.InteropServices;

namespace SPRView.Net.Core.Native;

/// <summary>
/// Translates opaque native handles into <see cref="SprDocument"/> references.
/// </summary>
internal static class SprNativeHandles
{
    public static nint Create(SprDocument document)
    {
        GCHandle handle = GCHandle.Alloc(document);
        return GCHandle.ToIntPtr(handle);
    }

    public static SprDocument? Resolve(nint handle)
    {
        if (handle == 0)
            return null;
        return GCHandle.FromIntPtr(handle).Target as SprDocument;
    }

    public static void Destroy(nint handle)
    {
        if (handle != 0)
            GCHandle.FromIntPtr(handle).Free();
    }
}

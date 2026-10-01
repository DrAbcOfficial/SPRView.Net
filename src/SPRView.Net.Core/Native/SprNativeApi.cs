using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace SPRView.Net.Core.Native;

/// <summary>
/// The C ABI exported when this assembly is published as a NativeAOT shared
/// library (<c>dotnet publish -r &lt;rid&gt; -p:NativeLib=Shared</c>).
/// Layout and semantics are documented in Native/sprview_core.h; keep both in
/// sync.
/// </summary>
internal static unsafe class SprNativeApi
{
    internal const int AbiVersionValue = 1;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SprInfo
    {
        public uint Frames;
        public uint MaxFrameWidth;
        public uint MaxFrameHeight;
        public int Type;
        public int Format;
        public int Synchronization;
        public float BoundRadius;
        public float BeamLength;
        public int PaletteSize;
    }

    [UnmanagedCallersOnly(EntryPoint = "sprview_abi_version")]
    internal static int AbiVersion() => AbiVersionValue;

    /// <summary>Open a .spr file. Returns an opaque handle, 0 on failure.</summary>
    [UnmanagedCallersOnly(EntryPoint = "sprview_open")]
    internal static nint Open(byte* pathUtf8)
    {
        try
        {
            if (pathUtf8 == null)
                return 0;
            string? path = Marshal.PtrToStringUTF8((nint)pathUtf8);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return 0;
            return SprNativeHandles.Create(SprDocument.Load(path));
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>Open a .spr from an existing stream handle with known length.</summary>
    [UnmanagedCallersOnly(EntryPoint = "sprview_open_memory")]
    internal static nint OpenMemory(byte* data, nint length)
    {
        try
        {
            if (data == null || length <= 0)
                return 0;
            using UnmanagedMemoryStream stream =
                new(data, (long)length, (long)length, FileAccess.Read);
            return SprNativeHandles.Create(SprDocument.Load(stream));
        }
        catch
        {
            return 0;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "sprview_close")]
    internal static void Close(nint handle)
    {
        SprNativeHandles.Destroy(handle);
    }

    [UnmanagedCallersOnly(EntryPoint = "sprview_get_info")]
    internal static int GetInfo(nint handle, SprInfo* info)
    {
        if (info == null)
            return (int)SprNativeResult.InvalidArgument;
        if (SprNativeHandles.Resolve(handle) is not { } document)
            return (int)SprNativeResult.InvalidHandle;
        try
        {
            info->Frames = document.Header.NumberOfFrames;
            info->MaxFrameWidth = document.Header.MaxFrameWidth;
            info->MaxFrameHeight = document.Header.MaxFrameHeight;
            info->Type = (int)document.Header.Type;
            info->Format = (int)document.Header.Format;
            info->Synchronization = (int)document.Header.Synchronization;
            info->BoundRadius = document.Header.BoundRadius;
            info->BeamLength = document.Header.BeamLength;
            info->PaletteSize = document.Palette.Length;
            return (int)SprNativeResult.Ok;
        }
        catch
        {
            return (int)SprNativeResult.UnknownError;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "sprview_get_frame_info")]
    internal static int GetFrameInfo(nint handle, int frame, int* width, int* height, int* originX, int* originY)
    {
        if (SprNativeHandles.Resolve(handle) is not { } document)
            return (int)SprNativeResult.InvalidHandle;
        if (frame < 0 || frame >= document.Frames.Count)
            return (int)SprNativeResult.FrameOutOfRange;
        SprFrame sprFrame = document.Frames[frame];
        if (width != null)
            *width = sprFrame.Width;
        if (height != null)
            *height = sprFrame.Height;
        if (originX != null)
            *originX = sprFrame.OriginX;
        if (originY != null)
            *originY = sprFrame.OriginY;
        return (int)SprNativeResult.Ok;
    }

    /// <summary>Copy one frame as tightly packed RGBA8888 pixels.</summary>
    [UnmanagedCallersOnly(EntryPoint = "sprview_read_frame_rgba")]
    internal static int ReadFrameRgba(nint handle, int frame, byte* buffer, nint bufferSize)
    {
        if (buffer == null)
            return (int)SprNativeResult.InvalidArgument;
        if (SprNativeHandles.Resolve(handle) is not { } document)
            return (int)SprNativeResult.InvalidHandle;
        if (frame < 0 || frame >= document.Frames.Count)
            return (int)SprNativeResult.FrameOutOfRange;
        try
        {
            SprFrame sprFrame = document.Frames[frame];
            long needed = (long)sprFrame.Width * sprFrame.Height * 4;
            if (bufferSize < needed)
                return (int)SprNativeResult.BufferTooSmall;
            byte[] rgba = sprFrame.DecodeToRgba();
            fixed (byte* source = rgba)
                Buffer.MemoryCopy(source, buffer, needed, needed);
            return (int)SprNativeResult.Ok;
        }
        catch
        {
            return (int)SprNativeResult.UnknownError;
        }
    }

    /// <summary>
    /// Render one frame, scaled to fit <paramref name="maxSize"/>, encoded as
    /// PNG. The caller releases the buffer with <c>sprview_free_buffer</c>.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "sprview_render_png")]
    internal static int RenderPng(nint handle, int frame, int maxSize, byte** outData, nint* outSize)
    {
        if (outData == null || outSize == null)
            return (int)SprNativeResult.InvalidArgument;
        *outData = null;
        *outSize = 0;
        if (SprNativeHandles.Resolve(handle) is not { } document)
            return (int)SprNativeResult.InvalidHandle;
        if (frame < 0 || frame >= document.Frames.Count)
            return (int)SprNativeResult.FrameOutOfRange;
        if (maxSize <= 0)
            return (int)SprNativeResult.InvalidArgument;
        try
        {
            SprFrame sprFrame = document.Frames[frame];
            float scale = Math.Min((float)maxSize / sprFrame.Width, (float)maxSize / sprFrame.Height);
            int width = Math.Max(1, (int)Math.Round(sprFrame.Width * scale));
            int height = Math.Max(1, (int)Math.Round(sprFrame.Height * scale));
            using Image image = sprFrame.Decode();
            image.Mutate(context => context.Resize(width, height, KnownResamplers.NearestNeighbor));
            using MemoryStream png = new();
            image.SaveAsPng(png);
            byte* buffer = (byte*)NativeMemory.Alloc((nuint)png.Length);
            png.TryGetBuffer(out ArraySegment<byte> segment);
            fixed (byte* source = segment.Array!)
                Buffer.MemoryCopy(source, buffer, png.Length, png.Length);
            *outData = buffer;
            *outSize = (nint)png.Length;
            return (int)SprNativeResult.Ok;
        }
        catch
        {
            return (int)SprNativeResult.UnknownError;
        }
    }

    /// <summary>Release a buffer returned by <c>sprview_render_png</c>.</summary>
    [UnmanagedCallersOnly(EntryPoint = "sprview_free_buffer")]
    internal static void FreeBuffer(void* buffer)
    {
        if (buffer != null)
            NativeMemory.Free(buffer);
    }
}

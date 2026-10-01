using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;
using SPRView.Net.Core;

namespace SPRView.Net.ThumbnailProvider.Windows.Com;

/// <summary>
/// The shell extension object: receives the .spr contents as a stream and
/// renders the first frame into a 32 bit ARGB DIB for Explorer.
/// </summary>
[GeneratedComClass]
internal sealed partial class SpriteThumbnailProvider : IInitializeWithStreamShim, IThumbnailProviderShim
{
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private const int E_UNEXPECTED = unchecked((int)0x8000FFFF);

    private byte[]? m_pSprBytes;

    public int Initialize(ISequentialStreamShim? stream, uint grfMode)
    {
        if (stream == null)
            return E_UNEXPECTED;
        try
        {
            m_pSprBytes = StreamShimReader.ReadAll(stream);
            return S_OK;
        }
        catch
        {
            // Shell retries other handlers when S_FALSE is returned.
            m_pSprBytes = null;
            return S_FALSE;
        }
    }

    public unsafe int GetThumbnail(uint cx, out nint phbmp, out WtsAlphaType pdwAlpha)
    {
        phbmp = 0;
        pdwAlpha = WtsAlphaType.WTSAT_UNKNOWN;
        try
        {
            byte[] sprBytes = m_pSprBytes ?? throw new InvalidOperationException("Provider was not initialized");
            using MemoryStream input = new(sprBytes, writable: false);
            SprDocument document = SprDocument.Load(input);
            SprFrame frame = document.Frames[0];

            int width = frame.Width;
            int height = frame.Height;
            byte[] rgba = frame.DecodeToRgba();

            nint bitmap = DibHelper.Create32bppArgbDib(width, height, rgba);
            if (bitmap == 0)
                return E_UNEXPECTED;
            phbmp = bitmap;
            pdwAlpha = WtsAlphaType.WTSAT_ARGB;
            return S_OK;
        }
        catch
        {
            return S_FALSE;
        }
    }
}

/// <summary>
/// Drains an ISequentialStream into a byte array using raw vtable calls.
/// </summary>
internal static unsafe class StreamShimReader
{
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private const uint ChunkSize = 64 * 1024;

    public static byte[] ReadAll(ISequentialStreamShim stream)
    {
        using MemoryStream output = new();
        byte[] chunk = GC.AllocateUninitializedArray<byte>((int)ChunkSize);
        fixed (byte* chunkPtr = chunk)
        {
            uint* readCount = (uint*)NativeMemory.AllocZeroed((nuint)sizeof(uint));
            try
            {
                while (true)
                {
                    int hr = stream.Read((nint)chunkPtr, ChunkSize, (nint)readCount);
                    // IStream::Read reports a short final chunk as S_FALSE.
                    if (hr is not (S_OK or S_FALSE))
                        throw new IOException($"IStream.Read failed with hr=0x{hr:X8}");
                    uint read = *readCount;
                    if (read == 0)
                        break;
                    output.Write(chunk, 0, (int)Math.Min(read, ChunkSize));
                }
            }
            finally
            {
                NativeMemory.Free(readCount);
            }
        }
        return output.ToArray();
    }
}

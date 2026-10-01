using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SPRView.Net.Core;

/// <summary>
/// Expands palette indexed frame data into full color pixel buffers.
/// </summary>
internal static class FrameDecoder
{
    /// <summary>
    /// Decode indexed data into an RGBA image.
    /// </summary>
    public static Image<Rgba32> Decode(byte[] indexedData, SprPalette palette, int width, int height)
    {
        ValidateLength(indexedData, width, height);

        Image<Rgba32> image = new(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                image[x, y] = palette[indexedData[(y * width) + x]];
        return image;
    }

    /// <summary>
    /// Decode indexed data into a tightly packed RGBA8888 buffer.
    /// </summary>
    public static byte[] DecodeToRgba(byte[] indexedData, SprPalette palette, int width, int height)
    {
        ValidateLength(indexedData, width, height);

        byte[] rgba = new byte[width * height * 4];
        for (int i = 0; i < width * height; i++)
        {
            Rgba32 color = palette[indexedData[i]];
            int o = i * 4;
            rgba[o] = color.R;
            rgba[o + 1] = color.G;
            rgba[o + 2] = color.B;
            rgba[o + 3] = color.A;
        }
        return rgba;
    }

    private static void ValidateLength(byte[] indexedData, int width, int height)
    {
        if (indexedData.Length < width * height)
            throw new InvalidDataException("Frame data is shorter than its declared dimensions");
    }
}

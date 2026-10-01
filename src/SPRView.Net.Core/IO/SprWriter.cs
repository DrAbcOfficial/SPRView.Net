using SixLabors.ImageSharp.PixelFormats;

namespace SPRView.Net.Core;

/// <summary>
/// Writes the GoldSrc sprite binary layout for composed indexed frames.
/// </summary>
internal static class SprWriter
{
    public static void Write(
        Stream output,
        SprHeader header,
        List<SixLabors.ImageSharp.PixelFormats.Rgba32> orderedColors,
        List<(int Width, int Height, byte[] IndexedData)> frames)
    {
        using BinaryWriter writer = new(output, System.Text.Encoding.UTF8, leaveOpen: true);

        // Header
        writer.Write(SprHeader.Signature);
        writer.Write(SprHeader.SupportedVersion);
        writer.Write((int)header.Type);
        writer.Write((int)header.Format);
        float radius = (float)Math.Sqrt(Math.Pow(header.MaxFrameWidth, 2) + Math.Pow(header.MaxFrameHeight, 2)) / 2;
        writer.Write(radius);
        writer.Write((int)header.MaxFrameWidth);
        writer.Write((int)header.MaxFrameHeight);
        writer.Write(frames.Count);
        writer.Write(header.BeamLength);
        writer.Write((int)header.Synchronization);

        WritePalette(writer, orderedColors, header.Format == SpriteFormat.AlphaTest);

        foreach (var (width, height, indexedData) in frames)
            WriteFrame(writer, width, height, indexedData);
    }

    /// <summary>
    /// The palette is written in color order; AlphaTest documents pad the file
    /// palette up to 256 entries so the transparency color lands on index 255.
    /// </summary>
    private static void WritePalette(BinaryWriter writer, List<Rgba32> orderedColors, bool isAlphaTest)
    {
        writer.Write((short)orderedColors.Count);
        foreach (Rgba32 color in orderedColors)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
        }
        if (orderedColors.Count < 256 && isAlphaTest)
        {
            for (int i = orderedColors.Count; i < 255; i++)
            {
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);
            }
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)255);
        }
    }

    private static void WriteFrame(BinaryWriter writer, int width, int height, byte[] indexedData)
    {
        writer.Write(0x00000000); // Group
        writer.Write(0x00000000); // OriginX
        writer.Write(0x00000000); // OriginY
        writer.Write(width);
        writer.Write(height);
        writer.Write(indexedData);
    }
}

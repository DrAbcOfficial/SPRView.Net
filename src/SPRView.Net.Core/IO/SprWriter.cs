namespace SPRView.Net.Core;

/// <summary>
/// Writes the GoldSrc sprite binary layout for composed indexed frames.
/// </summary>
internal static class SprWriter
{
    public static void Write(
        Stream output,
        SprHeader header,
        SprPalette palette,
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

        WritePalette(writer, palette);

        foreach (var (width, height, indexedData) in frames)
            WriteFrame(writer, width, height, indexedData);
    }

    /// <summary>
    /// Writes every palette entry.
    ///
    /// The entry count always matches the bytes that follow: declaring fewer
    /// entries than were written would leave a reader parsing frame data from
    /// inside the padding. The palette handed in is already laid out the way the
    /// format expects, including the transparency key on index 255.
    /// </summary>
    private static void WritePalette(BinaryWriter writer, SprPalette palette)
    {
        writer.Write((short)palette.Length);
        for (int i = 0; i < palette.Length; i++)
        {
            var color = palette[i];
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
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

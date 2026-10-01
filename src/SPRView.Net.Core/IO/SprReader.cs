using SixLabors.ImageSharp.PixelFormats;

namespace SPRView.Net.Core;

/// <summary>
/// Parses the GoldSrc sprite binary format into a <see cref="SprDocument"/>.
/// </summary>
internal static class SprReader
{
    public static SprDocument Read(Stream stream)
    {
        using BinaryReader reader = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);

        SprHeader header = ReadHeader(reader);
        SprPalette palette = ReadPalette(reader);
        List<SprFrame> frames = ReadFrames(reader, header, palette);
        return new SprDocument(header, palette, frames);
    }

    private static SprHeader ReadHeader(BinaryReader reader)
    {
        int signature = reader.ReadInt32();
        if (signature != SprHeader.Signature)
            throw new InvalidDataException("File not a valid goldsrc sprite");
        int version = reader.ReadInt32();
        if (version != SprHeader.SupportedVersion)
            throw new InvalidDataException("File not a supported goldsrc sprite version");

        SprHeader header = new()
        {
            Type = (SpriteType)reader.ReadInt32(),
            Format = (SpriteFormat)reader.ReadInt32(),
            BoundRadius = reader.ReadSingle(),
            MaxFrameWidth = (uint)reader.ReadInt32(),
            MaxFrameHeight = (uint)reader.ReadInt32(),
            NumberOfFrames = (uint)reader.ReadInt32(),
            BeamLength = reader.ReadSingle(),
            Synchronization = (SpriteSynchron)reader.ReadInt32()
        };
        return header;
    }

    private static SprPalette ReadPalette(BinaryReader reader)
    {
        short paletteSize = reader.ReadInt16();
        if (paletteSize <= 0)
            throw new InvalidDataException("Sprite palette is empty");
        SprPalette palette = new(paletteSize);
        for (int i = 0; i < paletteSize; i++)
            palette[i] = new Rgba32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), 255);
        return palette;
    }

    /// <summary>
    /// Reads the frame table.
    ///
    /// A frame that cannot exist - a non positive size, or a pixel block running
    /// past the end of the stream - ends the table instead of failing the whole
    /// document. Sprite files in the wild do carry malformed trailing entries:
    /// misc/wei.spr from Sven Co-op declares 22 frames but its last one is 0x0,
    /// and the 21 frames before it are perfectly viewable. Refusing the file
    /// would hide everything that is intact.
    /// </summary>
    private static List<SprFrame> ReadFrames(BinaryReader reader, SprHeader header, SprPalette palette)
    {
        List<SprFrame> frames = [];
        long remaining = reader.BaseStream.Length - reader.BaseStream.Position;

        for (int i = 0; i < header.NumberOfFrames; i++)
        {
            // The frame descriptor is five ints; anything shorter is a cut tail.
            if (remaining < 20)
                break;

            int group = reader.ReadInt32();
            int originX = reader.ReadInt32();
            int originY = reader.ReadInt32();
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            remaining -= 20;

            if (width <= 0 || height <= 0)
                break;

            // Compared as a long: a corrupt size can overflow an int product.
            long pixelCount = (long)width * height;
            if (pixelCount > remaining)
                break;

            frames.Add(new SprFrame(reader.ReadBytes((int)pixelCount), palette, header.Format,
                width, height, originX, originY, group));
            remaining -= pixelCount;
        }

        if (frames.Count == 0)
            throw new InvalidDataException("Sprite contains no readable frames");

        // Keep the header honest: the CLI and the C ABI expose this count, and a
        // declared-but-unreadable tail would make consumers index past the frames.
        header.NumberOfFrames = (uint)frames.Count;
        return frames;
    }
}

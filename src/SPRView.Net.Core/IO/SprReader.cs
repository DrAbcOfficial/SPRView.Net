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

    private static List<SprFrame> ReadFrames(BinaryReader reader, SprHeader header, SprPalette palette)
    {
        List<SprFrame> frames = [];
        for (int i = 0; i < header.NumberOfFrames; i++)
        {
            int group = reader.ReadInt32();
            int originX = reader.ReadInt32();
            int originY = reader.ReadInt32();
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            if (width <= 0 || height <= 0)
                throw new InvalidDataException($"Frame {i} has invalid dimensions {width}x{height}");
            byte[] data = reader.ReadBytes(width * height);
            if (data.Length < width * height)
                throw new InvalidDataException($"Frame {i} data is truncated");
            frames.Add(new SprFrame(data, palette, width, height, originX, originY, group));
        }
        return frames;
    }
}

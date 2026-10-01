using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SPRView.Net.Core;

/// <summary>
/// A parsed GoldSrc sprite document: header, palette and indexed frames.
/// </summary>
public sealed class SprDocument
{
    private readonly List<SprFrame> _frames;

    internal SprDocument(SprHeader header, SprPalette palette, List<SprFrame> frames)
    {
        Header = header;
        Palette = palette;
        _frames = frames;
    }

    public SprHeader Header { get; }
    public SprPalette Palette { get; }
    public IReadOnlyList<SprFrame> Frames => _frames;

    /// <summary>Parse a sprite from an open stream.</summary>
    public static SprDocument Load(Stream stream) => SprReader.Read(stream);

    /// <summary>Parse a sprite from a file path.</summary>
    public static SprDocument Load(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        return Load(stream);
    }

    public int GetFrameCount() => _frames.Count;

    public SprFrame GetFrame(int index) => _frames[index];

    /// <summary>Decode the frame at <paramref name="index"/> into a full color image.</summary>
    public Image<Rgba32> GetFrameImage(int index)
    {
        if (index < 0 || index >= _frames.Count)
            throw new IndexOutOfRangeException("Frame index out of bound");
        return _frames[index].Decode();
    }

    /// <summary>
    /// Compose ordinary image files into a new sprite document and write it.
    /// </summary>
    public static void Save(IEnumerable<string> imagePaths, Stream output, int width, int height,
        SpriteFormat format = SpriteFormat.Normal, SpriteType type = SpriteType.Parallel, SpriteSynchron sync = SpriteSynchron.Sync,
        float beamLength = 0, bool unpackAnimated = false)
    {
        List<string> paths = [.. imagePaths];
        if (paths.Count == 0)
            throw new ArgumentException("No input images", nameof(imagePaths));

        var (palette, frames) = SpriteComposer.Compose(paths, width, height, format, unpackAnimated);
        SprHeader header = new()
        {
            Type = type,
            Format = format,
            MaxFrameWidth = (uint)width,
            MaxFrameHeight = (uint)height,
            NumberOfFrames = (uint)frames.Count,
            BeamLength = beamLength,
            Synchronization = sync
        };
        SprWriter.Write(output, header, palette, frames);
    }
}

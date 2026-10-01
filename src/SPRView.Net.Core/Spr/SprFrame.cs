using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SPRView.Net.Core;

/// <summary>
/// One indexed color frame of a sprite document.
/// </summary>
public sealed class SprFrame
{
    private readonly SprPalette _palette;
    private readonly SpriteFormat _format;

    internal SprFrame(byte[] indexedData, SprPalette palette, SpriteFormat format,
        int width, int height, int originX, int originY, int group)
    {
        IndexedData = indexedData;
        _palette = palette;
        _format = format;
        Width = width;
        Height = height;
        OriginX = originX;
        OriginY = originY;
        Group = group;
    }

    /// <summary>Palette indexes, one byte per pixel, row major.</summary>
    public byte[] IndexedData { get; }

    public int Width { get; }
    public int Height { get; }
    public int OriginX { get; set; }
    public int OriginY { get; set; }
    public int Group { get; set; }

    /// <summary>
    /// Decode this frame into a full color image using the document palette and
    /// the transparency rule of the sprite format.
    /// </summary>
    /// <param name="applyTransparency">
    /// When false the frame is returned exactly as stored, background included,
    /// instead of honouring the sprite format's transparency rule.
    /// </param>
    public Image<Rgba32> Decode(bool applyTransparency = true)
    {
        return FrameDecoder.Decode(IndexedData, _palette, Width, Height, _format, applyTransparency);
    }

    /// <summary>
    /// Decode this frame into a tightly packed RGBA8888 buffer.
    /// </summary>
    /// <param name="applyTransparency">
    /// When false the frame is returned exactly as stored, background included,
    /// instead of honouring the sprite format's transparency rule.
    /// </param>
    public byte[] DecodeToRgba(bool applyTransparency = true)
    {
        return FrameDecoder.DecodeToRgba(IndexedData, _palette, Width, Height, _format, applyTransparency);
    }
}

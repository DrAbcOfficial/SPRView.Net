using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SPRView.Net.Core;

/// <summary>
/// Expands palette indexed frame data into full color pixel buffers, applying
/// the transparency rule of the sprite's blend format.
/// </summary>
internal static class FrameDecoder
{
    /// <summary>
    /// Decode indexed data into an RGBA image.
    /// </summary>
    /// <param name="applyTransparency">
    /// When false the frame is returned exactly as the file stores it: every
    /// pixel opaque, showing the background an author painted under the
    /// artwork. Useful for inspecting what is really in the file.
    /// </param>
    public static Image<Rgba32> Decode(byte[] indexedData, SprPalette palette, int width, int height,
        SpriteFormat format, bool applyTransparency = true)
    {
        ValidateLength(indexedData, width, height);

        var key = new TransparencyKey(palette, format, applyTransparency);

        Image<Rgba32> image = new(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                image[x, y] = key.Resolve(indexedData[(y * width) + x], palette);
        }
        return image;
    }

    /// <summary>
    /// Decode indexed data into a tightly packed RGBA8888 buffer.
    /// </summary>
    public static byte[] DecodeToRgba(byte[] indexedData, SprPalette palette, int width, int height,
        SpriteFormat format, bool applyTransparency = true)
    {
        ValidateLength(indexedData, width, height);

        var key = new TransparencyKey(palette, format, applyTransparency);

        byte[] rgba = new byte[width * height * 4];
        for (int i = 0; i < width * height; i++)
        {
            Rgba32 color = key.Resolve(indexedData[i], palette);
            int o = i * 4;
            rgba[o] = color.R;
            rgba[o + 1] = color.G;
            rgba[o + 2] = color.B;
            rgba[o + 3] = color.A;
        }
        return rgba;
    }

    /// <summary>
    /// How a sprite decides that a pixel is empty.
    ///
    /// IndexAlpha needs no key at all: its index already is the alpha value and
    /// the palette is the matching grey ramp.
    ///
    /// The other formats reserve palette entry 255 as the key colour, and the
    /// rule is a colour comparison, not an index comparison. That distinction
    /// matters: explode1.spr paints its empty area with the key colour (black)
    /// under index 0, and testing the index instead would leave it opaque - a
    /// solid rectangle. Across the stock Half-Life and Sven Co-op sprites, 593
    /// files have a key that is also reached through other indices, and none the
    /// other way round.
    ///
    /// Additive additionally treats black as empty. That is not a guess about
    /// intent: additive blending adds the colour to whatever is behind it, so
    /// adding black provably changes nothing and the pixel cannot be seen. Most
    /// Additive sprites rely on exactly that and park their empty area on black,
    /// which is why logo.spr and 320_logo.spr would otherwise come out as solid
    /// black blocks.
    ///
    /// The key is only honoured for a full 256 entry palette; a short palette has
    /// no reserved slot and its last entry is real content. hpbar.spr is such a
    /// file, with just two entries.
    /// </summary>
    private readonly struct TransparencyKey
    {
        private readonly bool _enabled;
        private readonly bool _alphaByIndex;
        private readonly bool _blackIsEmpty;
        private readonly bool _hasKey;
        private readonly Rgba32 _key;

        public TransparencyKey(SprPalette palette, SpriteFormat format, bool enabled)
        {
            _enabled = enabled;
            _alphaByIndex = format == SpriteFormat.IndexAlpha;
            _blackIsEmpty = format == SpriteFormat.Additive;
            _hasKey = !_alphaByIndex && palette.Length == SpritePaletteSize;
            _key = _hasKey ? palette[SpritePaletteSize - 1] : default;
        }

        /// <summary>
        /// Maps one palette index to a pixel. A disabled key returns the palette
        /// entry untouched, so the caller sees the stored image rather than the
        /// artwork with its background removed.
        /// </summary>
        public Rgba32 Resolve(int index, SprPalette palette)
        {
            Rgba32 color = palette[index];
            if (!_enabled)
                return color;

            if (_alphaByIndex)
                return WithAlpha(color, (byte)index);
            if (_blackIsEmpty && color.R == 0 && color.G == 0 && color.B == 0)
                return default;
            if (_hasKey && color.R == _key.R && color.G == _key.G && color.B == _key.B)
                return default;
            return color;
        }
    }

    /// <summary>Entry count of a full palette; shorter ones have no key slot.</summary>
    private const int SpritePaletteSize = 256;

    private static Rgba32 WithAlpha(Rgba32 color, byte alpha)
        => new(color.R, color.G, color.B, alpha);

    private static void ValidateLength(byte[] indexedData, int width, int height)
    {
        if (indexedData.Length < width * height)
            throw new InvalidDataException("Frame data is shorter than its declared dimensions");
    }
}

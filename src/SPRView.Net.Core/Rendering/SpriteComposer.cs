using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace SPRView.Net.Core;

/// <summary>
/// Composes ordinary image files into a quantized palette plus indexed frames,
/// the representation the sprite binary format stores.
/// </summary>
internal static class SpriteComposer
{
    /// <summary>
    /// Load every image file, resize to the target frame size, split animated
    /// images into single frames and turn them into a palette plus indexed
    /// frames in the layout the requested format expects.
    /// </summary>
    public static (SprPalette Palette, List<(int Width, int Height, byte[] IndexedData)> Frames)
        Compose(IReadOnlyList<string> imagePaths, int width, int height, SpriteFormat format, bool unpackAnimated)
    {
        using Image<Rgba32> strip = ComposeImageStrip(imagePaths, width, height);

        return format == SpriteFormat.IndexAlpha
            ? ComposeIndexAlpha(strip, width, height)
            : ComposeKeyed(strip, width, height, format);
    }

    /// <summary>
    /// Normal, Additive and AlphaTest: a shared palette of real colours with the
    /// transparency key parked on index 255.
    /// </summary>
    private static (SprPalette, List<(int, int, byte[])>) ComposeKeyed(
        Image<Rgba32> strip, int width, int height, SpriteFormat format)
    {
        WuQuantizer quantizer = new(new QuantizerOptions
        {
            Dither = null,
            // One slot stays free for the transparency key.
            MaxColors = KeyIndex
        });
        strip.Mutate(x => x.Quantize(quantizer));

        // Fully transparent pixels carry no colour worth keeping, so they are
        // left out of the palette and mapped to the key when indexing.
        List<Rgba32> colors = [];
        Dictionary<Rgba32, byte> seen = [];
        for (int y = 0; y < strip.Height; y++)
        {
            for (int x = 0; x < strip.Width; x++)
            {
                Rgba32 color = strip[x, y];
                if (color.A <= 128 || seen.ContainsKey(color))
                    continue;
                seen.Add(color, (byte)colors.Count);
                colors.Add(color);
            }
        }

        SprPalette palette = new(256);
        for (int i = 0; i < colors.Count && i < KeyIndex; i++)
            palette[i] = colors[i];
        palette[KeyIndex] = KeyColor;

        Dictionary<Rgba32, byte> lookup = [];
        for (int i = 0; i < palette.Length; i++)
            lookup[palette[i]] = (byte)i;

        // Additive blending makes black a no-op, so black pixels are written as
        // the key too; otherwise a viewer would show them as an opaque patch.
        bool blackIsEmpty = format == SpriteFormat.Additive;

        List<(int, int, byte[])> frames = [];
        for (int frame = 0; frame < strip.Height / height; frame++)
        {
            byte[] indexed = new byte[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Rgba32 color = strip[x, frame * height + y];
                    bool empty = color.A <= 128
                        || (blackIsEmpty && color.R == 0 && color.G == 0 && color.B == 0);
                    indexed[(y * width) + x] = empty
                        ? KeyIndex
                        : lookup.TryGetValue(color, out byte found) ? found : KeyIndex;
                }
            }
            frames.Add((width, height, indexed));
        }
        return (palette, frames);
    }

    /// <summary>
    /// IndexAlpha: the index is the alpha, so a sprite is only representable when
    /// its colour is a function of its alpha. The ramp is therefore built from the
    /// source itself - palette[i] is the average colour of the pixels at alpha i,
    /// and each pixel is indexed by its own alpha. A source that already varies
    /// colour and alpha together (steam, smoke, glows) round-trips exactly; one
    /// that does not is flattened towards that relationship.
    /// </summary>
    private static (SprPalette, List<(int, int, byte[])>) ComposeIndexAlpha(
        Image<Rgba32> strip, int width, int height)
    {
        // Accumulate the colour behind each alpha level.
        var sum = new double[256, 3];
        var hits = new int[256];
        for (int y = 0; y < strip.Height; y++)
        {
            for (int x = 0; x < strip.Width; x++)
            {
                Rgba32 c = strip[x, y];
                sum[c.A, 0] += c.R;
                sum[c.A, 1] += c.G;
                sum[c.A, 2] += c.B;
                hits[c.A]++;
            }
        }

        // Alphas the source never used borrow the nearest level that it did, so
        // the ramp stays continuous instead of dropping to black.
        var ramp = new Rgba32[256];
        int lastFilled = -1;
        for (int a = 0; a < 256; a++)
        {
            if (hits[a] > 0)
            {
                ramp[a] = new Rgba32(
                    (byte)Math.Round(sum[a, 0] / hits[a]),
                    (byte)Math.Round(sum[a, 1] / hits[a]),
                    (byte)Math.Round(sum[a, 2] / hits[a]),
                    (byte)a);
                lastFilled = a;
            }
            else if (lastFilled >= 0)
                ramp[a] = ramp[lastFilled];
            else
                ramp[a] = new Rgba32(0, 0, 0, (byte)a);
        }

        SprPalette palette = new(256);
        for (int a = 0; a < 256; a++)
            palette[a] = ramp[a];

        List<(int, int, byte[])> frames = [];
        for (int frame = 0; frame < strip.Height / height; frame++)
        {
            byte[] indexed = new byte[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    indexed[(y * width) + x] = strip[x, frame * height + y].A;
            }
            frames.Add((width, height, indexed));
        }
        return (palette, frames);
    }

    private static Image<Rgba32> ComposeImageStrip(IReadOnlyList<string> imagePaths, int width, int height)
    {
        int frameCount = 0;
        foreach (string path in imagePaths)
        {
            using Image probe = Image.Load(path);
            frameCount += probe.Frames.Count;
        }

        Image<Rgba32> strip = new(width, height * frameCount);
        try
        {
            int seek = 0;
            foreach (string path in imagePaths)
            {
                using Image source = Image.Load(path);
                source.Mutate(x => x.Resize(width, height));
                do
                {
                    strip.Mutate(x => x.DrawImage(source, new Point(0, height * seek), 1.0f));
                    if (source.Frames.Count > 1)
                        source.Frames.RemoveFrame(0);
                    seek++;
                } while (source.Frames.Count > 1);
            }
            return strip;
        }
        catch
        {
            strip.Dispose();
            throw;
        }
    }

    /// <summary>The palette entry reserved as the transparency key.</summary>
    private const byte KeyIndex = 255;

    /// <summary>
    /// The colour parked at the key index. The decoder treats every pixel of this
    /// colour as empty, so it must not collide with the artwork: pure blue is
    /// already the convention the stock sprites use. Sprites composed here are
    /// quantized to 255 real colours plus this key.
    /// </summary>
    private static readonly Rgba32 KeyColor = new(0, 0, 255, 255);
}

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
    /// images into single frames, quantize everything against one shared
    /// palette and index every frame against it.
    /// </summary>
    /// <param name="orderedColors">
    /// The distinct colors in file order. For <see cref="SpriteFormat.AlphaTest"/>
    /// the transparency color occupies index 255; the writer pads the file
    /// palette so both stay in sync.
    /// </param>
    public static (SprPalette Palette, List<Rgba32> OrderedColors, List<(int Width, int Height, byte[] IndexedData)> Frames)
        Compose(IReadOnlyList<string> imagePaths, int width, int height, SpriteFormat format, bool unpackAnimated)
    {
        bool isAlphaTest = format == SpriteFormat.AlphaTest;

        using Image<Rgba32> strip = ComposeImageStrip(imagePaths, width, height);
        Quantize(strip, isAlphaTest);

        List<Rgba32> orderedColors = CollectColors(strip, isAlphaTest);
        SprPalette palette = BuildPalette(orderedColors, isAlphaTest);
        List<(int, int, byte[])> frames = IndexFrames(strip, width, height, orderedColors.Count, isAlphaTest, palette);
        return (palette, orderedColors, frames);
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

    private static void Quantize(Image<Rgba32> strip, bool isAlphaTest)
    {
        WuQuantizer quantizer = new(new QuantizerOptions
        {
            Dither = null,
            MaxColors = isAlphaTest ? 255 : 256
        });
        strip.Mutate(x => x.Quantize(quantizer));
    }

    private static List<Rgba32> CollectColors(Image<Rgba32> strip, bool isAlphaTest)
    {
        Dictionary<Rgba32, byte> colors = [];
        for (int y = 0; y < strip.Height; y++)
        {
            for (int x = 0; x < strip.Width; x++)
            {
                Rgba32 color = strip[x, y];
                if (!colors.ContainsKey(color))
                    colors.Add(color, (byte)colors.Count);
            }
        }
        if (isAlphaTest)
            colors.Add(new Rgba32(0, 0, 255, 255), 255);
        return [.. colors.Keys];
    }

    /// <summary>
    /// Full 256 entry lookup palette; unused slots stay transparent black and
    /// the AlphaTest transparency color sits at index 255.
    /// </summary>
    private static SprPalette BuildPalette(List<Rgba32> orderedColors, bool isAlphaTest)
    {
        SprPalette palette = new(256);
        for (int i = 0; i < orderedColors.Count; i++)
        {
            if (!isAlphaTest || i != orderedColors.Count - 1)
                palette[i] = orderedColors[i];
        }
        if (isAlphaTest)
            palette[255] = new Rgba32(0, 0, 255, 255);
        return palette;
    }

    private static List<(int, int, byte[])> IndexFrames(
        Image<Rgba32> strip, int width, int height, int colorCount, bool isAlphaTest, SprPalette palette)
    {
        byte transparentIndex = isAlphaTest ? (byte)255 : byte.MinValue;
        Dictionary<Rgba32, byte> lookup = [];
        for (int i = 0; i < colorCount; i++)
            lookup[palette[i]] = (byte)i;

        List<(int, int, byte[])> frames = [];
        int frameCount = strip.Height / height;
        for (int frame = 0; frame < frameCount; frame++)
        {
            byte[] indexed = new byte[width * height];
            int startY = frame * height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Rgba32 color = strip[x, startY + y];
                    indexed[(y * width) + x] =
                        isAlphaTest && color.A <= 128 ? transparentIndex : lookup[color];
                }
            }
            frames.Add((width, height, indexed));
        }
        return frames;
    }
}

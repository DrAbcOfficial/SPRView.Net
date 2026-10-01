using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SPRView.Net.Core;

namespace SPRView.Net.Services;

/// <summary>
/// Renders a sprite document into animated image containers.
/// </summary>
public static class AnimationExporter
{
    /// <summary>
    /// Frame duration used when the caller does not supply one, in milliseconds.
    /// A sprite carries no playback rate of its own - the engine drives that
    /// from the entity - so the exported file has to pick a sensible default.
    /// </summary>
    public const int DefaultFrameDelayMilliseconds = 100;

    /// <summary>
    /// Write every frame of the sprite into an animated .gif or .webp stream.
    /// </summary>
    /// <param name="frameDelayMilliseconds">
    /// How long each frame is shown. Both containers store per-frame timing and
    /// neither has a usable default: leaving it unset wrote 0, which players
    /// interpret inconsistently and in practice meant an animation that did not
    /// play at the intended speed.
    /// </param>
    public static void SaveAnimated(SprDocument sprite, Stream stream, string extension,
        int frameDelayMilliseconds = DefaultFrameDelayMilliseconds)
    {
        int delay = Math.Max(1, frameDelayMilliseconds);
        bool webp = Path.GetExtension(extension)?.ToLowerInvariant() == ".webp";

        using Image<Rgba32> animation = new(
            (int)sprite.Header.MaxFrameWidth, (int)sprite.Header.MaxFrameHeight);

        // The Image constructor seeds one frame and AddFrame appends, so the
        // first sprite frame is painted into that seed and the rest are
        // appended. Appending past the seed instead left the export one frame
        // long and starting on a blank frame.
        for (int i = 0; i < sprite.Frames.Count; i++)
        {
            using Image<Rgba32> img = sprite.Frames[i].Decode();
            if (i == 0)
            {
                var source = img.Frames.RootFrame;
                for (int y = 0; y < animation.Height; y++)
                    for (int x = 0; x < animation.Width; x++)
                        animation[x, y] = source[x, y];
            }
            else
            {
                animation.Frames.AddFrame(img.Frames.RootFrame);
            }
        }

        // Loop forever. 0 means forever in both containers, and it is a property
        // of the animation rather than of a frame.
        if (webp)
            animation.Metadata.GetWebpMetadata().RepeatCount = 0;
        else
            animation.Metadata.GetGifMetadata().RepeatCount = 0;

        // Timing is per frame, and the two formats count in different units: GIF
        // in hundredths of a second, WebP in milliseconds. Using the wrong one
        // produces an animation at the wrong speed rather than an error, so the
        // conversion is explicit.
        for (int i = 0; i < animation.Frames.Count; i++)
        {
            if (webp)
                animation.Frames[i].Metadata.GetWebpMetadata().FrameDelay = (uint)delay;
            else
                animation.Frames[i].Metadata.GetGifMetadata().FrameDelay = ToCentiseconds(delay);
        }

        if (webp)
            animation.Save(stream, new WebpEncoder());
        else
            animation.Save(stream, new GifEncoder());
    }

    /// <summary>
    /// Milliseconds to GIF hundredths of a second, rounded to nearest and never
    /// zero: a zero delay is exactly what players disagree about.
    /// </summary>
    private static int ToCentiseconds(int milliseconds)
        => Math.Max(1, (int)Math.Round(milliseconds / 10.0));
}

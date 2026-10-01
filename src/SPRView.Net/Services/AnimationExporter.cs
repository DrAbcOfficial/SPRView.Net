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
    /// Write every frame of the sprite into an animated .gif or .webp stream.
    /// </summary>
    public static void SaveAnimated(SprDocument sprite, Stream stream, string extension)
    {
        using Image<Rgba32> animation = new((int)sprite.Header.MaxFrameWidth, (int)sprite.Header.MaxFrameHeight);
        foreach (SprFrame frame in sprite.Frames)
        {
            using Image<Rgba32> img = frame.Decode();
            animation.Frames.AddFrame(img.Frames[0]);
        }

        if (Path.GetExtension(extension)?.ToLower() == ".webp")
        {
            WebpMetadata metadata = animation.Metadata.GetWebpMetadata();
            metadata.RepeatCount = 0;
            animation.Save(stream, new WebpEncoder());
        }
        else
        {
            GifMetadata gifMetadata = animation.Metadata.GetGifMetadata();
            gifMetadata.RepeatCount = 0;
            animation.Save(stream, new GifEncoder());
        }
    }
}

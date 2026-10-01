using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SPRView.Net.Core;

namespace SPRView.Net.Thumbnailer;

/// <summary>
/// Renders a .spr thumbnail into a PNG file.
/// </summary>
internal static class Program
{
    private const int ExitSuccess = 0;
    private const int ExitUsageError = 2;
    private const int ExitRenderError = 3;

    private static int Main(string[] args)
    {
        if (!ThumbnailerOptions.TryParse(args, out ThumbnailerOptions options))
        {
            Console.Error.WriteLine("Usage: sprview-thumbnailer [-s maxsize] <input.spr> <output.png>");
            return ExitUsageError;
        }

        try
        {
            Render(options);
            return ExitSuccess;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"sprview-thumbnailer: {e.Message}");
            return ExitRenderError;
        }
    }

    private static void Render(ThumbnailerOptions options)
    {
        SprDocument document = SprDocument.Load(options.InputPath);
        SprFrame source = document.Frames[0];
        using Image image = source.Decode();

        float scale = Math.Min(
            (float)options.MaxSize / source.Width,
            (float)options.MaxSize / source.Height);
        int width = Math.Max(1, (int)Math.Round(source.Width * scale));
        int height = Math.Max(1, (int)Math.Round(source.Height * scale));
        image.Mutate(context => context.Resize(width, height, KnownResamplers.NearestNeighbor));
        image.SaveAsPng(options.OutputPath);
    }
}

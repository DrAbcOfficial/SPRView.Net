using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Pastel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SPRView.Net.Lib.Class;
namespace SPRView.Net.CLI.Cli;
[Command("preview", Description = "Preview a spr in console")]
public class PreviewCommand : ICommand
{
    [CommandParameter(0, Description = "Input path")]
    public required string Spr { get; set; }

    [CommandOption("size", 's', Description = "Preview pixel size")]
    public int PreviewSize { get; set; } = 32;

    [CommandOption("frame", 'f', Description = "Preview frame")]
    public int Frame { get; set; } = 0;

    public ValueTask ExecuteAsync(IConsole console)
    {
        if (Spr != null)
        {
            using FileStream fs = File.OpenRead(Spr);
            CSprite spr = new(fs);
            Image<Rgba32> image = (Image<Rgba32>)spr.GetImage(Math.Clamp(Frame, 0, spr.Frames.Count - 1));
            int width = image.Width;
            int height = image.Height;
            float ratio = 1.0f;
            if (width > height)
            {
                ratio = (float)PreviewSize / width;
                width = PreviewSize;
                height = (int)(height * ratio);
            }
            else
            {
                ratio = (float)PreviewSize / height;
                height = PreviewSize;
                width = (int)(width * ratio);
            }
            image.Mutate(x => x.Resize(width, height));
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Rgba32 rgba = image[j, i];
                    console.Output.Write("██".Pastel(System.Drawing.Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B)));
                }
                console.Output.Write('\n');
            }
        }
        return default;
    }
}

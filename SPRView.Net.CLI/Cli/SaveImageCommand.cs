using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using SixLabors.ImageSharp;
using SPRView.Net.Lib.Class;
namespace SPRView.Net.CLI.Cli;
[Command("image", Description = "Save spr to image")]
public class SaveImageCommand : ICommand
{
    [CommandParameter(0, Description = "Input path")]
    public required string Spr { get; set; }
    [CommandParameter(1, Description = "Output path")]
    public required string SavePath { get; set; }

    [CommandOption("format", 'f', Description = "Output Image format\n\tsupport .bmp .png .jpg .jpeg .tga .gif .qoi .webp .tiff .pbm")]
    public string Format { get; set; } = "bmp";

    public ValueTask ExecuteAsync(IConsole console)
    {
        if (Spr != null)
        {
            using FileStream fs = File.OpenRead(Spr);
            CSprite spr = new(fs);
            Image image = spr.GetImage(0);
            switch (Format)
            {
                case "bmp": image.SaveAsBmp(SavePath); break;
                case "png": image.SaveAsPng(SavePath); break;
                case "jpeg":
                case "jpg": image.SaveAsJpeg(SavePath); break;
                case "tga": image.SaveAsTga(SavePath); break;
                case "qoi": image.SaveAsQoi(SavePath); break;
                case "tiff": image.SaveAsTga(SavePath); break;
                case "webp":
                    {
                        for (int i = 1; i < spr.GetFrames(); i++)
                        {
                            image.Frames.AddFrame(spr.GetImage(i).Frames[0]);
                        }
                        image.SaveAsWebp(SavePath);
                        break;
                    }
                case "pbm": image.SaveAsPbm(SavePath); break;
                case "gif":
                    {
                        for (int i = 1; i < spr.GetFrames(); i++)
                        {
                            image.Frames.AddFrame(spr.GetImage(i).Frames[0]);
                        }
                        image.SaveAsGif(SavePath);
                        break;
                    }
                default:
                    {
                        console.Error.WriteLine("Not valid format!");
                        break;
                    }
            }
        }
        return default;
    }
}

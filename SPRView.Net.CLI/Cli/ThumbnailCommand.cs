using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SPRView.Net.Lib;
namespace SPRView.Net.Cli;
[Command("thumbnail", Description = "Generate thumbnail form a spr")]
public class ThumbnailCommand : ICommand
{
    [CommandParameter(0, Description = "Input spr path")]
    public required string Spr { get; set; }
    [CommandOption("output", 'o', Description = "Output path, if set, it will save into file instead of base64")]
    public string? SavePath { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        if (Spr != null)
        {
            using FileStream fs = File.OpenRead(Spr);
            CSprite spr = new(fs);
            Image bmp = spr.GetImage(0);
            bmp.Mutate(x => x.Resize(64, 64));
            if (SavePath != null)
                bmp.SaveAsBmp(SavePath);
            else
            {
                using MemoryStream nms = new();
                bmp.SaveAsBmp(nms);
                string base64 = Convert.ToBase64String(nms.ToArray());
                console.Output.Write(base64);
            }
        }
        return default;
    }
}

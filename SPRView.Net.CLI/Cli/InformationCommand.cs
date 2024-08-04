using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Pastel;
using SixLabors.ImageSharp.PixelFormats;
using SPRView.Net.Lib;
using System.Drawing;
namespace SPRView.Net.Cli;
[Command("information", Description = "Get spr information")]
public class InformationCommand : ICommand
{
    [CommandParameter(0, Description = "Input path")]
    public required string Spr { get; set; }
    private string Pad(object src, int len = 12)
    {
        string? temp = src.ToString();
        if(temp == null)
            return string.Empty;
        if (len <= temp.Length)
            return temp;
        while(temp.Length < len)
        {
            temp += " ";
        }
        return temp;
    }
    public ValueTask ExecuteAsync(IConsole console)
    {
        if (Spr != null)
        {
            using FileStream fs = File.OpenRead(Spr);
            CSprite spr = new(fs);
            string[] output = [
                $"\t{Pad("Frames:")}\t{Pad(spr.NumberOfFrames)}\t\t{Pad("Sync:")}\t{Pad(spr.Synchronization)}\n",
                $"\t{Pad("Width:")}\t{Pad(spr.MaxFrameWidth)}\t\t{Pad("Height:")}\t{Pad(spr.MaxFrameHeight)}\n",
                $"\t{Pad("BoundRadius:")}\t{Pad(spr.BoundRadius)}\t\t{Pad("BeamLength:")}\t{Pad(spr.BeamLength)}\n",
                $"\t{Pad("Type:")}\t{Pad(spr.Type)}\t\t{Pad("Format:")}\t{Pad(spr.Format)}\n"
            ];
            foreach (string line in output)
            {
                console.Output.Write(line);
            }
            console.Output.Write("\tPalette:\n");
            for(int i = 0; i < spr.Pallete.Length; i++)
            {
                if(i % 16 == 0)
                    console.Output.Write("\n\t\t");
                Rgba32 rgba = spr.Pallete.AtIndex(i);
                console.Output.Write("■ ".Pastel(Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B)));
            }
        }
        return default;
    }
}

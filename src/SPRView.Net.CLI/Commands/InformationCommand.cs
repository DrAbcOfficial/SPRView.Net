using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Pastel;
using SixLabors.ImageSharp.PixelFormats;
using SPRView.Net.Core;
using Color = System.Drawing.Color;

namespace SPRView.Net.CLI.Commands;

[Command("information", Description = "Get spr information")]
public class InformationCommand : ICommand
{
    [CommandParameter(0, Description = "Input path")]
    public required string Spr { get; set; }

    private static string Pad(object src, int len = 12)
    {
        string? temp = src.ToString();
        if (temp == null)
            return string.Empty;
        if (len <= temp.Length)
            return temp;
        while (temp.Length < len)
        {
            temp += " ";
        }
        return temp;
    }

    public ValueTask ExecuteAsync(IConsole console)
    {
        using FileStream fs = File.OpenRead(Spr);
        SprDocument spr = SprDocument.Load(fs);
        string[] output = [
            $"\t{Pad("Frames:")}\t{Pad(spr.Header.NumberOfFrames)}\t\t{Pad("Sync:")}\t{Pad(spr.Header.Synchronization)}\n",
            $"\t{Pad("Width:")}\t{Pad(spr.Header.MaxFrameWidth)}\t\t{Pad("Height:")}\t{Pad(spr.Header.MaxFrameHeight)}\n",
            $"\t{Pad("BoundRadius:")}\t{Pad(spr.Header.BoundRadius)}\t\t{Pad("BeamLength:")}\t{Pad(spr.Header.BeamLength)}\n",
            $"\t{Pad("Type:")}\t{Pad(spr.Header.Type)}\t\t{Pad("Format:")}\t{Pad(spr.Header.Format)}\n"
        ];
        foreach (string line in output)
        {
            console.Output.Write(line);
        }
        console.Output.Write("\tPalette:\n");
        for (int i = 0; i < spr.Palette.Length; i++)
        {
            if (i % 16 == 0)
                console.Output.Write("\n\t\t");
            Rgba32 rgba = spr.Palette[i];
            console.Output.Write("■ ".Pastel(Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B)));
        }
        return default;
    }
}

using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using SPRView.Net.Core;

namespace SPRView.Net.CLI.Commands;

[Command("create", Description = "create a spr from files")]
public class CreateCommand : ICommand
{
    [CommandParameter(0, Description = "Images path, Use \",\" connect multiple paths")]
    public required string Paths { get; set; }

    [CommandParameter(1, Description = "Spr width")]
    public required int Width { get; set; }

    [CommandParameter(2, Description = "Spr Height")]
    public required int Height { get; set; }

    [CommandParameter(3, Description = "Path to save spr file")]
    public required string SavePath { get; set; }

    [CommandOption("type", 't', Description = "Spr type")]
    public int Type { get; set; } = (int)SpriteType.Parallel;

    [CommandOption("format", 'f', Description = "Spr format")]
    public int Format { get; set; } = (int)SpriteFormat.Normal;

    [CommandOption("sync", 's', Description = "Spr sync")]
    public bool Sync { get; set; } = true;

    [CommandOption("beam length", 'b', Description = "Spr beam lenght")]
    public float BeamLenght { get; set; } = 0.0f;

    [CommandOption("unpack", 'u', Description = "Unpack multiple frames of images")]
    public bool Unpack { get; set; } = false;

    public ValueTask ExecuteAsync(IConsole console)
    {
        using FileStream fs = File.OpenWrite(SavePath);
        string[] paths = Paths.Split(',').Select(s => s.Trim()).ToArray();
        SprDocument.Save(paths, fs, Width, Height, (SpriteFormat)Format, (SpriteType)Type,
            Sync ? SpriteSynchron.Sync : SpriteSynchron.Random, BeamLenght, Unpack);
        return default;
    }
}

using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using SPRView.Net.Lib.Class;
using SPRView.Net.Lib.Interface;

namespace SPRView.Net.CLI.Cli;

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
    public int Type { get; set; } = (int)ISprite.SpriteType.Parallel;

    [CommandOption("format", 'f', Description = "Spr format")]
    public int Format { get; set; } = (int)ISprite.SpriteFormat.Normal;

    [CommandOption("sync", 's', Description = "Spr sync")]
    public bool Sync { get; set; } = true;

    [CommandOption("beam length", 'b', Description = "Spr beam lenght")]
    public float BeamLenght { get; set; } = 0.0f;

    [CommandOption("unpack", 'u', Description = "Unpack multiple frames of images")]
    public bool Unpack { get; set; } = false;

    public ValueTask ExecuteAsync(IConsole console)
    {
        using FileStream fs = File.OpenWrite(SavePath);
        string[] paths = Array.ConvertAll(Paths.Split(','), s => s.Trim());
        CSprite.Save(paths, fs, Width, Height, (ISprite.SpriteFormat)Format, (ISprite.SpriteType)Type, (ISprite.SpriteSynchron)(Sync ? 0 : 1), BeamLenght, Unpack);
        return default;
    }
}

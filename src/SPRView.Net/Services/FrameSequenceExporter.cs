using SixLabors.ImageSharp;
using SPRView.Net.Core;

namespace SPRView.Net.Services;

/// <summary>
/// Exports every frame of a sprite as numbered images plus a sequence list.
/// </summary>
public static class FrameSequenceExporter
{
    /// <summary>
    /// Write every frame as <c>{index}.bmp</c> into <paramref name="directory"/>
    /// and generate a matching <c>sequence.qc</c> fragment.
    /// </summary>
    public static void ExportSequence(SprDocument sprite, string directory)
    {
        string sequence = "";
        for (int i = 0; i < sprite.Frames.Count; i++)
        {
            using Image img = sprite.GetFrameImage(i);
            using FileStream fs = File.Create(Path.Combine(directory, $"{i}.bmp"));
            img.SaveAsBmp(fs);
            sequence += $"./{i}.bmp\n";
        }
        using StreamWriter text = new(Path.Combine(directory, "sequence.qc"));
        text.Write(sequence);
    }
}

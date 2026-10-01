using SixLabors.ImageSharp.PixelFormats;
using SPRView.Net.Core;

namespace SPRView.Net.Services;

/// <summary>
/// Writes sprite palettes to the supported external palette formats.
/// </summary>
public static class PaletteFileWriter
{
    /// <summary>Write a RIFF (.pal) palette, the format used by Half-Life tools.</summary>
    public static void WriteRiffPal(Stream stream, SprPalette palette)
    {
        using BinaryWriter bw = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        //0x00 Magic
        bw.Write((uint)0X52494646);
        //0x04 FileSize
        bw.Write((uint)0x0);
        //0x08 RIFF
        bw.Write((uint)0X50414C20);
        //0x0B Block Header
        bw.Write((uint)0X64617461);
        //0x10 Block Size
        bw.Write((uint)0x0);
        //0x14 Palette Size
        bw.Write((ushort)palette.Length);
        //0x16 Palette Version
        bw.Write((ushort)0X300);
        //0x18 Data
        for (int i = 0; i < palette.Length; i++)
        {
            bw.Write(palette[i].R);
            bw.Write(palette[i].G);
            bw.Write(palette[i].B);
            bw.Write((byte)0x00);
        }
        long fileSize = bw.BaseStream.Length;
        bw.Seek(0x04, SeekOrigin.Begin);
        bw.Write((uint)fileSize - 8);
        bw.Seek(0x10, SeekOrigin.Begin);
        bw.Write((uint)fileSize - 20);
    }

    /// <summary>Write a GIMP (.gpl) text palette.</summary>
    public static void WriteGimpPal(Stream stream, SprPalette palette, string name)
    {
        using StreamWriter sw = new(stream, leaveOpen: true);
        sw.WriteLine("GIMP Palette");
        sw.WriteLine($"Name: {name}");
        sw.WriteLine("Columns: 16");
        for (int i = 0; i < palette.Length; i++)
        {
            Rgba32 rgba = palette[i];
            sw.WriteLine($"{rgba.R} {rgba.G} {rgba.B} Index {i}");
        }
    }
}

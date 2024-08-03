using SixLabors.ImageSharp.PixelFormats;
namespace SPRView.Net.Lib.Interface;
public interface ISpriteColorPalette
{
    abstract public static ISpriteColorPalette Create(int size);
    abstract public Rgba32 AtIndex(int index);
    abstract public int Length { get; }
    abstract public int Size { get; }
}

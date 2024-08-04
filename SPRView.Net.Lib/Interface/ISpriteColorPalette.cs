using SixLabors.ImageSharp.PixelFormats;
namespace SPRView.Net.Lib.Interface;
public interface ISpriteColorPalette
{
    abstract public static ISpriteColorPalette Create(int size);
    abstract public Rgba32 AtIndex(int index);
    /// <summary>
    /// Color array actually length
    /// </summary>
    abstract public int Length { get; }
    /// <summary>
    /// Initial size
    /// </summary>
    abstract public int Size { get; }
}

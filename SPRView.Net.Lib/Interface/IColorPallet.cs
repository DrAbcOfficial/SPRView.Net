using Avalonia.Controls;
using Avalonia.Media;
namespace SPRView.Net.Lib.Interface
{
    public interface ISpriteColorPalette : IColorPalette
    {
        abstract public static ISpriteColorPalette Create(int size);
        abstract public Color AtIndex(int index);
        abstract public int Length { get; }
    }
}

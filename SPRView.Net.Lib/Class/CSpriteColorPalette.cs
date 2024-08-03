using Avalonia.Media;
using SPRView.Net.Lib.Interface;
namespace SPRView.Net.Lib;

public class CSpriteColorPalette(int size) : ISpriteColorPalette
{
    public static ISpriteColorPalette Create(int size)
    {
        return new CSpriteColorPalette(size);
    }
    public Color AtIndex(int index)
    {
        return this[index];
    }

    private Color[] _color = new Color[size];
    private int shadeSize = size / 16;

    public int ColorCount { get => 16; }
    public int ShadeCount { get => shadeSize; }
    public int Length { get => _color.Length; }
    public Color GetColor(int colorIndex, int shadeIndex)
    {
        return _color[colorIndex + shadeIndex * 16];
    }
    public Color this[int index]
    {
        get
        {
            if (index < 0 || index >= _color.Length)
                throw new IndexOutOfRangeException("Color index out of range");
            return _color[index];
        }
        set
        {
            if (index < 0 || index >= _color.Length)
                throw new IndexOutOfRangeException("Color index out of range");
            _color[index] = value;
        }
    }
}
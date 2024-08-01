using Avalonia.Controls;
using Avalonia.Media;
using System;
namespace SPRView.Net.Lib;

public class ColorPallet : IColorPalette
{
    private Color[] _color;
    private int shadeSize;
    public ColorPallet(int size)
    {
        _color = new Color[size];
        shadeSize = size / 16;
    }
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
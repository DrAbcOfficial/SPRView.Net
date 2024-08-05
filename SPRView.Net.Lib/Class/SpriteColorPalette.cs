using SixLabors.ImageSharp.PixelFormats;
using SPRView.Net.Lib.Interface;
namespace SPRView.Net.Lib;

public class CSpriteColorPalette(int size) : ISpriteColorPalette
{
    public static ISpriteColorPalette Create(int size)
    {
        return new CSpriteColorPalette(size);
    }
    public Rgba32 AtIndex(int index)
    {
        return this[index];
    }
    private readonly Rgba32[] _color = new Rgba32[size];
    private readonly int _size = size;
    public int Size { get => _size; }
    public int Length { get => _color.Length; }
    public Rgba32 this[int index]
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
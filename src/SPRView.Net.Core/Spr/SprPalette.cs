using SixLabors.ImageSharp.PixelFormats;

namespace SPRView.Net.Core;

/// <summary>
/// The 24 bit color palette shared by every frame of a sprite document.
/// </summary>
public sealed class SprPalette(int size)
{
    private readonly Rgba32[] _colors = new Rgba32[size > 0 ? size : throw new ArgumentOutOfRangeException(nameof(size), "Palette size must be positive")];

    /// <summary>
    /// Number of colors this palette holds.
    /// </summary>
    public int Length => _colors.Length;

    public Rgba32 this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_colors.Length)
                throw new IndexOutOfRangeException("Color index out of range");
            return _colors[index];
        }
        set
        {
            if ((uint)index >= (uint)_colors.Length)
                throw new IndexOutOfRangeException("Color index out of range");
            _colors[index] = value;
        }
    }

    /// <summary>Read only view over the raw color array.</summary>
    internal ReadOnlySpan<Rgba32> Span => _colors;
}

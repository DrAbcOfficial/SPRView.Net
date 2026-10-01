using Avalonia.Media;
using SPRView.Net.Core;

namespace SPRView.Net.Storage;

/// <summary>
/// Per application instance state: the sprite being viewed and derived views.
/// </summary>
public class AppStorage
{
    /// <summary>
    /// Palette of the loaded sprite, flattened into a grid of swatches for the
    /// side panel. The GoldSrc palette is always 16 colours per shade row.
    /// </summary>
    public sealed class ColorPaletteView
    {
        private SprPalette? _original;
        private IReadOnlyList<Color> _colors = [];

        /// <summary>Colour count of the loaded palette, zero when nothing is loaded.</summary>
        public int Count => _colors.Count;

        /// <summary>Colours in row-major order, 16 per row.</summary>
        public IReadOnlyList<Color> Colors => _colors;

        public SprPalette? GetOrigin() => _original;

        public void SetOrigin(SprPalette? palette)
        {
            _original = palette;
            if (palette == null)
            {
                _colors = [];
                return;
            }

            var colors = new Color[palette.Length];
            for (int i = 0; i < palette.Length; i++)
            {
                var rgba = palette[i];
                colors[i] = Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
            }
            _colors = colors;
        }
    }

    private ColorPaletteView m_pColorPalletView = new();
    private SprDocument? m_pNowSprite;

    public SprDocument? NowSprite
    {
        get => m_pNowSprite;
        set
        {
            m_pNowSprite = value;
            m_pColorPalletView = new ColorPaletteView();
            m_pColorPalletView.SetOrigin(value?.Palette);
        }
    }

    public ColorPaletteView NowPalette => m_pColorPalletView;

    public int PlaySpeed = 10;

    /// <summary>File name of the loaded sprite, shown in the window caption.</summary>
    public string? FileName;
}

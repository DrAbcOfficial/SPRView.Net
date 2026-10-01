using Avalonia.Controls;
using Avalonia.Media;
using SixLabors.ImageSharp.PixelFormats;
using SPRView.Net.Core;

namespace SPRView.Net.Storage;

/// <summary>
/// Per application instance state: the sprite being viewed and derived views.
/// </summary>
public class AppStorage
{
    /// <summary>
    /// Adapts the sprite palette to Avalonia's color palette view model.
    /// </summary>
    public class ColorPaletteView : Avalonia.Controls.IColorPalette
    {
        private SprPalette? _original;

        public int ColorCount
        {
            get
            {
                if (_original == null)
                    return 0;
                return _original.Length < 16 ? _original.Length : 16;
            }
        }

        public int ShadeCount
        {
            get
            {
                if (_original == null)
                    return 0;
                return Math.Max(1, _original.Length / 16);
            }
        }

        public Color GetColor(int colorIndex, int shadeIndex)
        {
            if (_original == null)
                return Color.FromUInt32(0);
            Rgba32 rgba = _original[colorIndex + (shadeIndex * 16)];
            return Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
        }

        public SprPalette? GetOrigin() => _original;

        public void SetOrigin(SprPalette orgPalette) => _original = orgPalette;

        public bool IsValid() => _original != null;
    }

    private ColorPaletteView m_pColorPalletView = new();
    private SprDocument? m_pNowSprite;

    public SprDocument? NowSprite
    {
        get => m_pNowSprite;
        set
        {
            if (value != null)
            {
                m_pNowSprite = value;
                m_pColorPalletView = new();
                m_pColorPalletView.SetOrigin(value.Palette);
            }
        }
    }

    public ColorPaletteView NowPalette => m_pColorPalletView;

    public int PlaySpeed = 10;
}

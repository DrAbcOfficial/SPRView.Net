using Avalonia.Controls;
using Avalonia.Media;
using SixLabors.ImageSharp.PixelFormats;
using SPRView.Net.Lib;
using SPRView.Net.Lib.Interface;

namespace SPRView.Net.Storage;

public class CStorage
{
    public class ColorPaletteView : IColorPalette
    {
        private ISpriteColorPalette? Original;
        public int ColorCount { get => 16; }
        public int ShadeCount { get => Original.Size / 16; }
        public Color GetColor(int colorIndex, int shadeIndex)
        {
            Rgba32 rgba = Original.AtIndex(colorIndex + shadeIndex * 16);
            return Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
        }
        public void SetOrigin(ISpriteColorPalette orgPalette)
        {
            Original = orgPalette;
        }
        public bool IsValid()
        {
            return Original != null;
        }
    }
    private readonly ColorPaletteView m_pColorPalletView = new();
    private CSprite? m_pNowSprite;
    public CSprite? NowSprite {
        get => m_pNowSprite;
        set
        {
            if(value != null)
            {
                m_pNowSprite = value;
                m_pColorPalletView.SetOrigin(m_pNowSprite.Pallete);
            }
        }
    }
    public ColorPaletteView NowPalette
    {
        get => m_pColorPalletView;
    }
    public int PlaySpeed = 10;
}

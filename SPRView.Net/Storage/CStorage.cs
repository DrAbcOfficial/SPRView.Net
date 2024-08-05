using Avalonia.Controls;
using Avalonia.Media;
using SixLabors.ImageSharp.PixelFormats;
using SPRView.Net.Lib;
using SPRView.Net.Lib.Interface;
using System;

namespace SPRView.Net.Storage;

public class CStorage
{
    public class ColorPaletteView : IColorPalette
    {
        private ISpriteColorPalette? Original;
        public int ColorCount
        {
            get
            {
                if (Original == null)
                    return 0;
                return Original.Length < 16 ? Original.Length : 16;
            }
        }
        public int ShadeCount
        {
            get
            {
                if (Original == null)
                    return 0;
                return Math.Max(1, Original.Length / 16);
            }
        }
        public Color GetColor(int colorIndex, int shadeIndex)
        {
            if (Original == null)
                return Color.FromUInt32(0);
            Rgba32 rgba = Original.AtIndex(colorIndex + shadeIndex * 16);
            return Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B);
        }
        public ISpriteColorPalette? GetOrigin()
        {
            return Original;
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
    private ColorPaletteView m_pColorPalletView = new();
    private CSprite? m_pNowSprite;
    public CSprite? NowSprite
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
    public ColorPaletteView NowPalette
    {
        get => m_pColorPalletView;
    }
    public int PlaySpeed = 10;
}

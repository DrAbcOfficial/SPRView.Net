using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SPRView.Net.Lib.Interface;

namespace SPRView.Net.Lib;

public class CFrame : IFrame
{
    public ISprite Parent { get => m_pParent; set { m_pParent = (CSprite)value; } }
    public int Group { get; set; }
    public int OriginX { get; set; }
    public int OriginY { get; set; }
    public Size Size { get; set; }

    private readonly Image<Rgba32> m_pBitmap;
    private CSprite m_pParent;

    public CFrame(byte[] data, CSpriteColorPalette pallet, int w, int h, int originX, int originY, int group, CSprite parent)
    {
        Size = new Size(w, h);
        OriginX = originX;
        OriginY = originY;
        Group = group;
        m_pParent = parent;

        m_pBitmap = new Image<Rgba32>(Size.Width, Size.Height);
        for (int y = 0; y < m_pBitmap.Height; y++)
            for (int x = 0; x < m_pBitmap.Width; x++)
                m_pBitmap[x, y] = pallet[data[y * m_pBitmap.Width + x]];
    }

    public Image GetImage()
    {
        if (m_pBitmap == null)
            throw new Exception("Sprite Frame has null iamge!");
        return m_pBitmap;
    }

    public static IFrame Create(byte[] data, ISpriteColorPalette pallet, int w, int h, int originX, int originY, int group, ISprite parent)
    {
        return new CFrame(data, (CSpriteColorPalette)pallet, w, h, originX, originY, group, (CSprite)parent);
    }
}

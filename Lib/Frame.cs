using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace SPRView.Net.Lib;

public class CFrame
{
    public CSprite Parent;
    public int Group { get; set; }
    public int OriginX { get; set; }
    public int OriginY { get; set; }
    public PixelSize Size { get; private set; }

    private readonly WriteableBitmap m_pBitmap;

    public CFrame(byte[] data, ColorPallet pallet, int w, int h, int originX, int originY, int group, CSprite parent)
    {
        Size = new PixelSize(w, h);
        OriginX = originX;
        OriginY = originY;
        Group = group;
        Parent = parent;

        m_pBitmap = new WriteableBitmap(Size, new Vector(100, 100), PixelFormat.Bgra8888);
        using var lockedBitmap = m_pBitmap.Lock();
        IntPtr buffer = lockedBitmap.Address;
        for (int i = 0; i < data.Length; i++)
        {
            byte pixel = data[i];
            if (pixel < pallet.Length && pixel >= 0)
            {
                unsafe
                {
                    byte* p = (byte*)buffer.ToPointer() + (i * 4);
                    p[0] = pallet[pixel].B;
                    p[1] = pallet[pixel].G;
                    p[2] = pallet[pixel].R;
                    p[3] = pallet[pixel].A;
                }
            }
        }
    }
    public Bitmap GetBitmap()
    {
        if (m_pBitmap == null)
            throw new Exception("Sprite Frame has null bitmap!");
        return m_pBitmap;
    }
}

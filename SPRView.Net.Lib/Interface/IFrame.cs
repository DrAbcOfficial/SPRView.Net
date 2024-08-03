using Avalonia.Media.Imaging;
using Avalonia;
namespace SPRView.Net.Lib.Interface;

public interface IFrame
{
    abstract public ISprite Parent { get; set; }
    abstract public int Group { get; set; }
    abstract public int OriginX { get; set; }
    abstract public int OriginY { get; set; }
    abstract public PixelSize Size { get; protected set; }

    abstract public static IFrame Create(byte[] data, ISpriteColorPalette pallet, int w, int h, int originX, int originY, int group, ISprite parent);
    abstract public Bitmap GetBitmap();
}

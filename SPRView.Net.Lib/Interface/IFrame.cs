using SixLabors.ImageSharp;
namespace SPRView.Net.Lib.Interface;

public interface IFrame
{
    abstract public ISprite Parent { get; set; }
    abstract public int Group { get; set; }
    abstract public int OriginX { get; set; }
    abstract public int OriginY { get; set; }
    abstract public Size Size { get; protected set; }

    abstract public static IFrame Create(byte[] data, ISpriteColorPalette pallet, int w, int h, int originX, int originY, int group, ISprite parent);
    abstract public Image GetImage();
}

using Avalonia.Media.Imaging;
namespace SPRView.Net.Lib.Interface;

public interface ISprite
{
    public enum SpriteType
    {
        ParallelUpright = 0,
        FacingUpright = 1,
        Parallel = 2,
        Oriented = 3,
        ParallelOriented = 4
    }

    public enum SpriteFormat
    {
        Normal = 0,
        Additive = 1,
        IndexAlpha = 2,
        AlphaTest = 3
    }

    public enum SpriteSynchron
    {
        Sync = 0,
        Random = 1
    }

    abstract public SpriteType Type { get; set; }
    abstract public SpriteFormat Format { get; set; }
    abstract public float BoundRadius { get; set; }
    abstract public uint MaxFrameWidth { get; set; }
    abstract public uint MaxFrameHeight { get; set; }
    abstract public uint NumberOfFrames { get; set; }
    abstract public float BeamLength { get; set; }
    abstract public SpriteSynchron Synchronization { get; set; }
    abstract public ISpriteColorPalette Pallete { get; set; }
    abstract public IFrame GetFrame(int i);
    abstract public int GetFrames();
    abstract public static ISprite Create(Stream stream);
    abstract public static ISprite Create(string? path);
    abstract public Bitmap GetBitmap(int frame);
}

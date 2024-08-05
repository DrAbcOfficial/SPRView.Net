using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SPRView.Net.Lib.Interface;
using System.Diagnostics.CodeAnalysis;
namespace SPRView.Net.Lib;
public class CSprite : ISprite
{
    public ISprite.SpriteType Type { get; set; }
    public ISprite.SpriteFormat Format { get; set; }
    public float BoundRadius { get; set; }
    public uint MaxFrameWidth { get; set; }
    public uint MaxFrameHeight { get; set; }
    public uint NumberOfFrames { get; set; }
    public float BeamLength { get; set; }
    public ISprite.SpriteSynchron Synchronization { get; set; }
    public ISpriteColorPalette Palette { get => m_pPallete; set { m_pPallete = (CSpriteColorPalette)value; } }
    private CSpriteColorPalette m_pPallete { get; set; }
    private List<CFrame> m_aryFrames { get; set; } = [];
    public List<CFrame> Frames { get => m_aryFrames; set { m_aryFrames = value; } }

#pragma warning disable CS8618
    private void Init(Stream stream)
    {
        using BinaryReader br = new(stream);
        int identify = br.ReadInt32();
        if (identify != 0x50534449)
            throw new InvalidDataException("File not a valid goldsrc sprite");
        int version = br.ReadInt32();
        if (version != 2)
            throw new InvalidDataException("File not a supported goldsrc sprite version");
        Type = (ISprite.SpriteType)br.ReadInt32();
        Format = (ISprite.SpriteFormat)br.ReadInt32();
        BoundRadius = br.ReadSingle();
        MaxFrameWidth = (uint)br.ReadInt32();
        MaxFrameHeight = (uint)br.ReadInt32();
        NumberOfFrames = (uint)br.ReadInt32();
        BeamLength = br.ReadSingle();
        Synchronization = (ISprite.SpriteSynchron)br.ReadInt32();

        short palletSize = br.ReadInt16();
        m_pPallete = new CSpriteColorPalette(palletSize);
        for (int i = 0; i < palletSize; i++)
        {
            m_pPallete[i] = new Rgba32(br.ReadByte(), br.ReadByte(), br.ReadByte(), 255);
        }
        for (int i = 0; i < NumberOfFrames; i++)
        {
            int group = br.ReadInt32();
            int originX = br.ReadInt32();
            int originY = br.ReadInt32();
            int width = br.ReadInt32();
            int height = br.ReadInt32();
            byte[] data = br.ReadBytes(width * height);
            m_aryFrames.Add(new CFrame(data, m_pPallete, width, height, originX, originY, group, this));
        }
    }
    public CSprite(Stream stream)
    {
        Init(stream);
    }
    public CSprite(string? path)
    {
        if(path == null) 
            throw new FileLoadException("Open a null invalid file path");
        using FileStream fs = new(path, FileMode.Open);
        Init(fs);
    }
#pragma warning restore CS8618

    public static ISprite Create(Stream stream)
    {
        return new CSprite(stream);
    } 
    public static ISprite Create(string? path)
    {
        return new CSprite(path);
    }

    public IFrame GetFrame(int index)
    {
        return m_aryFrames[index];
    }
    public int GetFrames()
    {
        return m_aryFrames.Count;
    }

    public Image GetImage(int frame)
    {
        if(frame < 0 || frame >= m_aryFrames.Count)
        {
            throw new IndexOutOfRangeException("Frame index out of bound");
        }
        return m_aryFrames[frame].GetImage();
    }
}

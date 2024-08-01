using Avalonia.Media.Imaging;
using Avalonia.Media;
using System.Collections.Generic;
using System.IO;
using System;
namespace SPRView.Net.Lib;
public class CSprite
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

    public SpriteType Type { get; set; }
    public SpriteFormat Format { get; set; }
    public float BoundRadius { get; set; }
    public uint MaxFrameWidth { get; set; }
    public uint MaxFrameHeight { get; set; }
    public uint NumberOfFrames { get; set; }
    public float BeamLength { get; set; }

    public SpriteSynchron Synchronization { get; set; }


    public ColorPallet Pallete { get; set; }
    public List<CFrame> Frames { get; private set; } = [];

    private void Init(Stream stream)
    {
        using BinaryReader br = new(stream);
        int identify = br.ReadInt32();
        if (identify != 0x50534449)
            throw new InvalidDataException("File not a valid goldsrc sprite");
        int version = br.ReadInt32();
        if (version != 2)
            throw new InvalidDataException("File not a supported goldsrc sprite version");
        Type = (SpriteType)br.ReadInt32();
        Format = (SpriteFormat)br.ReadInt32();
        BoundRadius = br.ReadSingle();
        MaxFrameWidth = (uint)br.ReadInt32();
        MaxFrameHeight = (uint)br.ReadInt32();
        NumberOfFrames = (uint)br.ReadInt32();
        BeamLength = br.ReadSingle();
        Synchronization = (SpriteSynchron)br.ReadInt32();

        short palletSize = br.ReadInt16();
        Pallete = new ColorPallet(palletSize);
        for (int i = 0; i < palletSize; i++)
        {
            Pallete[i] = Color.FromArgb(255, br.ReadByte(), br.ReadByte(), br.ReadByte());
        }
        for (int i = 0; i < NumberOfFrames; i++)
        {
            int group = br.ReadInt32();
            int originX = br.ReadInt32();
            int originY = br.ReadInt32();
            int width = br.ReadInt32();
            int height = br.ReadInt32();
            byte[] data = br.ReadBytes(width * height);
            Frames.Add(new CFrame(data, Pallete, width, height, originX, originY, group, this));
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

    public Bitmap GetBitmap(int frame)
    {
        if(frame < 0 || frame >= Frames.Count)
        {
            throw new IndexOutOfRangeException("Frame index out of bound");
        }
        return Frames[frame].GetBitmap();
    }
}

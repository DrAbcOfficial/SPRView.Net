using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SPRView.Net.Lib.Interface;
namespace SPRView.Net.Lib.Class;
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
        if (path == null)
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
        if (frame < 0 || frame >= m_aryFrames.Count)
        {
            throw new IndexOutOfRangeException("Frame index out of bound");
        }
        return m_aryFrames[frame].GetImage();
    }
    public static void Save(string[] files, Stream stream, int width, int height,
        ISprite.SpriteFormat format = ISprite.SpriteFormat.Normal, ISprite.SpriteType type = ISprite.SpriteType.Parallel, ISprite.SpriteSynchron sync = ISprite.SpriteSynchron.Sync,
        float beamlength = 0, bool bUnpack = false)
    {
        //量化
        //降低一个维度，以便统一量化
        int buffersize = 0;
        if (bUnpack)
        {
            foreach (var path in files)
            {
                Image frame = Image.Load(path);
                buffersize += frame.Frames.Count;
                frame.Dispose();
            }
            //会多一个
            buffersize--;
        }
        else
            buffersize = files.Length;
        using Image<Rgba32> image = new(width, height * buffersize);
        int bufferseek = 0;
        for (int i = 0; i < files.Length; i++)
        {
            var path = files[i];
            Image frame = Image.Load(path);
            frame.Mutate(x => x.Resize(width, height));
            do
            {
                image.Mutate(x => x.DrawImage(frame, new Point(0, height * bufferseek), 1.0f));
                if (frame.Frames.Count > 1)
                    frame.Frames.RemoveFrame(0);
                bufferseek++;
            } while (frame.Frames.Count > 1);
            frame.Dispose();
        }
        bool isAlphaTest = format == ISprite.SpriteFormat.AlphaTest;
        WuQuantizer quantizer = new(new QuantizerOptions
        {
            Dither = null,
            MaxColors = isAlphaTest ? 255 : 256
        });
        image.Mutate(x => x.Quantize(quantizer));
        //生成色板
        Dictionary<Rgba32, byte> palette = [];
        for (int j = 0; j < image.Height; j++)
        {
            for (int i = 0; i < image.Width; i++)
            {
                Rgba32 rgba32 = image[i, j];
                if (!palette.ContainsKey(rgba32))
                    palette.Add(rgba32, (byte)palette.Count);
            }
        }
        if (isAlphaTest)
            palette.Add(new Rgba32(0, 0, 255, 255), 255);
        //保存
        using BinaryWriter writer = new(stream);
        //Header
        writer.Write(0x50534449);
        //Version
        writer.Write(0x00000002);
        //Type
        writer.Write((int)type);
        //Format
        writer.Write((int)format);
        //BoundRadius
        float radius = (float)Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2)) / 2;
        writer.Write(radius);
        //Width
        writer.Write(width);
        //Height
        writer.Write(height);
        //Count
        writer.Write(buffersize);
        //BeamLength
        writer.Write(beamlength);
        //Sync
        writer.Write((int)sync);
        //Palette Size
        writer.Write((short)palette.Count);
        //Palette
        foreach (var p in palette.Keys)
        {
            writer.Write(p.R);
            writer.Write(p.G);
            writer.Write(p.B);
        }
        if (palette.Count < 256 && isAlphaTest)
        {
            for (int i = palette.Count; i < 255; i++)
            {
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);
            }
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)255);
        }
        //保存数据
        for (int k = 0; k < buffersize; k++)
        {
            //Group
            writer.Write(0x00000000);
            //OriginX
            writer.Write(0x00000000);
            //OriginY
            writer.Write(0x00000000);
            //Width
            writer.Write(width);
            //Height
            writer.Write(height);

            var startY = k * height;
            for (int j = startY; j < startY + height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    Rgba32 rgba32 = image[i, j];
                    if (isAlphaTest && rgba32.A <= 128)
                        writer.Write(palette.Last().Value);
                    else
                        writer.Write(palette[rgba32]);
                }
            }
        }
    }
}

namespace SPRView.Net.Core;

/// <summary>
/// The 24 byte header describing a GoldSrc sprite document.
/// </summary>
public sealed class SprHeader
{
    public const int Signature = 0x50534449; // "IDSP"
    public const int SupportedVersion = 2;

    public SpriteType Type { get; set; }
    public SpriteFormat Format { get; set; }
    public float BoundRadius { get; set; }
    public uint MaxFrameWidth { get; set; }
    public uint MaxFrameHeight { get; set; }
    public uint NumberOfFrames { get; set; }
    public float BeamLength { get; set; }
    public SpriteSynchron Synchronization { get; set; }
}

namespace SPRView.Net.Core;

/// <summary>
/// GoldSrc sprite rendering type.
/// </summary>
public enum SpriteType
{
    ParallelUpright = 0,
    FacingUpright = 1,
    Parallel = 2,
    Oriented = 3,
    ParallelOriented = 4
}

/// <summary>
/// GoldSrc sprite blending format.
/// </summary>
public enum SpriteFormat
{
    Normal = 0,
    Additive = 1,
    IndexAlpha = 2,
    AlphaTest = 3
}

/// <summary>
/// GoldSrc sprite playback synchronization.
/// </summary>
public enum SpriteSynchron
{
    Sync = 0,
    Random = 1
}

namespace SPRView.Net.Core.Native;

/// <summary>
/// Status codes shared with native consumers; see sprview_core.h.
/// </summary>
internal enum SprNativeResult
{
    Ok = 0,
    InvalidHandle = -1,
    FrameOutOfRange = -2,
    BufferTooSmall = -3,
    IoError = -4,
    InvalidArgument = -5,
    UnknownError = -6
}

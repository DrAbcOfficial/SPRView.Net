using System.Runtime.InteropServices.Marshalling;

namespace SPRView.Net.ThumbnailProvider.Windows.Com;

/// <summary>
/// Single <see cref="StrategyBasedComWrappers"/> instance backing the CCWs the
/// shell consumes; identity and interface caches must stay process wide.
/// </summary>
internal static class ComWrappersSource
{
    public static readonly StrategyBasedComWrappers Instance = new();
}

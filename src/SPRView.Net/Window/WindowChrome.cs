using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace SPRView.Net;

/// <summary>
/// Chrome plumbing shared by the borderless windows.
///
/// The windows use <c>WindowDecorations="None"</c> together with
/// <c>ExtendClientAreaToDecorationsHint</c> so the caption is drawn by the app
/// and looks identical on Windows, macOS and Linux. The cost is that the OS no
/// longer supplies resize hit testing, so thin grips are added along the edges.
/// </summary>
public static class WindowChrome
{
    /// <summary>Edge thickness of the resize grips, in device independent pixels.</summary>
    private const double EdgeThickness = 5;
    private const double CornerSize = 12;

    /// <summary>
    /// Adds invisible resize grips over the window edges. Call from the window
    /// constructor once the content is set, and only for resizable windows.
    /// </summary>
    public static void AddResizeGrips(Window window)
    {
        if (window.Content is not Panel root)
            throw new InvalidOperationException(
                "The window content must be a Panel to host the resize grips.");

        var grips = new Grid { Name = "ResizeGrips" };

        // Corners go in first so the edge strips stay on top along the borders.
        grips.Children.Add(CornerGrip(WindowEdge.NorthWest, StandardCursorType.TopLeftCorner,
            HorizontalAlignment.Left, VerticalAlignment.Top));
        grips.Children.Add(CornerGrip(WindowEdge.NorthEast, StandardCursorType.TopRightCorner,
            HorizontalAlignment.Right, VerticalAlignment.Top));
        grips.Children.Add(CornerGrip(WindowEdge.SouthWest, StandardCursorType.BottomLeftCorner,
            HorizontalAlignment.Left, VerticalAlignment.Bottom));
        grips.Children.Add(CornerGrip(WindowEdge.SouthEast, StandardCursorType.BottomRightCorner,
            HorizontalAlignment.Right, VerticalAlignment.Bottom));

        grips.Children.Add(HorizontalGrip(WindowEdge.North, StandardCursorType.SizeNorthSouth,
            VerticalAlignment.Top));
        grips.Children.Add(HorizontalGrip(WindowEdge.South, StandardCursorType.SizeNorthSouth,
            VerticalAlignment.Bottom));
        grips.Children.Add(Grip(WindowEdge.West, StandardCursorType.SizeWestEast,
            HorizontalAlignment.Left));
        grips.Children.Add(Grip(WindowEdge.East, StandardCursorType.SizeWestEast,
            HorizontalAlignment.Right));

        grips.PointerPressed += (_, e) =>
        {
            if (!window.CanResize)
                return;
            if (!e.GetCurrentPoint(window).Properties.IsLeftButtonPressed)
                return;
            if (e.Source is Border { Tag: WindowEdge edge })
            {
                window.BeginResizeDrag(edge, e);
                e.Handled = true;
            }
        };

        root.Children.Add(grips);
    }

    /// <summary>
    /// Builds one edge grip: a thin strip along its edge of the window.
    /// </summary>
    private static Border Grip(WindowEdge edge, StandardCursorType cursor, HorizontalAlignment horizontal)
        => new()
        {
            Tag = edge,
            Width = EdgeThickness,
            HorizontalAlignment = horizontal,
            Background = Brushes.Transparent,
            Cursor = new Cursor(cursor),
        };

    /// <summary>Builds one horizontal edge grip, stretched across the window.</summary>
    private static Border HorizontalGrip(WindowEdge edge, StandardCursorType cursor, VerticalAlignment vertical)
        => new()
        {
            Tag = edge,
            Height = EdgeThickness,
            VerticalAlignment = vertical,
            Background = Brushes.Transparent,
            Cursor = new Cursor(cursor),
        };

    /// <summary>Builds a corner grip: a small square pinned to one corner.</summary>
    private static Border CornerGrip(WindowEdge edge, StandardCursorType cursor,
        HorizontalAlignment horizontal, VerticalAlignment vertical)
        => new()
        {
            Tag = edge,
            Width = CornerSize,
            Height = CornerSize,
            HorizontalAlignment = horizontal,
            VerticalAlignment = vertical,
            Background = Brushes.Transparent,
            Cursor = new Cursor(cursor),
        };
}

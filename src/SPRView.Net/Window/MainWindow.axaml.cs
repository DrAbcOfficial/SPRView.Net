using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SPRView.Net.ViewModel;

namespace SPRView.Net;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // The window is chromeless: edges are resized by app-drawn grips and
        // drop targets are handled here rather than by the platform.
        WindowChrome.AddResizeGrips(this);
        // DragEnter fires when the pointer first enters a drop target and is the
        // only feedback some backends give; DragOver keeps it up to date after.
        AddHandler(DragDrop.DragEnterEvent, OnDragOver);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        AddHandler(DragDrop.DropEvent, OnDrop);

        ViewportScroll.SizeChanged += (_, e) => Vm?.OnViewportSizeChanged(e.NewSize);
        SizeChanged += (_, e) => UpdateCommandBar(e.NewSize.Width);
        // The first layout pass can finish before the canvas reports a size,
        // so the fit is recomputed once the window is on screen.
        Opened += (_, _) => Vm?.OnViewportSizeChanged(ViewportScroll.Bounds.Size);

        PropertyChanged += (_, e) =>
        {
            if (e.Property == WindowStateProperty)
                UpdateMaximizeGlyph();
        };
    }

    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;

    /// <summary>Collapses the command bar to icons when the window is narrow.</summary>
    private void UpdateCommandBar(double width)
    {
        if (Vm is { } vm)
            vm.ShowCommandLabels = width >= MainWindowViewModel.CommandLabelsMinWidth;
    }

    /// <summary>Exposed so the layout tests can assert the drag feedback.</summary>
    public bool DropHintVisible => DropHint.IsVisible;

    #region Caption

    private void OnCaptionPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed)
            return;

        if (e.ClickCount == 2 && CanResize)
        {
            ToggleMaximize();
            e.Handled = true;
            return;
        }

        BeginMoveDrag(e);
        e.Handled = true;
    }

    private void OnMinimizeClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void OnMaximizeClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => ToggleMaximize();

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.Exit();

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    /// <summary>Swaps between the maximize and restore glyphs of the caption.</summary>
    private void UpdateMaximizeGlyph()
    {
        string key = WindowState == WindowState.Maximized ? "Glyph_Restore" : "Glyph_Maximize";
        if (this.TryFindResource(key, out object? glyph) && glyph is Avalonia.Media.StreamGeometry geometry)
            MaximizeGlyph.Data = geometry;
    }

    #endregion

    #region Drag and drop

    private static bool HasSpriteFile(DragEventArgs e)
        => e.DataTransfer.TryGetFiles()?.Any() == true;

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        bool accept = HasSpriteFile(e);
        e.DragEffects = accept ? DragDropEffects.Copy : DragDropEffects.None;
        DropHint.IsVisible = accept;
        e.Handled = true;
    }

    private void OnDragLeave(object? sender, DragEventArgs e)
        => DropHint.IsVisible = false;

    private void OnDrop(object? sender, DragEventArgs e)
    {
        DropHint.IsVisible = false;
        if (!HasSpriteFile(e))
            return;

        var item = e.DataTransfer.TryGetFiles()?.FirstOrDefault();
        if (item != null)
            Vm?.OpenStorageItem(item);
        e.Handled = true;
    }

    #endregion

    #region Command bar

    private void OnNewClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.CreateFile();

    private void OnOpenClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.OpenFile();

    private void OnSaveFrameClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.SaveFrame();

    private void OnSaveGifClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.SaveGIF();

    private void OnExportClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.Export();

    private void OnSavePaletteClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.SavePalette();

    private void OnAboutClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.About();

    private void OnToggleInfoSection(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Vm is { } vm)
            vm.IsInfoExpanded = !vm.IsInfoExpanded;
    }

    private void OnTogglePaletteSection(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Vm is { } vm)
            vm.IsPaletteExpanded = !vm.IsPaletteExpanded;
    }

    #endregion

    #region Transport

    private void OnTogglePlayClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.ToggleAnimationTimer();

    private void OnZoomInClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.ZoomIn();

    private void OnZoomOutClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.ZoomOut();

    private void OnZoomResetClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.ZoomReset();

    private void OnZoomFitClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.ZoomToFit();

    #endregion
}

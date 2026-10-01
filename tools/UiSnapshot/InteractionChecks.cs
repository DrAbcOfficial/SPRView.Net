using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Input.Raw;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SPRView.Net;
using SPRView.Net.Core;
using SPRView.Net.ViewModel;

namespace UiSnapshot;

/// <summary>
/// Drives the real windows through the headless input pipeline and checks the
/// resulting state. This exercises the actual event wiring (key bindings,
/// button handlers, bindings) rather than a mock, and runs in CI without a
/// desktop session.
/// </summary>
internal static class InteractionChecks
{
    private static int _passed;
    private static int _failed;

    public static int Run(string? sprPath)
    {
        if (sprPath == null || !File.Exists(sprPath))
        {
            Console.WriteLine("interaction checks need a sprite path");
            return 1;
        }

        CheckMainWindow(sprPath);
        CheckExtremeSizes(sprPath);
        CheckDragAndDrop(sprPath);
        CheckCreateNewWindow(sprPath);

        Console.WriteLine($"interaction: {_passed} passed, {_failed} failed");
        return _failed == 0 ? 0 : 1;
    }

    private static void CheckMainWindow(string sprPath)
    {
        var window = new MainWindow();
        var vm = new MainWindowViewModel(window) { Lang = LangLoader.Load("en") };
        window.DataContext = vm;
        window.Show();
        Settle();

        LoadSprite(vm, sprPath, "demo.spr");
        Settle();

        // Arrow keys step frames.
        int before = vm.NowFrame;
        window.KeyPressQwerty(PhysicalKey.ArrowRight, RawInputModifiers.None);
        Settle();
        Expect(vm.NowFrame == before + 1, "ArrowRight advances the frame");

        window.KeyPressQwerty(PhysicalKey.ArrowLeft, RawInputModifiers.None);
        Settle();
        Expect(vm.NowFrame == before, "ArrowLeft steps back");

        // Space toggles playback and the play glyph state follows.
        window.KeyPressQwerty(PhysicalKey.Space, RawInputModifiers.None);
        Settle();
        Expect(vm.IsPlaying, "Space starts playback");
        window.KeyPressQwerty(PhysicalKey.Space, RawInputModifiers.None);
        Settle();
        Expect(!vm.IsPlaying, "Space stops playback");

        // Loading fits the sprite to the canvas, so start from 1:1.
        window.KeyPressQwerty(PhysicalKey.Digit0, RawInputModifiers.Control);
        Settle();
        Expect(Math.Abs(vm.ZoomPercent - 100) < 0.5, "Ctrl+0 resets to 100%");

        // Ctrl+plus / minus walk the zoom ladder.
        double zoom = vm.ZoomPercent;
        window.KeyPressQwerty(PhysicalKey.Equal, RawInputModifiers.Control);
        Settle();
        Expect(vm.ZoomPercent > zoom, "Ctrl+= zooms in");

        window.KeyPressQwerty(PhysicalKey.Minus, RawInputModifiers.Control);
        Settle();
        Expect(Math.Abs(vm.ZoomPercent - 100) < 0.5, "Ctrl+- steps back down");

        // Fit to window is a button handler rather than a key binding.
        vm.ZoomToFit();
        Settle();
        Expect(vm.ZoomPercent > 100, "fit upscales a small sprite");
        Expect(vm.ZoomPercent % 100 == 0, "fit keeps whole-number zoom steps");

        // Transparency toggle: on = the sprite format's rule is applied, off =
        // the frame is shown exactly as stored.
        var withTransparency = vm.SPR!;
        Expect(vm.ApplyTransparency, "transparency starts enabled");
        vm.ApplyTransparency = false;
        Settle();
        Expect(!vm.ApplyTransparency, "the toggle switches off");
        Expect(!ReferenceEquals(vm.SPR, withTransparency), "switching re-decodes the frame");

        int opaqueOff = CountOpaque(vm.SPR!);
        vm.ApplyTransparency = true;
        Settle();
        int opaqueOn = CountOpaque(vm.SPR!);
        Expect(opaqueOff > opaqueOn,
            $"hiding transparency removes pixels ({opaqueOn} opaque vs {opaqueOff} raw)");

        // Switching back and forth must be stable.
        vm.ApplyTransparency = false;
        vm.ApplyTransparency = true;
        Settle();
        Expect(CountOpaque(vm.SPR!) == opaqueOn, "toggling twice returns to the same image");
        Expect(vm.SPR!.Size == withTransparency.Size, "the frame geometry is unchanged");

        // Drive the real command bar button, so the binding is covered and not
        // just the property behind it.
        var toggle = window.FindControl<ToggleButton>("TransparencyToggle");
        Expect(toggle != null, "the command bar exposes the transparency toggle");
        if (toggle != null)
        {
            Expect(toggle.IsChecked == true, "the button reflects the enabled state");
            toggle.IsChecked = false;
            Settle();
            Expect(!vm.ApplyTransparency, "clicking the button turns transparency off");
            toggle.IsChecked = true;
            Settle();
            Expect(vm.ApplyTransparency, "clicking it again turns transparency back on");
        }

        // Section headers inside the side panel collapse independently of the
        // command bar, so a long palette can use the whole panel.
        vm.ShowPalettePanel = true;
        Settle();

        // The panel is the outer control and starts collapsed.
        Expect(!vm.ShowSidePanel, "the side panel starts collapsed");
        Expect(!vm.CanShowSideBar, "nothing shows on the right until it is asked for");
        Expect(!vm.SectionTogglesEnabled, "the section toggles grey out with the panel");

        var panelToggle = window.FindControl<ToggleButton>("SidePanelToggle");
        Expect(panelToggle != null, "the command bar exposes the side panel toggle");
        if (panelToggle != null)
        {
            Expect(panelToggle.IsChecked == false, "the toggle shows the collapsed state");
            panelToggle.IsChecked = true;
            Settle();
            Expect(vm.ShowSidePanel && vm.CanShowSideBar, "the toggle opens the panel");
            Expect(vm.SectionTogglesEnabled, "opening the panel enables the section toggles");
            Expect(vm.ShowInfoPanel, "the section choice is remembered");
            Expect(vm.ShowPalettePanel, "the palette choice is remembered");
        }

        Expect(vm.IsInfoExpanded && vm.IsPaletteExpanded, "both sections start expanded");
        Expect(vm.ShowInfoDivider, "the divider shows while both sections are open");

        vm.IsInfoExpanded = false;
        Settle();
        Expect(!vm.IsInfoExpanded, "the information section collapses");
        Expect(!vm.ShowInfoDivider, "collapsing information drops the divider");

        var infoHeader = window.FindControl<Button>("InfoSectionHeader");
        Expect(infoHeader != null, "the panel exposes the information header button");
        if (infoHeader != null)
        {
            // A Button has no click verb in the API; raising the event is what
            // the pointer would do, and it runs the real handler.
            infoHeader.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Settle();
            Expect(vm.IsInfoExpanded, "clicking the header expands it again");
            infoHeader.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Settle();
            Expect(!vm.IsInfoExpanded, "clicking it again collapses it");
        }

        var paletteHeader = window.FindControl<Button>("PaletteSectionHeader");
        Expect(paletteHeader != null, "the panel exposes the palette header button");
        if (paletteHeader != null)
        {
            paletteHeader.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Settle();
            Expect(!vm.IsPaletteExpanded, "the palette section collapses");
            paletteHeader.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Settle();
            Expect(vm.IsPaletteExpanded, "the palette section expands again");
        }

        vm.IsInfoExpanded = true;
        Settle();

        // The command bar opens on icons and only spells labels out once the
        // window is wide enough that they cannot clip.
        Expect(!vm.ShowCommandLabels, "the command bar starts on icons");
        Expect(MainWindowViewModel.CommandLabelsMinWidth > 0, "the threshold is defined");

        window.Width = 900;
        Settle();
        Expect(vm.ShowCommandLabels, "a wide window spells the labels out");
        window.Width = 720;
        Settle();
        Expect(!vm.ShowCommandLabels, "narrowing it goes back to icons");

        // Panel toggles are plain properties the command bar binds to.
        vm.ShowPalettePanel = true;
        Settle();
        Expect(vm.CanShowSideBar, "opening the palette shows the side bar");
        vm.ShowInfoPanel = false;
        vm.ShowPalettePanel = false;
        Settle();
        Expect(!vm.CanShowSideBar, "closing both panels hides the side bar");

        // Switching language must relabel the caption and the palette count.
        vm.ShowPalettePanel = true;
        vm.ChangeLang("zh");
        Settle();
        Expect(vm.Lang!.TaskBar_View_Information == "信息", "ChangeLang loads Chinese");
        Expect(vm.PaletteCountLabel.Contains("色"), "palette count is localized");
        vm.ChangeLang("en");
        Settle();

        // Playback must stop when the document is replaced.
        vm.ToggleAnimationTimer();
        Settle();
        Expect(vm.IsPlaying, "playback started before reload");
        LoadSprite(vm, sprPath, "second.spr");
        Settle();
        Expect(!vm.IsPlaying, "loading a document stops playback");

        window.Close();
    }

    /// <summary>
    /// Shrinks the window to absurd sizes and checks the layout holds.
    ///
    /// Sizes well under the declared minimum are requested on purpose: the
    /// platform clamps them, but a layout that only survives because of that
    /// clamp would still break the moment the clamp changed, so the content is
    /// also measured while tiny.
    /// </summary>
    private static void CheckExtremeSizes(string sprPath)
    {
        var window = new MainWindow();
        var vm = new MainWindowViewModel(window) { Lang = LangLoader.Load("en") };
        window.DataContext = vm;
        window.Show();
        Settle();
        LoadSprite(vm, sprPath, "demo.spr");
        Settle();

        // What the window asks for versus what it allows.
        Expect(window.MinWidth > 0 && window.MinHeight > 0,
            $"the window declares a floor ({window.MinWidth}x{window.MinHeight})");

        // Negative sizes are rejected by Avalonia's own property guard, so the
        // cases worth testing are the non-negative ones that a drag, a snap or a
        // window manager restore can actually produce.
        foreach (var (width, height) in new[]
                 { (0.0, 0.0), (1.0, 1.0), (40.0, 30.0), (599.0, 439.0), (600.0, 440.0) })
        {
            window.Width = width;
            window.Height = height;
            Settle();

            double actualWidth = window.ClientSize.Width;
            double actualHeight = window.ClientSize.Height;
            Expect(actualWidth >= window.MinWidth - 0.5 && actualHeight >= window.MinHeight - 0.5,
                $"requesting {width}x{height} settles at {actualWidth:F0}x{actualHeight:F0}, not below the floor");
        }

        // The content has to stay laid out at the floor, panel open, rather than
        // collapsing to zero or throwing while measuring.
        window.Width = 600;
        window.Height = 440;
        vm.ShowSidePanel = true;
        vm.ShowPalettePanel = true;
        Settle();
        Expect(window.ClientSize.Width > 0 && window.ClientSize.Height > 0,
            "the window still has a size with every panel open at the floor");
        Expect(vm.SPR != null, "the sprite is still decoded at the floor size");

        // A window manager may ignore the declared minimum, so the content is
        // laid out at sizes below the floor as well. Measuring and arranging the
        // root directly bypasses the window clamp and shows whether the visuals
        // survive on their own.
        foreach (var (w, h) in new[] { (200.0, 150.0), (80.0, 60.0), (16.0, 16.0) })
        {
            var root = (Control)window.Content!;
            root.Measure(new Size(w, h));
            root.Arrange(new Rect(0, 0, w, h));
            Dispatcher.UIThread.RunJobs();

            var measured = root.Bounds;
            Expect(measured.Width >= 0 && measured.Height >= 0 &&
                   !double.IsNaN(measured.Width) && !double.IsNaN(measured.Height),
                $"content lays out without NaN or negative bounds at {w}x{h}");

            // Layout alone can look fine while the render throws, so the frame
            // is captured too. At these sizes it will be empty or clipped; what
            // matters is that the pass completes and yields a bitmap.
            var tiny = window.CaptureRenderedFrame();
            Expect(tiny != null, $"the render pass completes at {w}x{h}");
            tiny?.Dispose();
        }

        // Restoring a normal size must return the layout to normal.
        window.Width = 720;
        window.Height = 560;
        Settle();
        Expect(Math.Abs(window.ClientSize.Width - 720) < 1 && Math.Abs(window.ClientSize.Height - 560) < 1,
            "restoring the default size works after shrinking");

        window.Close();
    }

    /// <summary>
    /// Drives a file drop through the real handlers. The window must start
    /// empty so the check proves the drop is what loaded the document.
    /// </summary>
    private static void CheckDragAndDrop(string sprPath)
    {
        App.Storage.NowSprite = null;
        App.Storage.FileName = null;

        var window = new MainWindow();
        var vm = new MainWindowViewModel(window) { Lang = LangLoader.Load("en") };
        window.DataContext = vm;
        window.Show();
        Settle();
        Expect(!vm.HasSprite, "drop check starts with no sprite");

        // IStorageFile cannot be implemented by user code, so ask the platform
        // for a real item. Headless may not provide one; skip in that case.
        IStorageFile? file = window.StorageProvider
            .TryGetFileFromPathAsync(sprPath).GetAwaiter().GetResult();
        if (file == null)
        {
            Console.WriteLine("  skip drag and drop: no storage provider in this host");
            window.Close();
            return;
        }

        var transfer = new DataTransfer();
        transfer.Add(DataTransferItem.CreateFile(file));
        var point = new Point(400, 300);

        window.DragDrop(point, RawDragEventType.DragEnter, transfer, DragDropEffects.Copy, RawInputModifiers.None);
        Settle();
        Expect(window.DropHintVisible, "dragging a file over the window shows the drop hint");

        window.DragDrop(point, RawDragEventType.DragLeave, transfer, DragDropEffects.None, RawInputModifiers.None);
        Settle();
        Expect(!window.DropHintVisible, "leaving the window hides the drop hint");

        window.DragDrop(point, RawDragEventType.DragOver, transfer, DragDropEffects.Copy, RawInputModifiers.None);
        Settle();
        Expect(window.DropHintVisible, "dragging back in shows the hint again");

        window.DragDrop(point, RawDragEventType.Drop, transfer, DragDropEffects.Copy, RawInputModifiers.None);
        Settle();
        Expect(vm.HasSprite, "dropping a .spr file loads it");
        Expect(vm.CaptionPrimary == Path.GetFileName(sprPath), "the caption shows the dropped file name");
        Expect(!window.DropHintVisible, "the drop hint hides after the drop");

        window.Close();
    }

    private static void CheckCreateNewWindow(string sprPath)
    {
        var window = new CreateNewWindow();
        var vm = new CreateNewViewModel(window) { Lang = LangLoader.Load("en") };
        window.DataContext = vm;
        window.Show();
        Settle();

        string dir = Path.GetDirectoryName(sprPath)!;
        string[] frames = Directory.GetFiles(dir, "frame*.png").OrderBy(f => f).ToArray();
        vm.SetImages(frames);
        Settle();

        Expect(vm.Entries.Count == frames.Length, "list rows match the image count");
        Expect(vm.Entries[0].Index == 1, "row numbering is one based");
        Expect(vm.Preview_MaxFrame == frames.Length - 1, "preview maximum follows the list");
        Expect(vm.SaveValid, "export is enabled once frames exist");

        int selected = vm.PathSelected;
        vm.MovedownImage();
        Settle();
        Expect(vm.PathSelected == selected + 1, "move down follows the moved item");
        Expect(vm.Entries[selected].Name == Path.GetFileName(frames[selected + 1]),
            "the row order really changed");

        vm.MoveupImage();
        Settle();
        Expect(vm.PathSelected == selected, "move up restores the order");

        int count = vm.Entries.Count;
        vm.RemoveImage();
        Settle();
        Expect(vm.Entries.Count == count - 1, "remove drops one row");
        Expect(vm.PreviewFrameLabel.StartsWith("1 /"), "preview resets to the first frame");

        vm.SetImages([]);
        Settle();
        Expect(!vm.SaveValid, "export is disabled with an empty list");
        Expect(vm.PreviewImage == null, "preview clears with an empty list");

        window.Close();
    }

    /// <summary>Counts fully opaque pixels by reading the rendered bitmap back.</summary>
    private static unsafe int CountOpaque(Bitmap bitmap)
    {
        var size = bitmap.PixelSize;
        using var readable = new WriteableBitmap(size, new Vector(96, 96),
            PixelFormat.Bgra8888, AlphaFormat.Unpremul);
        using (var target = readable.Lock())
        {
            bitmap.CopyPixels(new PixelRect(size), target.Address,
                target.RowBytes * size.Height, target.RowBytes);
        }

        using var frame = readable.Lock();
        int opaque = 0;
        for (int y = 0; y < size.Height; y++)
        {
            byte* row = (byte*)frame.Address + (y * frame.RowBytes);
            for (int x = 0; x < size.Width; x++)
            {
                // BGRA8888, so the alpha channel is the fourth byte of the pixel.
                if (row[(x * 4) + 3] == 255)
                    opaque++;
            }
        }
        return opaque;
    }

    private static void LoadSprite(MainWindowViewModel vm, string path, string name)
    {
        using FileStream stream = File.OpenRead(path);
        SprDocument doc = SprDocument.Load(stream);
        vm.LoadSprite(doc, name);
    }

    private static void Settle()
    {
        for (int i = 0; i < 4; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        }
    }

    private static void Expect(bool condition, string what)
    {
        if (condition)
        {
            _passed++;
            Console.WriteLine($"  ok   {what}");
        }
        else
        {
            _failed++;
            Console.WriteLine($"  FAIL {what}");
        }
    }
}

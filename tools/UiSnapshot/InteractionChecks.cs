using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using SPRView.Net;
using SPRView.Net.Core;
using SPRView.Net.ViewModel;

namespace UiSnapshot;

/// <summary>
/// Renders each window off screen and writes a PNG, for reviewing the layout
/// without a desktop session.
///
///   dotnet run --project tools/UiSnapshot -- &lt;output dir&gt; [spr path]
/// </summary>
internal static class Program
{
    private static string _outDir = ".";
    private static string? _sprPath;
    private static string _lang = "en";
    private static string _mode = "render";
    private static int _exitCode;

    [STAThread]
    public static int Main(string[] args)
    {
        // Arguments: <output dir> [spr path] [lang] [mode]
        //   mode = render (default, writes PNGs) | check (asserts behaviour) | both
        _outDir = args.Length > 0 ? args[0] : ".";
        _sprPath = args.Length > 1 ? args[1] : null;
        _lang = args.Length > 2 ? args[2] : "en";
        _mode = args.Length > 3 ? args[3] : "render";
        Directory.CreateDirectory(_outDir);

        AppBuilder builder = AppBuilder.Configure<SnapshotApp>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .WithInterFont();

        builder.StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
        return 0;
    }

    /// <summary>Minimal host that mirrors the real App resource and style setup.</summary>
    private sealed class SnapshotApp : Application
    {
        public override void Initialize()
        {
            Styles.Add(new FluentTheme());
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                MergedDictionaries =
                {
                    (ResourceDictionary)AvaloniaXamlLoader.Load(
                        new Uri("avares://SPRView.Net/Themes/Tokens.axaml")),
                    (ResourceDictionary)AvaloniaXamlLoader.Load(
                        new Uri("avares://SPRView.Net/Themes/Icons.axaml")),
                    (ResourceDictionary)AvaloniaXamlLoader.Load(
                        new Uri("avares://SPRView.Net/Themes/ControlThemes.axaml")),
                    (ResourceDictionary)AvaloniaXamlLoader.Load(
                        new Uri("avares://SPRView.Net/Themes/SliderTheme.axaml")),
                }
            });
            Styles.Add((IStyle)AvaloniaXamlLoader.Load(
                new Uri("avares://SPRView.Net/Themes/Styles.axaml")));
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (_mode == "check")
            {
                Environment.Exit(InteractionChecks.Run(_sprPath));
                return;
            }
            if (_mode == "both")
                _exitCode = InteractionChecks.Run(_sprPath);

            Render();
            Environment.Exit(_exitCode);
        }
    }

    private static void Render()
    {
        foreach (ThemeVariant variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            Application.Current!.RequestedThemeVariant = variant;
            string suffix = variant == ThemeVariant.Light ? "light" : "dark";
            RenderMainWindow(suffix);
            RenderCreateNew(suffix);
            RenderAbout(suffix);
            RenderMessageBox(suffix);
        }
        Console.WriteLine($"written to {_outDir}");
    }

    private static void Dump(string file, Window window, int width, int height)
    {
        window.Width = width;
        window.Height = height;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        Pump();

        var frame = window.CaptureRenderedFrame();
        if (frame == null)
        {
            Console.WriteLine($"capture failed: {file}");
            window.Close();
            return;
        }

        var size = frame.PixelSize;
        string path = Path.Combine(_outDir, file);
        frame.Save(path, new PngBitmapEncoderOptions());
        frame.Dispose();
        window.Close();
        Console.WriteLine($"ok {file} {size.Width}x{size.Height}");
    }

    /// <summary>Lets layout, bindings and render passes settle.</summary>
    private static void Pump()
    {
        for (int i = 0; i < 6; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        }
    }

    private static void RenderMainWindow(string suffix)
    {
        var window = new MainWindow();
        var vm = new MainWindowViewModel(window) { Lang = LangLoader.Load(_lang) };
        window.DataContext = vm;

        if (_sprPath != null && File.Exists(_sprPath))
        {
            using FileStream stream = File.OpenRead(_sprPath);
            SprDocument sprite = SprDocument.Load(stream);
            vm.LoadSprite(sprite, Path.GetFileName(_sprPath));
        }

        Console.WriteLine($"  diag: HasSprite={vm.HasSprite} SPR={vm.SPR?.PixelSize} " +
                          $"viewer={vm.SprViewerSize} sidebar={vm.CanShowSideBar} frames={vm.MaxFrame}");
        Dump($"main-{suffix}.png", window, 1080, 720);

        // Third pass: transparency off, so the stored pixels show through.
        var rawWindow = new MainWindow();
        var rawVm = new MainWindowViewModel(rawWindow) { Lang = LangLoader.Load(_lang) };
        rawWindow.DataContext = rawVm;
        if (_sprPath != null && File.Exists(_sprPath))
        {
            using FileStream stream = File.OpenRead(_sprPath);
            SprDocument sprite = SprDocument.Load(stream);
            rawVm.LoadSprite(sprite, Path.GetFileName(_sprPath));
            rawVm.ApplyTransparency = false;
        }
        Dump($"main-raw-{suffix}.png", rawWindow, 1080, 720);

        // Second pass: both side panels open.
        var window2 = new MainWindow();
        var vm2 = new MainWindowViewModel(window2) { Lang = LangLoader.Load(_lang) };
        window2.DataContext = vm2;
        vm2.ShowPalettePanel = true;
        if (_sprPath != null && File.Exists(_sprPath))
        {
            using FileStream stream = File.OpenRead(_sprPath);
            SprDocument sprite = SprDocument.Load(stream);
            vm2.LoadSprite(sprite, Path.GetFileName(_sprPath));
        }
        Dump($"main-panels-{suffix}.png", window2, 1080, 720);

        // Empty state: the storage is process wide, so clear it for this pass.
        App.Storage.NowSprite = null;
        App.Storage.FileName = null;
        var window3 = new MainWindow();
        var vm3 = new MainWindowViewModel(window3) { Lang = LangLoader.Load(_lang) };
        window3.DataContext = vm3;
        Dump($"main-empty-{suffix}.png", window3, 1080, 720);
    }

    private static void RenderCreateNew(string suffix)
    {
        var window = new CreateNewWindow();
        var vm = new CreateNewViewModel(window) { Lang = LangLoader.Load(_lang) };
        window.DataContext = vm;

        // Seed the list with the generated frames when available.
        string? sampleDir = Path.GetDirectoryName(_sprPath ?? "");
        if (sampleDir != null)
        {
            var frames = Directory.GetFiles(sampleDir, "frame*.png").OrderBy(f => f).ToArray();
            vm.SetImages(frames);
        }

        Dump($"createnew-{suffix}.png", window, 760, 560);
    }

    private static void RenderAbout(string suffix)
    {
        var window = new AboutWindow();
        window.DataContext = LangLoader.Load(_lang);
        Dump($"about-{suffix}.png", window, 420, 340);
    }

    private static void RenderMessageBox(string suffix)
    {
        var box = MessageBoxWindow.CreateMessageBox(
            "System.IO.EndOfStreamException: Unable to read beyond the end of the stream.\n" +
            "   at SPRView.Net.Core.SprReader.ReadHeader(BinaryReader reader)\n" +
            "   at SPRView.Net.Core.SprDocument.Load(Stream stream)",
            "Something went wrong", "OK");
        Dump($"messagebox-{suffix}.png", box, 420, 260);
    }
}

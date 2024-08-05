using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SPRView.Net.Storage;
using SPRView.Net.ViewModel;
using System.IO;

namespace SPRView.Net;

public partial class App : Application
{
#pragma warning disable CS8618
    private static MainWindow m_pMainWindow;
    public static MainWindow GetMainWindow()
    {
        return m_pMainWindow;
    }
    private static MainWindowViewModel m_pViewModel;
    public static MainWindowViewModel GetViewModel()
    {
        return m_pViewModel;
    }
#pragma warning restore CS8618
    private static readonly CStorage m_pStorage = new();
    public static CStorage GetAppStorage()
    {
        return m_pStorage;
    }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            m_pMainWindow = new MainWindow();
            m_pViewModel = new MainWindowViewModel(m_pMainWindow)
            {
                Lang = new LangViewModel()
            };
            m_pViewModel.LoadLangFile();
            m_pMainWindow.DataContext = m_pViewModel;
            desktop.MainWindow = m_pMainWindow;
            desktop.Startup += OnStartup;
        }

        base.OnFrameworkInitializationCompleted();
    }
    public static void LoadFile(Stream file)
    {
        var newSprite = new Lib.CSprite(file);
        m_pStorage.NowSprite = newSprite;
        using var memoryStream = new MemoryStream();
        Image img = newSprite.GetImage(0);
        img.SaveAsPng(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        m_pViewModel.SPR = new Bitmap(memoryStream);
        m_pViewModel.SprViewerSize = m_pViewModel.SPR.Size;
    }

    private void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        if (e.Args.Length > 0)
        {
            string filePath = e.Args[0];
            if (Path.Exists(filePath))
                LoadFile(File.OpenRead(filePath));
        }
    }
}
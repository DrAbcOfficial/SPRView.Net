using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using SPRView.Net.Core;
using SPRView.Net.Storage;
using SPRView.Net.ViewModel;
using SixLabors.ImageSharp;
using System;
using System.IO;

namespace SPRView.Net;

public partial class App : Application
{
#pragma warning disable CS8618
    private static MainWindow m_pMainWindow;
    public static MainWindow GetMainWindow() => m_pMainWindow;
    private static MainWindowViewModel m_pViewModel;
    public static MainWindowViewModel GetViewModel() => m_pViewModel;
#pragma warning restore CS8618

    private static readonly AppStorage m_pStorage = new();
    public static AppStorage Storage => m_pStorage;

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
        try
        {
            SprDocument newSprite = SprDocument.Load(file);
            Storage.NowSprite = newSprite;
            using var memoryStream = new MemoryStream();
            using var img = newSprite.GetFrameImage(0);
            img.SaveAsPng(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            m_pViewModel.SPR = new Bitmap(memoryStream);
            m_pViewModel.SprViewerSize = m_pViewModel.SPR.Size;
        }
        catch (Exception e)
        {
            MessageBoxWindow messageBoxWindow = MessageBoxWindow.CreateMessageBox(e.ToString());
            messageBoxWindow.ShowDialog(m_pMainWindow);
        }
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

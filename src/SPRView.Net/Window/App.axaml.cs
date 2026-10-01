using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SPRView.Net.Core;
using SPRView.Net.Storage;
using SPRView.Net.ViewModel;
using System;
using System.IO;

namespace SPRView.Net;

public partial class App : Application
{
#pragma warning disable CS8618
    private static MainWindowViewModel m_pViewModel;
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
            var mainWindow = new MainWindow();
            m_pViewModel = new MainWindowViewModel(mainWindow);
            mainWindow.DataContext = m_pViewModel;
            m_pViewModel.LoadLangFile();
            desktop.MainWindow = mainWindow;
            desktop.Startup += OnStartup;
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Opens a sprite stream. Failures surface as a dialog instead of taking the
    /// process down, which matters when the file comes from the shell (open with)
    /// or from a storage provider without a local path.
    /// </summary>
    public static void LoadFile(Stream file, string? fileName = null)
    {
        try
        {
            SprDocument newSprite = SprDocument.Load(file);
            m_pViewModel.LoadSprite(newSprite, fileName);
        }
        catch (Exception e)
        {
            m_pViewModel.ShowError(e);
        }
    }

    private void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        if (e.Args.Length > 0)
        {
            string filePath = e.Args[0];
            if (File.Exists(filePath))
            {
                m_pViewModel.OpenPath(filePath);
            }
        }
    }
}

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SPRView.Net.Storage;
using SPRView.Net.ViewModel;

namespace SPRView.Net;

public partial class App : Application
{
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
    private static CStorage m_pStorage = new();
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

            m_pViewModel = new MainWindowViewModel()
            {
                Lang = new LangViewModel(),
                Command = new CommandViewModel(),
                SprInfo = new SpriteInfoViewModel()
            };
            m_pViewModel.LoadLangFile();
            m_pMainWindow.DataContext = m_pViewModel;
            desktop.MainWindow = m_pMainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SPRView.Net;

public partial class AboutWindow : Window
{
    public AboutWindow() => InitializeComponent();

    private void OnCaptionPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        BeginMoveDrag(e);
        e.Handled = true;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

    private void OnSourceCodeClick(object? sender, RoutedEventArgs e)
        => OpenUri("https://github.com/DrAbcOfficial/SPRView.Net");

    private void OpenUri(string uri)
    {
        try
        {
            var launcher = TopLevel.GetTopLevel(this)?.Launcher;
            launcher?.LaunchUriAsync(new Uri(uri));
        }
        catch (Exception)
        {
            // A failure to open the browser should not close the dialog.
        }
    }
}

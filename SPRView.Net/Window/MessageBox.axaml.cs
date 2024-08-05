using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SPRView.Net;

public partial class MessageBoxWindow : Window
{
    public MessageBoxWindow() => InitializeComponent();
    public static MessageBoxWindow CreateMessageBox(string message, string? title = null, string? ok = null, string? cancel = null)
    {
        var box = new MessageBoxWindow();
        box.FindControl<TextBlock>("Message")!.Text = message;
        if (title != null)
            box.FindControl<TextBlock>("Title")!.Text = title;
        if (ok != null)
            box.FindControl<Button>("OK")!.Content = ok;
        if (cancel != null)
            box.FindControl<Button>("Cancel")!.Content = cancel;
        return box;
    }
    public void Button_Click(object? obj, RoutedEventArgs e)
    {
        Close();
    }
}
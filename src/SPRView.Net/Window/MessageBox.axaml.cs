using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SPRView.Net;

/// <summary>
/// Minimal ContentDialog-alike: a message, an optional title and one or two
/// buttons. Kept as a window because the app targets desktop only.
/// </summary>
public partial class MessageBoxWindow : Window
{
    public MessageBoxWindow() => InitializeComponent();

    /// <summary>
    /// Builds a message box. When <paramref name="cancel"/> is null only the
    /// confirm button is shown, which is what error reports need.
    /// </summary>
    public static MessageBoxWindow CreateMessageBox(
        string message, string? title = null, string? ok = null, string? cancel = null)
    {
        var box = new MessageBoxWindow();
        box.FindControl<TextBlock>("Message")!.Text = message;
        if (title != null)
            box.FindControl<TextBlock>("TitleText")!.Text = title;
        if (ok != null)
            box.FindControl<Button>("OK")!.Content = ok;
        if (cancel == null)
        {
            var cancelButton = box.FindControl<Button>("Cancel")!;
            cancelButton.IsVisible = false;
            var closeButton = box.FindControl<Button>("CloseButton")!;
            closeButton.IsVisible = false;
        }
        else
            box.FindControl<Button>("Cancel")!.Content = cancel;
        return box;
    }

    private void OnCaptionPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        BeginMoveDrag(e);
        e.Handled = true;
    }

    private void Button_Click(object? obj, RoutedEventArgs e) => Close();
}

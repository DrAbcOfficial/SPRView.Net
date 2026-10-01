using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using SPRView.Net.ViewModel;

namespace SPRView.Net;

public partial class CreateNewWindow : Window
{
    public CreateNewWindow()
    {
        InitializeComponent();
        WindowChrome.AddResizeGrips(this);
    }

    private CreateNewViewModel? Vm => DataContext as CreateNewViewModel;

    private void OnCaptionPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed)
            return;

        BeginMoveDrag(e);
        e.Handled = true;
    }

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Vm?.StopPreview();
        Close();
    }

    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Close();

    private void OnAddImageClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.AddImage();

    private void OnRemoveImageClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.RemoveImage();

    private void OnMoveUpClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.MoveupImage();

    private void OnMoveDownClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.MovedownImage();

    private void OnPlayPreviewClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Vm?.StartPreview();

    private async void OnSaveClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await (Vm?.SaveToSpr() ?? Task.CompletedTask);
}

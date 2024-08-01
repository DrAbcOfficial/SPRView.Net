using Avalonia.Controls;

namespace SPRView.Net;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnFrameTextBoxChanged(object? sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox ?? throw new System.Exception("Null text box");
        textBox.Text = App.GetViewModel().NowFrame;
    }
    private void OnSizeTextBoxChanged(object? sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox ?? throw new System.Exception("Null text box");
        textBox.Text = App.GetViewModel().NowScale;
    }
}
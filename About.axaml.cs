using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SPRView.Net;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
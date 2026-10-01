using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SPRView.Net.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SPRView.Net.ViewModel;

/// <summary>
/// New sprite wizard state: sprite properties and shared UI plumbing.
/// </summary>
public partial class CreateNewViewModel : INotifyPropertyChanged
{
    public Window Parent;
    public event PropertyChangedEventHandler? PropertyChanged;

    public CreateNewViewModel(Window parent)
    {
        Parent = parent;
        animation_timer = new();
        animation_timer.Tick += (object? sender, EventArgs e) =>
        {
            int frame = Preview_Frame;
            frame++;
            if (frame >= m_aryImagePaths.Count)
            {
                animation_timer.Stop();
                frame--;
            }
            Preview_Frame = frame;
            OnPropertyChanged(nameof(Preview_Frame));
        };
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public int PathSelected { get; set; } = 0;
    internal readonly List<string> m_aryImagePaths = [];

    #region Property
    public int Type { get; set; } = 0;
    public int Format { get; set; } = 0;
    public int Sync { get; set; } = 0;
    public float BeamLength { get; set; } = 0;
    public bool UnPackAnimate { get; set; } = false;
    public int PlaySpeed { get; set; } = 24;

    private LangViewModel? _lang = null;
    public LangViewModel? Lang
    {
        get => _lang;
        set
        {
            if (_lang != value)
                _lang = value;
        }
    }
    #endregion
}

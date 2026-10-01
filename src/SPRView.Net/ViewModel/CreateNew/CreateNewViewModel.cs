using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.ComponentModel;
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
            int frame = m_Preview_Frame;
            frame++;
            if (frame >= m_aryImagePaths.Count)
            {
                StopPreview();
                frame = Math.Max(0, m_aryImagePaths.Count - 1);
            }
            Preview_Frame = frame;
        };
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>Replaces the whole list; used by the file dialog and by tooling.</summary>
    public void SetImages(IEnumerable<string> paths)
    {
        m_aryImagePaths.Clear();
        m_aryImagePaths.AddRange(paths);
        PathSelected = 0;
        RaiseListChanged();
        if (!ResetPreview())
            PreviewImage = null;
    }

    internal readonly List<string> m_aryImagePaths = [];

    private int m_iPathSelected;
    /// <summary>
    /// Selected row in the frame list. Selecting a row drives the preview, but
    /// not the other way round: the preview slider only moves the preview.
    /// </summary>
    public int PathSelected
    {
        get => m_iPathSelected;
        set
        {
            int index = Math.Clamp(value, 0, Math.Max(0, m_aryImagePaths.Count - 1));
            if (index == m_iPathSelected)
                return;
            m_iPathSelected = index;
            OnPropertyChanged(nameof(PathSelected));
            Preview_Frame = index;
        }
    }

    #region Property
    public int Type { get; set; } = 0;
    public int Format { get; set; } = 0;
    public int Sync { get; set; } = 0;
    public float BeamLength { get; set; } = 0;
    public bool UnPackAnimate { get; set; } = false;
    public int PlaySpeed { get; set; } = 24;

    /// <summary>Number of queued images, shown next to the list header.</summary>
    public string ImageCountLabel => $"{m_aryImagePaths.Count}";

    private LangViewModel? _lang = null;
    public LangViewModel? Lang
    {
        get => _lang;
        set
        {
            if (ReferenceEquals(_lang, value))
                return;
            _lang = value;
            OnPropertyChanged(nameof(Lang));
        }
    }
    #endregion
}

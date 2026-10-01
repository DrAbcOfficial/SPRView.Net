using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System.Linq;

namespace SPRView.Net.ViewModel;

/// <summary>
/// Image list editing and the animated preview built on it.
/// </summary>
public partial class CreateNewViewModel
{
    public string[] ImagePaths => [.. m_aryImagePaths];

    public async void AddImage()
    {
        var files = await Parent.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang?.CreateNew_AddImage_Title,
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypes.ImageAll],
        });
        if (files.Count >= 1)
        {
            foreach (var file in files)
            {
                string? local = file.TryGetLocalPath();
                if (local != null)
                    m_aryImagePaths.Add(local);
            }
            OnPropertyChanged(nameof(ImagePaths));
            OnPropertyChanged(nameof(Preview_MaxFrame));
            OnPropertyChanged(nameof(SaveValid));
        }
        ResetPreview();
    }

    public void RemoveImage()
    {
        if (m_aryImagePaths.Count <= 0)
            return;
        m_aryImagePaths.RemoveAt(PathSelected);
        OnPropertyChanged(nameof(ImagePaths));
        OnPropertyChanged(nameof(Preview_MaxFrame));
        OnPropertyChanged(nameof(SaveValid));
        if (PathSelected + 1 < m_aryImagePaths.Count)
            PathSelected += 1;
        else
            PathSelected = 0;
        OnPropertyChanged(nameof(PathSelected));
        if (!ResetPreview() && PreviewImage != null)
            PreviewImage = null;
    }

    public void MoveupImage()
    {
        if (m_aryImagePaths.Count <= 0)
            return;
        if (PathSelected <= 0)
            return;
        string p = m_aryImagePaths[PathSelected];
        m_aryImagePaths.RemoveAt(PathSelected);
        m_aryImagePaths.Insert(PathSelected - 1, p);
        OnPropertyChanged(nameof(ImagePaths));
        ResetPreview();
    }

    public void MovedownImage()
    {
        if (PathSelected >= m_aryImagePaths.Count - 1)
            return;
        string p = m_aryImagePaths[PathSelected];
        m_aryImagePaths.RemoveAt(PathSelected);
        m_aryImagePaths.Insert(PathSelected + 1, p);
        PathSelected += 1;
        OnPropertyChanged(nameof(ImagePaths));
        OnPropertyChanged(nameof(PathSelected));
        ResetPreview();
    }

    #region Preview
    private readonly Avalonia.Threading.DispatcherTimer animation_timer;

    public void StartPreview()
    {
        if (!animation_timer.IsEnabled)
        {
            animation_timer.Interval = TimeSpan.FromSeconds(1.0f / PlaySpeed);
            animation_timer.Start();
        }
    }

    private bool ResetPreview()
    {
        if (m_aryImagePaths.Count > 0)
        {
            PreviewImage = new Bitmap(m_aryImagePaths.First());
            m_Preview_Frame = 0;
            return true;
        }
        return false;
    }

    private Bitmap? m_previewImage;
    public Bitmap? PreviewImage
    {
        get => m_previewImage;
        set
        {
            m_previewImage?.Dispose();
            m_previewImage = value;
            OnPropertyChanged(nameof(PreviewImage));
        }
    }

    private int m_Preview_Frame = 0;
    public int Preview_Frame
    {
        get => m_Preview_Frame;
        set
        {
            m_Preview_Frame = value;
            if (PreviewImage != null)
                PreviewImage = new Bitmap(m_aryImagePaths[m_Preview_Frame]);
        }
    }

    public int Preview_MaxFrame => m_aryImagePaths.Count - 1;
    #endregion
}

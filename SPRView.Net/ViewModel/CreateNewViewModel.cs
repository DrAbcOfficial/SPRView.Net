using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
namespace SPRView.Net.ViewModel;

public class CreateNewViewModel : INotifyPropertyChanged
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
                animation_timer.Stop();
            Preview_Frame = frame;
            OnPropertyChanged(nameof(Preview_Frame));
        };
    }
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public int PathSelected { get; set; } = 0;
    private List<string> m_aryImagePaths = [];
    public string[] ImagePaths
    {
        get { return m_aryImagePaths.ToArray(); }
    }

    #region Property
    public int PlaySpeed { get; set; } = 24;
    public int Type { get; set; } = 0;
    public int Format { get; set; } = 0;
    public int Sync { get; set; } = 0;

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
        }
        ResetPreview();
    }
    public void RemoveImage()
    {
        m_aryImagePaths.RemoveAt(PathSelected);
        OnPropertyChanged(nameof(ImagePaths));
        OnPropertyChanged(nameof(Preview_MaxFrame));
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
    #endregion

    #region Export
    private DispatcherTimer animation_timer;
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
    public int Preview_MaxFrame
    {
        get
        {
            return m_aryImagePaths.Count - 1;
        }
    }
    public int Export_Width { get; set; } = 64;
    public int Export_Height { get; set; } = 64;
    #endregion

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
}

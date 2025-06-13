using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SPRView.Net.Lib.Class;
using SPRView.Net.Lib.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    private readonly List<string> m_aryImagePaths = [];
    public string[] ImagePaths
    {
        get { return [.. m_aryImagePaths]; }
    }

    #region Property
    public int Type { get; set; } = 0;
    public int Format { get; set; } = 0;
    public int Sync { get; set; } = 0;
    public float BeamLength { get; set; } = 0;
    public bool UnPackAnimate { get; set; } = false;

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
        if (m_aryImagePaths.Count <= 0)
            return;
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
    private readonly DispatcherTimer animation_timer;
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
    public int PlaySpeed { get; set; } = 24;
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
    private int m_iProgress = 0;
    public int Progress { get => m_iProgress; set { m_iProgress = value; OnPropertyChanged(nameof(Progress)); } }
    public bool SaveValid
    {
        get => m_aryImagePaths.Count > 0;
    }
    public async void SaveToSpr()
    {
        if (Export_Width % 2 == 1 || Export_Height % 2 == 1)
        {
            var box = MessageBoxWindow.CreateMessageBox(Lang!.CreateNew_Export_NotSQRTWarning, null, Lang.Shared_OK, Lang.Shared_Cancel);
            box.Position = new Avalonia.PixelPoint(Parent.Position.X + (int)Parent.Width / 2, Parent.Position.Y + (int)Parent.Height / 2);
            await box.ShowDialog(Parent);
        }
        Progress = 0;
        FilePickerFileType Sprites = new("GoldSrc Sprites")
        {
            Patterns = ["*.spr"],
            AppleUniformTypeIdentifiers = ["public.sprite"],
            MimeTypes = ["sprite/*"]
        };
        var files = await Parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang?.FileManager_OpenSprite,
            FileTypeChoices = [Sprites]
        });
        if (files != null)
        {
            Progress = 0;
            await using Stream fs = await files.OpenWriteAsync();
            CSprite.Save([.. m_aryImagePaths], fs, Export_Width, Export_Height,
                (ISprite.SpriteFormat)Format, (ISprite.SpriteType)Type, (ISprite.SpriteSynchron)Sync, BeamLength, UnPackAnimate);
            Progress = 200;
            var box = MessageBoxWindow.CreateMessageBox("☑︎💾", null, Lang!.Shared_OK, Lang.Shared_Cancel);
            box.Position = new Avalonia.PixelPoint(Parent.Position.X + (int)Parent.Width / 2, Parent.Position.Y + (int)Parent.Height / 2);
            await box.ShowDialog(Parent);
        }
    }
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

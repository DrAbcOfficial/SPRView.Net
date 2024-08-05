using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
namespace SPRView.Net.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public Window Parent;
    public MainWindowViewModel(Window window)
    {
        Parent = window;
        animation_timer = new();
        animation_timer.Tick += (object? sender, EventArgs e) =>
        {
            var spr = App.GetAppStorage().NowSprite ?? throw new NullReferenceException("Null storage spr");
            int frame = NowFrame;
            frame++;
            if (frame >= spr.Frames.Count)
            {
                frame = 0;
                if (!IsLoopPlay)
                {
                    animation_timer.Stop();
                    OnPropertyChanged(nameof(IsTimerVliad));
                }
            }
            NowFrame = frame;
        };
    }
    private void LoadLangFileInternal(Stream file)
    {
        using var reader = new StreamReader(file);
        string json = reader.ReadToEnd();
        LangViewModel? person = JsonSerializer.Deserialize<LangViewModel>(json);
        if (person != null)
            Lang = person;
        file.Dispose();
        OnPropertyChanged(nameof(Lang));
    }
    public void LoadLangFile(string lang)
    {
        Stream asset;
        try
        {
            asset = AssetLoader.Open(new Uri($"avares://SPRView.Net/Assets/Lang/{lang}.json"));
        }
        catch (Exception)
        {
            asset = AssetLoader.Open(new Uri($"avares://SPRView.Net/Assets/Lang/en.json"));
        }
        LoadLangFileInternal(asset);
    }
    public void LoadLangFile()
    {
        Stream asset;
        try
        {
            asset = AssetLoader.Open(new Uri($"avares://SPRView.Net/Assets/Lang/{CultureInfo.CurrentCulture.TwoLetterISOLanguageName}.json"));
        }
        catch (Exception)
        {
            asset = AssetLoader.Open(new Uri($"avares://SPRView.Net/Assets/Lang/en.json"));
        }
        LoadLangFileInternal(asset);
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

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

    private Bitmap? m_pSpr;
    public Bitmap? SPR
    {
        get => m_pSpr;
        set
        {
            m_pSpr = value;
            OnPropertyChanged(nameof(SPR));
            OnPropertyChanged(nameof(LoadedSpr));
            OnPropertyChanged(nameof(ColorPallet));
            OnPropertyChanged(nameof(MaxFrame));
            SpriteInfoUpdateAll();
        }
    }

    private Avalonia.Size m_SprViewerSize;
    public Avalonia.Size SprViewerSize
    {
        get => m_SprViewerSize;
        set
        {
            m_SprViewerSize = value;
            OnPropertyChanged(nameof(SprViewerSize));
        }
    }

    public bool LoadedSpr
    {
        get => m_pSpr != null;
    }

    public int m_iNowFrame = 0;
    public int NowFrame
    {
        get => m_iNowFrame;
        set
        {
            m_iNowFrame = value;
            var spr = App.GetAppStorage().NowSprite ?? throw new Exception("Spr is null!");
            m_iNowFrame = Math.Clamp(m_iNowFrame, 0, spr.Frames.Count - 1);
            using var memoryStream = new MemoryStream();
            SixLabors.ImageSharp.Image img = spr.GetImage(m_iNowFrame);
            img.SaveAsPng(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            SPR = new Bitmap(memoryStream);
            OnPropertyChanged(nameof(NowFrame));
            UpdateOriginXY();
        }
    }
    public int MaxFrame
    {
        get
        {
            var spr = App.GetAppStorage().NowSprite;
            return spr == null ? 0 : spr.GetFrames();
        }
    }

    private float m_flNowScale = 1.0f;
    public float NowScale
    {
        get => m_flNowScale;
        set
        {
            var spr = SPR ?? throw new NullReferenceException("SPR is null!");
            m_flNowScale = value;
            SprViewerSize = spr.Size * m_flNowScale;
            OnPropertyChanged(nameof(NowScale));
        }
    }

    private bool m_bIsLoopPlay = false;
    public bool IsLoopPlay
    {
        get => m_bIsLoopPlay;
        set
        {
            m_bIsLoopPlay = value;
            OnPropertyChanged(nameof(IsLoopPlay));
        }
    }

    private bool m_bCanShowInfo = false;
    public bool CanShowInfo
    {
        get => m_bCanShowInfo;
        set
        {
            m_bCanShowInfo = value;
            OnPropertyChanged(nameof(CanShowInfo));
            OnPropertyChanged(nameof(CanShowSideBar));
        }
    }
    private bool m_bCanShowPallet = false;
    public bool CanShowPallet
    {
        get => m_bCanShowPallet;
        set
        {
            m_bCanShowPallet = value;
            OnPropertyChanged(nameof(CanShowPallet));
            OnPropertyChanged(nameof(CanShowSideBar));
            OnPropertyChanged(nameof(ColorPallet));
        }
    }
    public IColorPalette? ColorPallet
    {
        get
        {
            var pal = App.GetAppStorage().NowPalette;
            if (!pal.IsValid())
                return null;
            return pal;
        }
    }
    public bool CanShowSideBar
    {
        get
        {
            return m_bCanShowInfo || m_bCanShowPallet;
        }
    }

    #region SpriteInfo

    public string Frame
    {
        get
        {
            var spr = App.GetAppStorage().NowSprite;
            if (spr == null)
                return "0";
            else
                return spr.Frames.Count.ToString();
        }
    }
    public string Width
    {
        get
        {
            var spr = App.GetAppStorage().NowSprite;
            if (spr == null)
                return "0";
            else
                return spr.MaxFrameWidth.ToString();
        }
    }
    public string Height
    {
        get
        {
            var spr = App.GetAppStorage().NowSprite;
            if (spr == null)
                return "0";
            else
                return spr.MaxFrameHeight.ToString();
        }
    }
    public string Type
    {
        get
        {
            var spr = App.GetAppStorage().NowSprite;
            if (spr == null)
                return "0";
            else
                return spr.Type.ToString();
        }
    }
    public string Format
    {
        get
        {
            var spr = App.GetAppStorage().NowSprite;
            if (spr == null)
                return "0";
            else
                return spr.Format.ToString();
        }
    }
    public string Sync
    {
        get
        {
            var spr = App.GetAppStorage().NowSprite;
            if (spr == null)
                return "0";
            else
                return spr.Synchronization.ToString();
        }
    }
    public string BeamLength
    {
        get
        {
            var spr = App.GetAppStorage().NowSprite;
            if (spr == null)
                return "0";
            else
                return spr.BeamLength.ToString();
        }
    }
    public string BoundRadius
    {
        get
        {
            var spr = App.GetAppStorage().NowSprite;
            if (spr == null)
                return "0";
            else
                return spr.BoundRadius.ToString();
        }
    }
    public string OriginX
    {
        get
        {
            var spr = App.GetAppStorage().NowSprite;
            if (spr == null)
                return "0";
            else
                return spr.Frames[m_iNowFrame].OriginX.ToString();
        }
    }
    public string OriginY
    {
        get
        {
            var spr = App.GetAppStorage().NowSprite;
            if (spr == null)
                return "0";
            else
                return spr.Frames[m_iNowFrame].OriginY.ToString();
        }
    }

    public void UpdateOriginXY()
    {
        OnPropertyChanged(nameof(OriginX));
        OnPropertyChanged(nameof(OriginY));
    }
    public void SpriteInfoUpdateAll()
    {
        OnPropertyChanged(nameof(Frame));
        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));
        OnPropertyChanged(nameof(Type));
        OnPropertyChanged(nameof(Format));
        OnPropertyChanged(nameof(Sync));
        OnPropertyChanged(nameof(BeamLength));
        OnPropertyChanged(nameof(BoundRadius));
    }
    #endregion

    #region Command
    private DispatcherTimer animation_timer;
    public bool IsTimerVliad
    {
        get => animation_timer.IsEnabled;
    }

    public async void CreateFile()
    {
        var createNew = new CreateNewWindow();
        var createNewData = new CreateNewViewModel(createNew)
        {
            Lang = Lang
        };
        createNew.DataContext = createNewData;
        await createNew.ShowDialog(Parent);
    }
    public async void OpenFile()
    {
        FilePickerFileType Sprites = new("GoldSrc Sprites")
        {
            Patterns = ["*.spr"],
            AppleUniformTypeIdentifiers = ["public.sprite"],
            MimeTypes = ["sprite/*"]
        };
        var files = await Parent.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang?.FileManager_OpenSprite,
            AllowMultiple = false,
            FileTypeFilter = [Sprites]
        });
        if (files.Count >= 1)
        {
            await using Stream file = await files[0].OpenReadAsync();
            App.LoadFile(file);
        }
    }
    public async void SaveFrame()
    {
        var bitmap = SPR ?? throw new ArgumentNullException("SPR is null!");
        var file = await Parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang?.FileManager_SaveImage,
            DefaultExtension = "bmp",
            FileTypeChoices = [FilePickerFileTypes.ImageAll],
            ShowOverwritePrompt = true
        });
        if (file == null)
            return;
        await using var stream = await file.OpenWriteAsync();
        bitmap.Save(stream);
    }
    public async void SaveGIF()
    {
        var sprite = App.GetAppStorage().NowSprite ?? throw new ArgumentNullException("Storage sprite is null!");
        FilePickerFileType gifstype = new("Animate Image")
        {
            Patterns = ["*.gif", "*.webp"],
            AppleUniformTypeIdentifiers = ["public.gif", "public.webp"],
            MimeTypes = ["image/*"]
        };
        var file = await Parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang?.FileManager_SaveGIF,
            DefaultExtension = "gif",
            FileTypeChoices = [gifstype],
            ShowOverwritePrompt = true
        });
        if (file == null)
            return;
        using var gif = new Image<Rgba32>((int)sprite.MaxFrameWidth, (int)sprite.MaxFrameHeight);
        foreach (var frame in sprite.Frames)
        {
            var img = frame.GetImage();
            gif.Frames.AddFrame(img.Frames[0]);
        }

        await using var stream = await file.OpenWriteAsync();
        if (Path.GetExtension(file.TryGetLocalPath())?.ToLower() == ".webp")
        {
            WebpMetadata metadata = gif.Metadata.GetWebpMetadata();
            metadata.RepeatCount = 0;
            gif.Save(stream, new WebpEncoder());
        }
        else
        {
            GifMetadata gifMetadata = gif.Metadata.GetGifMetadata();
            gifMetadata.RepeatCount = 0;
            gif.Save(stream, new GifEncoder());
        }
    }
    public async void Export()
    {
        var sprite = App.GetAppStorage().NowSprite ?? throw new ArgumentNullException("Storage sprite is null!");
        var directories = await Parent.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Lang?.FileManager_SaveSequence,
            AllowMultiple = false
        });
        if (directories.Count > 0)
        {
            string sequence = "";
            string? directory = directories[0].TryGetLocalPath() ?? throw new DirectoryNotFoundException("Can not found directory");
            for (int i = 0; i < sprite.Frames.Count; i++)
            {
                using StreamWriter sw = new(Path.Combine(directory, $"{i}.bmp"));
                var img = sprite.Frames[i].GetImage();
                img.SaveAsBmp(sw.BaseStream);
                sequence += $"./{i}.bmp\n";
            }
            using StreamWriter text = new(Path.Combine(directory, $"sequence.qc"));
            text.Write(sequence);
        }
    }
    public async void SavePalette()
    {
        FilePickerFileType types = new("Palette files")
        {
            Patterns = ["*.pal", "*.gpl"],
            AppleUniformTypeIdentifiers = ["microsoft.pal", "gimp.gpl"],
            MimeTypes = ["palette/*"]
        };
        var file = await Parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang?.FileManager_SaveGIF,
            DefaultExtension = "pal",
            FileTypeChoices = [types],
            ShowOverwritePrompt = true
        });
        if (file == null)
            return;
        var palette = App.GetAppStorage().NowPalette ?? throw new ArgumentNullException("Storage palette is null!");
        var orgpalette = palette.GetOrigin() ?? throw new ArgumentNullException("Storage original palette is null!");
        await using var fs = await file.OpenWriteAsync();
        string? ext = Path.GetExtension(file.TryGetLocalPath())?.ToLower();
        string? name = Path.GetFileName(file.TryGetLocalPath());
        if (ext == null || name == null)
            return;
        switch (ext)
        {
            case ".pal":
                {
                    using BinaryWriter bw = new(fs);
                    //0x00 Magic
                    bw.Write((uint)0X52494646);
                    //0x04 FileSize
                    bw.Write((uint)0x0);
                    //0x08 RIFF
                    bw.Write((uint)0X50414C20);
                    //0x0B Block Header
                    bw.Write((uint)0X64617461);
                    //0x10 Block Size
                    bw.Write((uint)0x0);
                    //0x14 Palette Size
                    bw.Write((ushort)orgpalette.Length);
                    //0x16 Palette Version
                    bw.Write((ushort)0X300);
                    //0x18 Data
                    for (int i = 0; i < orgpalette.Length; i++)
                    {
                        bw.Write(orgpalette.AtIndex(i).R);
                        bw.Write(orgpalette.AtIndex(i).G);
                        bw.Write(orgpalette.AtIndex(i).B);
                        bw.Write((byte)0x00);
                    }
                    long fileSize = bw.BaseStream.Length;
                    bw.Seek(0x04, SeekOrigin.Begin);
                    bw.Write((uint)fileSize - 8);
                    bw.Seek(0x10, SeekOrigin.Begin);
                    bw.Write((uint)fileSize - 20);
                    break;
                }
            case ".gpl":
                {
                    using StreamWriter sw = new(fs);
                    sw.WriteLine("GIMP Palette");
                    sw.WriteLine($"Name: {name}");
                    sw.WriteLine("Columns: 16");
                    for (int i = 0; i < orgpalette.Length; i++)
                    {
                        Rgba32 rgba = orgpalette.AtIndex(i);
                        sw.WriteLine($"{rgba.R} {rgba.G} {rgba.B} Index {i}");
                    }
                    break;
                }
        }
    }
    public void Exit()
    {
        Parent.Close();
    }
    public async void About()
    {
        var about = new AboutWindow();
        await about.ShowDialog(Parent);
    }

    public void ToggleAnimationTimer()
    {
        if (!animation_timer.IsEnabled)
        {
            int timer_span = App.GetAppStorage().PlaySpeed;
            animation_timer.Interval = TimeSpan.FromSeconds(1.0f / timer_span);
            animation_timer.Start();
        }
        else
            animation_timer.Stop();
        OnPropertyChanged(nameof(IsTimerVliad));
    }

    public void ChangeLang(string lang)
    {
        LoadLangFile(lang);
    }
    #endregion
}

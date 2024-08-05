using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SixLabors.ImageSharp;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
namespace SPRView.Net.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
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

    private CommandViewModel? _cmd = null;
    public CommandViewModel? Command
    {
        get => _cmd;
        set
        {
            if (_cmd != value)
                _cmd = value;
        }
    }

    private SpriteInfoViewModel? _sprinfo = null;
    public SpriteInfoViewModel? SprInfo
    {
        get => _sprinfo;
        set
        {
            if (_sprinfo != value)
                _sprinfo = value;
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
            SprInfo?.UpdateAll();
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
            SprInfo?.UpdateOriginXY();
        }
    }
    public int MaxFrame
    {
        get
        {
            var spr = App.GetAppStorage().NowSprite ?? throw new NullReferenceException("Null storage spr");
            return spr.Frames.Count;
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
}

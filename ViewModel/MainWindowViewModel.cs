using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
namespace SPRView.Net.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public void LoadLangFile()
    {
        using var asset = AssetLoader.Open(new Uri($"avares://SPRView.Net/Assets/Lang/{CultureInfo.CurrentCulture.TwoLetterISOLanguageName}.json"));
        using var reader = new StreamReader(asset);
        string json = reader.ReadToEnd();
        LangViewModel? person = JsonSerializer.Deserialize<LangViewModel>(json);
        if (person != null)
            Lang = person;

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
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(ColorPallet));
            SprInfo?.UpdateAll();
        }
    }

    private Size m_SprViewerSize;
    public Size SprViewerSize
    {
        get => m_SprViewerSize;
        set
        {
            m_SprViewerSize = value;
            OnPropertyChanged(nameof(SprViewerSize));
        }
    }

    public bool CanSave
    {
        get => m_pSpr != null;
    }

    public int m_iNowFrame = 0;
    public string NowFrame
    {
        get => m_iNowFrame.ToString();
        set
        {
            m_iNowFrame = int.TryParse(value, out int result) ? result : 0;
            var spr = App.GetAppStorage().NowSprite ?? throw new System.Exception("Spr is null!");
            m_iNowFrame = Math.Clamp(m_iNowFrame, 0, spr.Frames.Count - 1);
            SPR = spr.Frames[m_iNowFrame].GetBitmap();
            OnPropertyChanged(nameof(NowFrame));
            SprInfo?.UpdateOriginXY();
        }
    }

    private float m_flNowScale = 1.0f;
    public string NowScale
    {
        get => ((int)(m_flNowScale * 100)).ToString();
        set
        {
            var spr = SPR ?? throw new NullReferenceException("SPR is null!");
            m_flNowScale = Math.Clamp(float.TryParse(value, out float result) ? result / 100.0f : 1.0f, 0.1f, 5.0f);
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
            var spr = App.GetAppStorage().NowSprite;
            if (spr == null)
                return null;
            return spr.Pallete;
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

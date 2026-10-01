using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SPRView.Net.Core;
using SPRView.Net.Storage;
using SixLabors.ImageSharp;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Image = SixLabors.ImageSharp.Image;
using Size = Avalonia.Size;

namespace SPRView.Net.ViewModel;

/// <summary>
/// Viewer state: the displayed frame bitmap, zoom and playback.
/// </summary>
public partial class MainWindowViewModel : INotifyPropertyChanged
{
    public Window Parent;

    public MainWindowViewModel(Window window)
    {
        Parent = window;
        animation_timer = new();
        animation_timer.Tick += (object? sender, EventArgs e) =>
        {
            var spr = App.Storage.NowSprite ?? throw new NullReferenceException("Null storage spr");
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    public bool LoadedSpr => m_pSpr != null;

    private int m_iNowFrame = 0;
    public int NowFrame
    {
        get => m_iNowFrame;
        set
        {
            var spr = App.Storage.NowSprite ?? throw new Exception("Spr is null!");
            m_iNowFrame = Math.Clamp(value, 0, spr.Frames.Count - 1);
            using var memoryStream = new MemoryStream();
            using Image img = spr.GetFrameImage(m_iNowFrame);
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
            var spr = App.Storage.NowSprite;
            return spr == null ? 0 : spr.GetFrameCount();
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

    public Avalonia.Controls.IColorPalette? ColorPallet
    {
        get
        {
            var pal = App.Storage.NowPalette;
            if (!pal.IsValid())
                return null;
            return pal;
        }
    }

    public bool CanShowSideBar => m_bCanShowInfo || m_bCanShowPallet;

    private readonly DispatcherTimer animation_timer;
    public bool IsTimerVliad => animation_timer.IsEnabled;

    public void ToggleAnimationTimer()
    {
        if (!animation_timer.IsEnabled)
        {
            int timer_span = App.Storage.PlaySpeed;
            animation_timer.Interval = TimeSpan.FromSeconds(1.0f / timer_span);
            animation_timer.Start();
        }
        else
            animation_timer.Stop();
        OnPropertyChanged(nameof(IsTimerVliad));
    }
}

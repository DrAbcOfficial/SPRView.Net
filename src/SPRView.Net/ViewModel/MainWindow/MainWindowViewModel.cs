using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
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
            var spr = App.Storage.NowSprite;
            if (spr == null || spr.Frames.Count == 0)
            {
                animation_timer.Stop();
                OnPropertyChanged(nameof(IsPlaying));
                return;
            }

            int frame = m_iNowFrame + 1;
            if (frame >= spr.Frames.Count)
            {
                frame = 0;
                if (!IsLoopPlay)
                {
                    animation_timer.Stop();
                    OnPropertyChanged(nameof(IsPlaying));
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

    /// <summary>Loads a document into the shared storage and shows its first frame.</summary>
    public void LoadSprite(SprDocument sprite, string? fileName)
    {
        App.Storage.NowSprite = sprite;
        App.Storage.FileName = fileName;

        StopPlayback();
        m_iNowFrame = 0;
        SPR = RenderFrame(0);
        // Scale is applied after the bitmap: both the zoom readout and the
        // fit calculation need the frame size.
        _autoFit = true;
        NowScale = 1.0f;

        OnPropertyChanged(nameof(CaptionPrimary));
        OnPropertyChanged(nameof(CaptionSecondary));
        OnPropertyChanged(nameof(HasFile));
        OnPropertyChanged(nameof(CanShowSideBar));
        OnPropertyChanged(nameof(NowFrame));
        OnPropertyChanged(nameof(FrameLabel));
        UpdateOriginXY();
    }

    private Bitmap? m_pSpr;
    public Bitmap? SPR
    {
        get => m_pSpr;
        set
        {
            m_pSpr = value;
            OnPropertyChanged(nameof(SPR));
            OnPropertyChanged(nameof(HasSprite));
            OnPropertyChanged(nameof(LoadedSpr));
            OnPropertyChanged(nameof(PaletteSwatches));
            OnPropertyChanged(nameof(PaletteCount));
            OnPropertyChanged(nameof(PaletteCountLabel));
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

    /// <summary>True when a sprite is loaded; drives the viewport / empty state switch.</summary>
    public bool HasSprite => m_pSpr != null;

    /// <summary>Kept for the enable state of file commands.</summary>
    public bool LoadedSpr => HasSprite;

    private int m_iNowFrame = 0;
    public int NowFrame
    {
        get => m_iNowFrame;
        set
        {
            var spr = App.Storage.NowSprite;
            if (spr == null || spr.Frames.Count == 0)
                return;

            int frame = Math.Clamp(value, 0, spr.Frames.Count - 1);
            if (frame == m_iNowFrame && m_pSpr != null)
                return;

            m_iNowFrame = frame;
            SPR = RenderFrame(frame);
            ApplyScale();
            OnPropertyChanged(nameof(NowFrame));
            OnPropertyChanged(nameof(FramePosition));
            OnPropertyChanged(nameof(FrameLabel));
            UpdateOriginXY();
        }
    }

    /// <summary>Frame index as a double so a Slider can bind to it directly.</summary>
    public double FramePosition
    {
        get => m_iNowFrame;
        set => NowFrame = (int)Math.Round(value);
    }

    /// <summary>Human readable "current / total" frame counter.</summary>
    public string FrameLabel => $"{m_iNowFrame + 1} / {Math.Max(1, MaxFrame + 1)}";

    private Bitmap RenderFrame(int index)
    {
        var spr = App.Storage.NowSprite ?? throw new NullReferenceException("Null storage spr");
        using var memoryStream = new MemoryStream();
        using Image img = spr.GetFrameImage(index);
        img.SaveAsPng(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return new Bitmap(memoryStream);
    }

    public int MaxFrame
    {
        get
        {
            var spr = App.Storage.NowSprite;
            return spr == null ? 0 : Math.Max(0, spr.GetFrameCount() - 1);
        }
    }

    private float m_flNowScale = 1.0f;
    public float NowScale
    {
        get => m_flNowScale;
        set
        {
            m_flNowScale = Math.Clamp(value, MinScale, MaxScale);
            ApplyScale();
            OnPropertyChanged(nameof(NowScale));
            OnPropertyChanged(nameof(ZoomPercent));
            OnPropertyChanged(nameof(ZoomLabel));
        }
    }

    /// <summary>Pushes the current zoom onto the rendered bitmap size.</summary>
    private void ApplyScale()
    {
        if (m_pSpr == null)
            return;
        SprViewerSize = m_pSpr.Size * m_flNowScale;
    }

    /// <summary>True while zoom follows the viewport instead of the user.</summary>
    private bool _autoFit = true;

    /// <summary>Largest whole-number zoom used when fitting; beyond this the
    /// sprite would read as a few giant blocks.</summary>
    private const int MaxFitZoom = 8;

    /// <summary>
    /// Fits the sprite to the canvas on whole-number steps, which keeps pixel
    /// art sharp. Runs again whenever the canvas or the sprite changes.
    /// </summary>
    private void ApplyAutoFit()
    {
        if (!_autoFit || m_pSpr == null)
            return;
        var viewport = ViewportSize;
        if (viewport.Width <= 1 || viewport.Height <= 1)
            return;

        double raw = FitScale(viewport, m_pSpr.PixelSize, padding: 32);
        // Upscaling snaps to whole steps; shrinking stays continuous because a
        // sprite larger than the window has to shrink by an arbitrary amount.
        double scale = raw >= 1 ? Math.Floor(Math.Min(raw, MaxFitZoom)) : raw;
        NowScale = (float)Math.Clamp(scale, MinScale, MaxScale);
    }

    private static double FitScale(Size viewport, PixelSize content, double padding)
    {
        double width = Math.Max(1, viewport.Width - padding);
        double height = Math.Max(1, viewport.Height - padding);
        return Math.Min(width / content.Width, height / content.Height);
    }

    /// <summary>Called by the view when the canvas gets its real size.</summary>
    public void OnViewportSizeChanged(Size size)
    {
        ViewportSize = size;
        ApplyAutoFit();
    }

    public const float MinScale = 0.25f;
    public const float MaxScale = 8.0f;

    /// <summary>Zoom in percent, for the zoom readout and the zoom slider.</summary>
    public double ZoomPercent
    {
        get => Math.Round(m_flNowScale * 100);
        set => NowScale = (float)(value / 100.0);
    }

    public string ZoomLabel => $"{ZoomPercent:0}%";

    /// <summary>Canvas size, tracked so "fit to window" can compute a scale.</summary>
    public Size ViewportSize { get; private set; } = new(1, 1);

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

    private bool m_bShowInfoPanel = true;
    public bool ShowInfoPanel
    {
        get => m_bShowInfoPanel;
        set
        {
            m_bShowInfoPanel = value;
            OnPropertyChanged(nameof(ShowInfoPanel));
            OnPropertyChanged(nameof(CanShowSideBar));
        }
    }

    private bool m_bShowPalettePanel = false;
    public bool ShowPalettePanel
    {
        get => m_bShowPalettePanel;
        set
        {
            m_bShowPalettePanel = value;
            OnPropertyChanged(nameof(ShowPalettePanel));
            OnPropertyChanged(nameof(CanShowSideBar));
        }
    }

    public bool CanShowSideBar => (m_bShowInfoPanel || m_bShowPalettePanel) && HasSprite;

    /// <summary>
    /// Palette of the loaded sprite as brushes, laid out 16 per row like the
    /// original GoldSrc palette.
    /// </summary>
    public IReadOnlyList<IBrush> PaletteSwatches
    {
        get
        {
            var palette = App.Storage.NowPalette;
            var brushes = new IBrush[palette.Count];
            for (int i = 0; i < palette.Count; i++)
                brushes[i] = new SolidColorBrush(palette.Colors[i]);
            return brushes;
        }
    }

    public int PaletteCount => App.Storage.NowPalette.Count;

    /// <summary>Colour count as shown next to the palette grid title.</summary>
    public string PaletteCountLabel =>
        string.Format(Lang?.Palette_Colors ?? "{0} colors", PaletteCount);

    private readonly DispatcherTimer animation_timer;

    /// <summary>True while the animation timer runs; drives the play/pause glyph.</summary>
    public bool IsPlaying => animation_timer.IsEnabled;

    /// <summary>Window caption: the file name, or the app name when nothing is loaded.</summary>
    public string CaptionPrimary => App.Storage.FileName ?? "SPRView.Net";

    /// <summary>Window caption suffix; empty when nothing is loaded.</summary>
    public string CaptionSecondary => HasFile ? "SPRView.Net" : string.Empty;

    public bool HasFile => App.Storage.FileName != null;

    public void ToggleAnimationTimer()
    {
        if (animation_timer.IsEnabled)
            StopPlayback();
        else
        {
            int timer_span = Math.Max(1, App.Storage.PlaySpeed);
            animation_timer.Interval = TimeSpan.FromSeconds(1.0f / timer_span);
            animation_timer.Start();
        }
        OnPropertyChanged(nameof(IsPlaying));
    }

    public void StopPlayback()
    {
        animation_timer.Stop();
        OnPropertyChanged(nameof(IsPlaying));
    }
}

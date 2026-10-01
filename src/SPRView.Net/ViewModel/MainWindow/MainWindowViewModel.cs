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
        OnPropertyChanged(nameof(SectionTogglesEnabled));
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
        using Image img = spr.GetFrameImage(index, ApplyTransparency);
        img.SaveAsPng(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return new Bitmap(memoryStream);
    }

    private bool m_bShowCommandLabels;

    /// <summary>
    /// Whether the command bar spells out its labels.
    ///
    /// The window is meant to be usable when narrow, so below the width the
    /// labelled bar needs the buttons collapse to their icons, as they do in
    /// WinUI. The tooltips stay either way.
    /// </summary>
    public bool ShowCommandLabels
    {
        get => m_bShowCommandLabels;
        set
        {
            if (m_bShowCommandLabels == value)
                return;
            m_bShowCommandLabels = value;
            OnPropertyChanged(nameof(ShowCommandLabels));
        }
    }

    /// <summary>
    /// Width under which the command bar drops its labels, in DIPs.
    ///
    /// Measured rather than guessed: with labels the button row wants 550px and
    /// the view group plus margins take another 237, so anything below 790
    /// clips. The default window sits well under it and starts on icons.
    /// </summary>
    public const double CommandLabelsMinWidth = 790;

    private bool m_bApplyTransparency = true;

    /// <summary>
    /// Whether the sprite format's transparency rule is applied.
    ///
    /// Off shows the frames exactly as the file stores them, background and all,
    /// which is what you want when checking what an artist actually painted
    /// underneath the artwork.
    /// </summary>
    public bool ApplyTransparency
    {
        get => m_bApplyTransparency;
        set
        {
            if (m_bApplyTransparency == value)
                return;
            m_bApplyTransparency = value;
            OnPropertyChanged(nameof(ApplyTransparency));

            // The current frame has to be decoded again under the new rule.
            if (m_pSpr != null)
                SPR = RenderFrame(m_iNowFrame);
        }
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

    private bool m_bShowSidePanel;

    /// <summary>
    /// Whether the side panel is on screen at all.
    ///
    /// Starts collapsed: the sprite is what this viewer is for, so metadata and
    /// the palette only take space once they are asked for. The section toggles
    /// choose what goes inside the panel; this decides whether there is a panel.
    /// </summary>
    public bool ShowSidePanel
    {
        get => m_bShowSidePanel;
        set
        {
            if (m_bShowSidePanel == value)
                return;
            m_bShowSidePanel = value;
            OnPropertyChanged(nameof(ShowSidePanel));
            OnPropertyChanged(nameof(CanShowSideBar));
            OnPropertyChanged(nameof(SectionTogglesEnabled));
            OnPropertyChanged(nameof(ShowInfoDivider));
        }
    }

    /// <summary>
    /// The section toggles only have a visible effect while the panel is open,
    /// so they grey out with it instead of silently doing nothing.
    /// </summary>
    public bool SectionTogglesEnabled => m_bShowSidePanel && HasSprite;

    private bool m_bShowInfoPanel = true;
    public bool ShowInfoPanel
    {
        get => m_bShowInfoPanel;
        set
        {
            m_bShowInfoPanel = value;
            OnPropertyChanged(nameof(ShowInfoPanel));
            OnPropertyChanged(nameof(CanShowSideBar));
            OnPropertyChanged(nameof(ShowInfoDivider));
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
            OnPropertyChanged(nameof(ShowInfoDivider));
        }
    }

    private bool m_bIsInfoExpanded = true;

    /// <summary>
    /// Whether the information rows are expanded inside the side panel. The
    /// command bar controls whether the panel exists at all; this controls the
    /// section within it, so a long palette can take the whole panel.
    /// </summary>
    public bool IsInfoExpanded
    {
        get => m_bIsInfoExpanded;
        set
        {
            if (m_bIsInfoExpanded == value)
                return;
            m_bIsInfoExpanded = value;
            OnPropertyChanged(nameof(IsInfoExpanded));
            OnPropertyChanged(nameof(ShowInfoDivider));
        }
    }

    private bool m_bIsPaletteExpanded = true;

    /// <summary>Whether the palette swatches are expanded inside the side panel.</summary>
    public bool IsPaletteExpanded
    {
        get => m_bIsPaletteExpanded;
        set
        {
            if (m_bIsPaletteExpanded == value)
                return;
            m_bIsPaletteExpanded = value;
            OnPropertyChanged(nameof(IsPaletteExpanded));
        }
    }

    /// <summary>The rule between the two sections only earns its space when both are open.</summary>
    public bool ShowInfoDivider =>
        m_bShowSidePanel && m_bShowInfoPanel && m_bShowPalettePanel && m_bIsInfoExpanded;

    public bool CanShowSideBar =>
        m_bShowSidePanel && (m_bShowInfoPanel || m_bShowPalettePanel) && HasSprite;

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

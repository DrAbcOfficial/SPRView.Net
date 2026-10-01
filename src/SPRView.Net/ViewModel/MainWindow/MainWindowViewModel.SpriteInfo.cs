using SPRView.Net.Storage;

namespace SPRView.Net.ViewModel;

/// <summary>
/// Read only sprite metadata shown in the information side bar.
/// </summary>
public partial class MainWindowViewModel
{
    #region SpriteInfo
    public string Frame
    {
        get
        {
            var spr = App.Storage.NowSprite;
            return spr == null ? "0" : spr.Frames.Count.ToString();
        }
    }
    public string Width
    {
        get
        {
            var spr = App.Storage.NowSprite;
            return spr == null ? "0" : spr.Header.MaxFrameWidth.ToString();
        }
    }
    public string Height
    {
        get
        {
            var spr = App.Storage.NowSprite;
            return spr == null ? "0" : spr.Header.MaxFrameHeight.ToString();
        }
    }
    public string Type
    {
        get
        {
            var spr = App.Storage.NowSprite;
            return spr == null ? "0" : spr.Header.Type.ToString();
        }
    }
    public string Format
    {
        get
        {
            var spr = App.Storage.NowSprite;
            return spr == null ? "0" : spr.Header.Format.ToString();
        }
    }
    public string Sync
    {
        get
        {
            var spr = App.Storage.NowSprite;
            return spr == null ? "0" : spr.Header.Synchronization.ToString();
        }
    }
    public string BeamLength
    {
        get
        {
            var spr = App.Storage.NowSprite;
            return spr == null ? "0" : spr.Header.BeamLength.ToString();
        }
    }
    public string BoundRadius
    {
        get
        {
            var spr = App.Storage.NowSprite;
            return spr == null ? "0" : spr.Header.BoundRadius.ToString();
        }
    }
    public string OriginX
    {
        get
        {
            var spr = App.Storage.NowSprite;
            if (spr == null)
                return "0";
            return spr.Frames[m_iNowFrame].OriginX.ToString();
        }
    }
    public string OriginY
    {
        get
        {
            var spr = App.Storage.NowSprite;
            if (spr == null)
                return "0";
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
}

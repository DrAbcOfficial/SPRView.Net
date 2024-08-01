using System.ComponentModel;
namespace SPRView.Net.ViewModel;

public class SpriteInfoViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

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

    public void UpdateAll()
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
}

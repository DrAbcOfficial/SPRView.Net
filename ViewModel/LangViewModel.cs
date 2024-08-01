using System.ComponentModel;
namespace SPRView.Net.ViewModel;


public class LangViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public ObservableProperty TaskBar_File { get; }
    public ObservableProperty TaskBar_File_Open { get; }
    public ObservableProperty TaskBar_File_SaveFrame { get; }
    public ObservableProperty TaskBar_File_SaveGIF { get; }
    public ObservableProperty TaskBar_File_Export { get; }
    public ObservableProperty TaskBar_File_Exit { get; }

    public ObservableProperty TaskBar_View { get; }
    public ObservableProperty TaskBar_View_Information { get; }
    public ObservableProperty TaskBar_View_Pallet { get; }

    public ObservableProperty TaskBar_Help { get; }
    public ObservableProperty TaskBar_Help_About { get; }

    public ObservableProperty Dock_Frame { get; }

    public ObservableProperty SpriteInfo_Frames { get; }
    public ObservableProperty SpriteInfo_Width { get; }
    public ObservableProperty SpriteInfo_Height { get; }
    public ObservableProperty SpriteInfo_Type { get; }
    public ObservableProperty SpriteInfo_Format { get; }
    public ObservableProperty SpriteInfo_Sync { get; }
    public ObservableProperty SpriteInfo_BoundRadius { get; }
    public ObservableProperty SpriteInfo_BeamLength { get; }
    public LangViewModel()
    {
        TaskBar_File = new ObservableProperty(nameof(TaskBar_File), "File");
        TaskBar_File_Open = new ObservableProperty(nameof(TaskBar_File_Open), "Open");
        TaskBar_File_SaveFrame = new ObservableProperty(nameof(TaskBar_File_SaveFrame), "Save Frame");
        TaskBar_File_SaveGIF = new ObservableProperty(nameof(TaskBar_File_SaveGIF), "Save GIF");
        TaskBar_File_Export = new ObservableProperty(nameof(TaskBar_File_Export), "Export");
        TaskBar_File_Exit = new ObservableProperty(nameof(TaskBar_File_Exit), "Exit");
        TaskBar_View = new ObservableProperty(nameof(TaskBar_View), "View");
        TaskBar_View_Information = new ObservableProperty(nameof(TaskBar_View_Information), "Sprite Information");
        TaskBar_View_Pallet = new ObservableProperty(nameof(TaskBar_View_Pallet), "Sprite Pallet");
        TaskBar_Help = new ObservableProperty(nameof(TaskBar_Help), "Help");
        TaskBar_Help_About = new ObservableProperty(nameof(TaskBar_Help_About), "About");

        Dock_Frame = new ObservableProperty(nameof(Dock_Frame), "Frame: ");

        SpriteInfo_Frames = new ObservableProperty(nameof(SpriteInfo_Frames), "Frames: ");
        SpriteInfo_Width = new ObservableProperty(nameof(SpriteInfo_Width), "Width: ");
        SpriteInfo_Height = new ObservableProperty(nameof(SpriteInfo_Height), "Height: ");
        SpriteInfo_Type = new ObservableProperty(nameof(SpriteInfo_Type), "Type: ");
        SpriteInfo_Format = new ObservableProperty(nameof(SpriteInfo_Format), "Format: ");
        SpriteInfo_Sync = new ObservableProperty(nameof(SpriteInfo_Sync), "Sync: ");
        SpriteInfo_BoundRadius = new ObservableProperty(nameof(SpriteInfo_BoundRadius), "BoundRadius: ");
        SpriteInfo_BeamLength = new ObservableProperty(nameof(SpriteInfo_BeamLength), "BeamLength: ");
    }
}

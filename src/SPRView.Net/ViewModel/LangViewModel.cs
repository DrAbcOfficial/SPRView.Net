using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SPRView.Net.ViewModel;

/// <summary>
/// Localized strings. Defaults are the English texts and act as fallback for
/// missing keys in the loaded language file.
/// </summary>
/// <remarks>
/// Backed by C# 14 <c>field</c> keyword properties; the JSON source generator
/// (<see cref="LangJsonContext"/>) deserializes into this class under AOT.
/// </remarks>
public class LangViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public string TaskBar_File { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_File)); } } = "File";
    public string TaskBar_File_Create { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_File_Create)); } } = "Create";
    public string TaskBar_File_Open { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_File_Open)); } } = "Open";
    public string TaskBar_File_SaveFrame { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_File_SaveFrame)); } } = "Save Frame";
    public string TaskBar_File_SaveGIF { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_File_SaveGIF)); } } = "Save GIF";
    public string TaskBar_File_SavePalette { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_File_SavePalette)); } } = "Save Palette";
    public string TaskBar_File_Export { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_File_Export)); } } = "Export";

    public string TaskBar_View { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_View)); } } = "View";
    public string TaskBar_View_Information { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_View_Information)); } } = "Information";
    public string TaskBar_View_Pallet { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_View_Pallet)); } } = "Pallet";
    public string TaskBar_View_Language { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_View_Language)); } } = "Language";
    public string Palette_Colors { get => field; set { field = value; OnPropertyChanged(nameof(Palette_Colors)); } } = "{0} colors";

    public string TaskBar_Help { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_Help)); } } = "Help";
    public string TaskBar_Help_About { get => field; set { field = value; OnPropertyChanged(nameof(TaskBar_Help_About)); } } = "About";


    public string SpriteInfo_Frames { get => field; set { field = value; OnPropertyChanged(nameof(SpriteInfo_Frames)); } } = "Frames:";
    public string SpriteInfo_Width { get => field; set { field = value; OnPropertyChanged(nameof(SpriteInfo_Width)); } } = "Width:";
    public string SpriteInfo_Height { get => field; set { field = value; OnPropertyChanged(nameof(SpriteInfo_Height)); } } = "Height:";
    public string SpriteInfo_Type { get => field; set { field = value; OnPropertyChanged(nameof(SpriteInfo_Type)); } } = "Type:";
    public string SpriteInfo_Format { get => field; set { field = value; OnPropertyChanged(nameof(SpriteInfo_Format)); } } = "Format:";
    public string SpriteInfo_Sync { get => field; set { field = value; OnPropertyChanged(nameof(SpriteInfo_Sync)); } } = "Sync:";
    public string SpriteInfo_BoundRadius { get => field; set { field = value; OnPropertyChanged(nameof(SpriteInfo_BoundRadius)); } } = "BoundRadius:";
    public string SpriteInfo_BeamLength { get => field; set { field = value; OnPropertyChanged(nameof(SpriteInfo_BeamLength)); } } = "BeamLength:";
    public string SpriteInfo_OriginX { get => field; set { field = value; OnPropertyChanged(nameof(SpriteInfo_OriginX)); } } = "OriginX:";
    public string SpriteInfo_OriginY { get => field; set { field = value; OnPropertyChanged(nameof(SpriteInfo_OriginY)); } } = "OriginY:";

    // Command bar shortcuts. The menu texts above are kept verbatim because they
    // are also the labels of the file dialogs; these are the shorter toolbar forms.
    public string Action_Open { get => field; set { field = value; OnPropertyChanged(nameof(Action_Open)); } } = "Open";
    public string Command_SaveImage { get => field; set { field = value; OnPropertyChanged(nameof(Command_SaveImage)); } } = "Image";
    public string Command_SaveAnim { get => field; set { field = value; OnPropertyChanged(nameof(Command_SaveAnim)); } } = "Animation";
    public string Command_Export { get => field; set { field = value; OnPropertyChanged(nameof(Command_Export)); } } = "Frames";
    public string Command_SavePalette { get => field; set { field = value; OnPropertyChanged(nameof(Command_SavePalette)); } } = "Palette";

    // Transport bar
    public string Dock_PlayPause { get => field; set { field = value; OnPropertyChanged(nameof(Dock_PlayPause)); } } = "Play / Pause";
    public string Dock_Loop { get => field; set { field = value; OnPropertyChanged(nameof(Dock_Loop)); } } = "Loop";
    public string Dock_ZoomIn { get => field; set { field = value; OnPropertyChanged(nameof(Dock_ZoomIn)); } } = "Zoom in";
    public string Dock_ZoomOut { get => field; set { field = value; OnPropertyChanged(nameof(Dock_ZoomOut)); } } = "Zoom out";
    public string Dock_SidePanel { get => field; set { field = value; OnPropertyChanged(nameof(Dock_SidePanel)); } } = "Side panel";
    public string Dock_CollapseSection { get => field; set { field = value; OnPropertyChanged(nameof(Dock_CollapseSection)); } } = "Collapse / expand";
    public string Dock_Transparency { get => field; set { field = value; OnPropertyChanged(nameof(Dock_Transparency)); } } = "Hide transparent background";
    public string Dock_ZoomReset { get => field; set { field = value; OnPropertyChanged(nameof(Dock_ZoomReset)); } } = "Reset zoom";
    public string Dock_Fit { get => field; set { field = value; OnPropertyChanged(nameof(Dock_Fit)); } } = "Fit to window";

    // Empty state
    public string Empty_Title { get => field; set { field = value; OnPropertyChanged(nameof(Empty_Title)); } } = "No sprite open";
    public string Empty_Subtitle { get => field; set { field = value; OnPropertyChanged(nameof(Empty_Subtitle)); } } = "Open a .spr file, or drop one anywhere in this window.";
    public string Empty_Drop { get => field; set { field = value; OnPropertyChanged(nameof(Empty_Drop)); } } = "Release to open";

    public string Error_Title { get => field; set { field = value; OnPropertyChanged(nameof(Error_Title)); } } = "Something went wrong";

    // About dialog
    public string About_Title { get => field; set { field = value; OnPropertyChanged(nameof(About_Title)); } } = "About";
    public string About_Tagline { get => field; set { field = value; OnPropertyChanged(nameof(About_Tagline)); } } = "GoldSrc sprite viewer";
    public string About_BuiltWith { get => field; set { field = value; OnPropertyChanged(nameof(About_BuiltWith)); } } = "Built with";
    public string About_SourceCode { get => field; set { field = value; OnPropertyChanged(nameof(About_SourceCode)); } } = "Source code";

    public string Shared_OK { get; set; } = "OK";
    public string Shared_Cancel { get; set; } = "Cancel";

    public string FileManager_OpenSprite { get; set; } = "Open Sprite";
    public string FileManager_SaveImage { get; set; } = "Save Image";
    public string FileManager_SaveGIF { get; set; } = "Save GIF";
    public string FileManager_SaveSequence { get; set; } = "Save Sequence";
    public string FileManager_SavePalette { get; set; } = "Save Palette";

    public string CreateNew_Title { get; set; } = "Create New";
    public string CreateNew_Frames { get; set; } = "Frames";
    public string CreateNew_AddImage_Title { get; set; } = "Select Images";
    public string CreateNew_Tab_Property { get; set; } = "Property";
    public string CreateNew_Tab_Preview { get; set; } = "Preview";
    public string CreateNew_AddImage { get; set; } = "Add";
    public string CreateNew_RemoveImage { get; set; } = "Remove";
    public string CreateNew_MoveUpImage { get; set; } = "Move up";
    public string CreateNew_MoveDownImage { get; set; } = "Move down";
    public string CreateNew_PlaySpeed { get; set; } = "Play speed";
    public string CreateNew_PlaySpeed_WaterMarker { get; set; } = "Frames per seccond";
    public string CreateNew_Type { get; set; } = "Type:";
    public string CreateNew_Type_ParallelUpright { get; set; } = "ParallelUpright";
    public string CreateNew_Type_FacingUpright { get; set; } = "FacingUpright";
    public string CreateNew_Type_Parallel { get; set; } = "Parallel";
    public string CreateNew_Type_Oriented { get; set; } = "Oriented";
    public string CreateNew_Type_ParallelOriented { get; set; } = "ParallelOriented";
    public string CreateNew_Format { get; set; } = "Format:";
    public string CreateNew_Format_Normal { get; set; } = "Normal";
    public string CreateNew_Format_Additive { get; set; } = "Additive";
    public string CreateNew_Format_IndexAlpha { get; set; } = "IndexAlpha";
    public string CreateNew_Format_AlphaTest { get; set; } = "AlphaTest";
    public string CreateNew_Sync { get; set; } = "Sync:";
    public string CreateNew_Sync_Sync { get; set; } = "Sync";
    public string CreateNew_Sync_Random { get; set; } = "Random";
    public string CreateNew_BeamLength { get; set; } = "Beam Length:";
    public string CreateNew_BeamLength_Watermaker { get; set; } = "Optional，Useless in Half-Life";
    public string CreateNew_UnpackAnimate { get; set; } = "Unpack animated image";
    public string CreateNew_Export_Frame { get; set; } = "Frame:";
    public string CreateNew_Export_Play { get; set; } = "Play";
    public string CreateNew_Export_Width { get; set; } = "Export Width";
    public string CreateNew_Export_Height { get; set; } = "Export Height";
    public string CreateNew_Export_Save { get; set; } = "Export!";
    public string CreateNew_Export_NotSQRTWarning { get; set; } = "The size you set cannot be divided by 2, and it may not work in some versions of engine.";
    public string CreateNew_Export_Done { get; set; } = "Sprite exported successfully.";
}

/// <summary>
/// Reflection free JSON contract for <see cref="LangViewModel"/> (required under NativeAOT).
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LangViewModel))]
internal partial class LangJsonContext : JsonSerializerContext;

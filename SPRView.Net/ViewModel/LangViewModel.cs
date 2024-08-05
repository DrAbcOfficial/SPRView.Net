using System.ComponentModel;
namespace SPRView.Net.ViewModel;

public class LangViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private string m_TaskBar_File = "File";
    public string TaskBar_File
    {
        get => m_TaskBar_File;
        set
        {
            m_TaskBar_File = value;
            OnPropertyChanged(nameof(TaskBar_File));
        }
    }
    private string m_TaskBar_File_Create = "Create";
    public string TaskBar_File_Create
    {
        get => m_TaskBar_File_Create;
        set
        {
            m_TaskBar_File_Create = value;
            OnPropertyChanged(nameof(TaskBar_File_Create));
        }
    }
    private string m_TaskBar_File_Open = "Open";
    public string TaskBar_File_Open
    {
        get => m_TaskBar_File_Open;
        set
        {
            m_TaskBar_File_Open = value;
            OnPropertyChanged(nameof(TaskBar_File_Open));
        }
    }
    private string m_TaskBar_File_SaveFrame = "Save Frame";
    public string TaskBar_File_SaveFrame
    {
        get => m_TaskBar_File_SaveFrame;
        set
        {
            m_TaskBar_File_SaveFrame = value;
            OnPropertyChanged(nameof(TaskBar_File_SaveFrame));
        }
    }
    private string m_TaskBar_File_SaveGIF = "Save GIF";
    public string TaskBar_File_SaveGIF
    {
        get => m_TaskBar_File_SaveGIF;
        set
        {
            m_TaskBar_File_SaveGIF = value;
            OnPropertyChanged(nameof(TaskBar_File_SaveGIF));
        }
    }
    private string m_TaskBar_File_SavePalette = "Save Palette";
    public string TaskBar_File_SavePalette
    {
        get => m_TaskBar_File_SavePalette;
        set
        {
            m_TaskBar_File_SavePalette = value;
            OnPropertyChanged(nameof(TaskBar_File_SavePalette));
        }
    }
    private string m_TaskBar_File_Export = "Export";
    public string TaskBar_File_Export
    {
        get => m_TaskBar_File_Export;
        set
        {
            m_TaskBar_File_Export = value;
            OnPropertyChanged(nameof(TaskBar_File_Export));
        }
    }
    private string m_TaskBar_File_Exit = "Exit";
    public string TaskBar_File_Exit
    {
        get => m_TaskBar_File_Exit;
        set
        {
            m_TaskBar_File_Exit = value;
            OnPropertyChanged(nameof(TaskBar_File_Exit));
        }
    }

    private string m_TaskBar_View = "View";
    public string TaskBar_View
    {
        get => m_TaskBar_View;
        set
        {
            m_TaskBar_View = value;
            OnPropertyChanged(nameof(TaskBar_View));
        }
    }
    private string m_TaskBar_View_Information = "Information";
    public string TaskBar_View_Information
    {
        get => m_TaskBar_View_Information;
        set
        {
            m_TaskBar_View_Information = value;
            OnPropertyChanged(nameof(TaskBar_View_Information));
        }
    }
    private string m_TaskBar_View_Pallet = "Pallet";
    public string TaskBar_View_Pallet
    {
        get => m_TaskBar_View_Pallet;
        set
        {
            m_TaskBar_View_Pallet = value;
            OnPropertyChanged(nameof(TaskBar_View_Pallet));
        }
    }
    private string m_TaskBar_View_Language = "Language";
    public string TaskBar_View_Language
    {
        get => m_TaskBar_View_Language;
        set
        {
            m_TaskBar_View_Language = value;
            OnPropertyChanged(nameof(TaskBar_View_Language));
        }
    }

    private string m_TaskBar_Help = "Help";
    public string TaskBar_Help
    {
        get => m_TaskBar_Help;
        set
        {
            m_TaskBar_Help = value;
            OnPropertyChanged(nameof(TaskBar_Help));
        }
    }
    private string m_TaskBar_Help_About = "About";
    public string TaskBar_Help_About
    {
        get => m_TaskBar_Help_About;
        set
        {
            m_TaskBar_Help_About = value;
            OnPropertyChanged(nameof(TaskBar_Help_About));
        }
    }

    private string m_Dock_Frame = "Frame:";
    public string Dock_Frame
    {
        get => m_Dock_Frame;
        set
        {
            m_Dock_Frame = value;
            OnPropertyChanged(nameof(Dock_Frame));
        }
    }

    private string m_SpriteInfo = "Sprite Info:";
    public string SpriteInfo
    {
        get => m_SpriteInfo;
        set
        {
            m_SpriteInfo = value;
            OnPropertyChanged(nameof(SpriteInfo));
        }
    }
    private string m_SpriteInfo_Frames = "Frames:";
    public string SpriteInfo_Frames
    {
        get => m_SpriteInfo_Frames;
        set
        {
            m_SpriteInfo_Frames = value;
            OnPropertyChanged(nameof(SpriteInfo_Frames));
        }
    }
    private string m_SpriteInfo_Width = "Width:";
    public string SpriteInfo_Width
    {
        get => m_SpriteInfo_Width;
        set
        {
            m_SpriteInfo_Width = value;
            OnPropertyChanged(nameof(SpriteInfo_Width));
        }
    }
    private string m_SpriteInfo_Height = "Height:";
    public string SpriteInfo_Height
    {
        get => m_SpriteInfo_Height;
        set
        {
            m_SpriteInfo_Height = value;
            OnPropertyChanged(nameof(SpriteInfo_Height));
        }
    }
    private string m_SpriteInfo_Type = "Type:";
    public string SpriteInfo_Type
    {
        get => m_SpriteInfo_Type;
        set
        {
            m_SpriteInfo_Type = value;
            OnPropertyChanged(nameof(SpriteInfo_Type));
        }
    }
    private string m_SpriteInfo_Format = "Format:";
    public string SpriteInfo_Format
    {
        get => m_SpriteInfo_Format;
        set
        {
            m_SpriteInfo_Format = value;
            OnPropertyChanged(nameof(SpriteInfo_Format));
        }
    }
    private string m_SpriteInfo_Sync = "Sync:";
    public string SpriteInfo_Sync
    {
        get => m_SpriteInfo_Sync;
        set
        {
            m_SpriteInfo_Sync = value;
            OnPropertyChanged(nameof(SpriteInfo_Sync));
        }
    }
    private string m_SpriteInfo_BoundRadius = "BoundRadius:";
    public string SpriteInfo_BoundRadius
    {
        get => m_SpriteInfo_BoundRadius;
        set
        {
            m_SpriteInfo_BoundRadius = value;
            OnPropertyChanged(nameof(SpriteInfo_BoundRadius));
        }
    }
    private string m_SpriteInfo_BeamLength = "BeamLength:";
    public string SpriteInfo_BeamLength
    {
        get => m_SpriteInfo_BeamLength;
        set
        {
            m_SpriteInfo_BeamLength = value;
            OnPropertyChanged(nameof(SpriteInfo_BeamLength));
        }
    }
    private string m_SpriteInfo_OriginX = "OriginX:";
    public string SpriteInfo_OriginX
    {
        get => m_SpriteInfo_OriginX;
        set
        {
            m_SpriteInfo_OriginX = value;
            OnPropertyChanged(nameof(SpriteInfo_OriginX));
        }
    }
    private string m_SpriteInfo_OriginY = "OriginY:";
    public string SpriteInfo_OriginY
    {
        get => m_SpriteInfo_OriginY;
        set
        {
            m_SpriteInfo_OriginY = value;
            OnPropertyChanged(nameof(SpriteInfo_OriginY));
        }
    }

    public string Shared_OK { get; set; } = "OK";
    public string Shared_Cancel { get; set; } = "Cancel";

    public string FileManager_OpenSprite { get; set; } = "Open Sprite";
    public string FileManager_SaveImage { get; set; } = "Save Image";
    public string FileManager_SaveGIF { get; set; } = "Save GIF";
    public string FileManager_SaveSequence { get; set; } = "Save Sequence";
    public string FileManager_SavePalette { get; set; } = "Save Palette";

    public string CreateNew_Title { get; set; } = "Create New";
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
    public string CreateNew_Export_Frame { get; set; } = "Frame:";
    public string CreateNew_Export_Play { get; set; } = "Play";
    public string CreateNew_Export_Width { get; set; } = "Export Width";
    public string CreateNew_Export_Height { get; set; } = "Export Height";
    public string CreateNew_Export_Save { get; set; } = "Export!";
    public string CreateNew_Export_NotSQRTWarning { get; set; } = "The size you set cannot be divided by 2, and it may not work in some versions of engine.";
}

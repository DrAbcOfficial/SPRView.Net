using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SPRView.Net.Core;

namespace SPRView.Net.ViewModel;

/// <summary>
/// Turning the image list into a .spr file.
/// </summary>
public partial class CreateNewViewModel
{
    public int Export_Width { get; set; } = 64;
    public int Export_Height { get; set; } = 64;

    private int m_iProgress = 0;
    public int Progress { get => m_iProgress; set { m_iProgress = value; OnPropertyChanged(nameof(Progress)); } }

    public bool SaveValid => m_aryImagePaths.Count > 0;

    public async void SaveToSpr()
    {
        if (Export_Width % 2 == 1 || Export_Height % 2 == 1)
        {
            var box = MessageBoxWindow.CreateMessageBox(Lang!.CreateNew_Export_NotSQRTWarning, null, Lang.Shared_OK, Lang.Shared_Cancel);
            box.Position = new Avalonia.PixelPoint(Parent.Position.X + (int)Parent.Width / 2, Parent.Position.Y + (int)Parent.Height / 2);
            await box.ShowDialog(Parent);
        }
        Progress = 0;
        FilePickerFileType Sprites = new("GoldSrc Sprites")
        {
            Patterns = ["*.spr"],
            AppleUniformTypeIdentifiers = ["public.sprite"],
            MimeTypes = ["sprite/*"]
        };
        var files = await Parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang?.FileManager_OpenSprite,
            FileTypeChoices = [Sprites]
        });
        if (files != null)
        {
            Progress = 0;
            await using Stream fs = await files.OpenWriteAsync();
            SprDocument.Save([.. m_aryImagePaths], fs, Export_Width, Export_Height,
                (SpriteFormat)Format, (SpriteType)Type, (SpriteSynchron)Sync, BeamLength, UnPackAnimate);
            Progress = 200;
            var box = MessageBoxWindow.CreateMessageBox("☑︎💾", null, Lang!.Shared_OK, Lang.Shared_Cancel);
            box.Position = new Avalonia.PixelPoint(Parent.Position.X + (int)Parent.Width / 2, Parent.Position.Y + (int)Parent.Height / 2);
            await box.ShowDialog(Parent);
        }
    }
}

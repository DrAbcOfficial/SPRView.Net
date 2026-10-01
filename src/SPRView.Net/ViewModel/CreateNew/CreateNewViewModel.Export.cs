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
    public int Progress
    {
        get => m_iProgress;
        set
        {
            m_iProgress = value;
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(IsExporting));
        }
    }

    /// <summary>The progress bar only appears while an export is running.</summary>
    public bool IsExporting => m_iProgress is > 0 and < 200;

    public bool SaveValid => m_aryImagePaths.Count > 0;

    public async Task SaveToSpr()
    {
        if (Export_Width % 2 == 1 || Export_Height % 2 == 1)
        {
            var warn = MessageBoxWindow.CreateMessageBox(
                Lang!.CreateNew_Export_NotSQRTWarning, null, Lang.Shared_OK, Lang.Shared_Cancel);
            warn.Position = MainWindowViewModel.CenteredOver(Parent, warn);
            await warn.ShowDialog(Parent);
        }

        Progress = 0;
        var files = await Parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang?.CreateNew_Title,
            DefaultExtension = "spr",
            FileTypeChoices = [MainWindowViewModel.SpriteFileType]
        });
        if (files == null)
            return;

        try
        {
            Progress = 0;
            await using Stream fs = await files.OpenWriteAsync();
            SprDocument.Save([.. m_aryImagePaths], fs, Export_Width, Export_Height,
                (SpriteFormat)Format, (SpriteType)Type, (SpriteSynchron)Sync, BeamLength, UnPackAnimate);
            Progress = 200;

            var done = MessageBoxWindow.CreateMessageBox(
                Lang!.CreateNew_Export_Done, Lang.Shared_OK, Lang.Shared_OK);
            done.Position = MainWindowViewModel.CenteredOver(Parent, done);
            await done.ShowDialog(Parent);
            Parent.Close();
        }
        catch (Exception e)
        {
            Progress = 0;
            var failed = MessageBoxWindow.CreateMessageBox(e.ToString(), Lang!.Error_Title, Lang.Shared_OK);
            failed.Position = MainWindowViewModel.CenteredOver(Parent, failed);
            await failed.ShowDialog(Parent);
        }
    }
}

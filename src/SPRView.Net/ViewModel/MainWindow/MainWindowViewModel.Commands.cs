using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using SPRView.Net.Core;
using SPRView.Net.Services;
using SPRView.Net.Storage;
using SixLabors.ImageSharp.PixelFormats;

namespace SPRView.Net.ViewModel;

/// <summary>
/// Menu commands: file open/save, export and dialogs.
/// </summary>
public partial class MainWindowViewModel
{
    public async void CreateFile()
    {
        var createNew = new CreateNewWindow();
        var createNewData = new CreateNewViewModel(createNew)
        {
            Lang = Lang
        };
        createNew.DataContext = createNewData;
        await createNew.ShowDialog(Parent);
    }

    public async void OpenFile()
    {
        FilePickerFileType Sprites = new("GoldSrc Sprites")
        {
            Patterns = ["*.spr"],
            AppleUniformTypeIdentifiers = ["public.sprite"],
            MimeTypes = ["sprite/*"]
        };
        var files = await Parent.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang?.FileManager_OpenSprite,
            AllowMultiple = false,
            FileTypeFilter = [Sprites]
        });
        if (files.Count >= 1)
        {
            await using Stream file = await files[0].OpenReadAsync();
            App.LoadFile(file);
        }
    }

    public async void SaveFrame()
    {
        var bitmap = SPR ?? throw new ArgumentNullException("SPR is null!");
        var file = await Parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang?.FileManager_SaveImage,
            DefaultExtension = "bmp",
            FileTypeChoices = [FilePickerFileTypes.ImageAll],
            ShowOverwritePrompt = true
        });
        if (file == null)
            return;
        await using var stream = await file.OpenWriteAsync();
        bitmap.Save(stream, new PngBitmapEncoderOptions());
    }

    public async void SaveGIF()
    {
        var sprite = App.Storage.NowSprite ?? throw new ArgumentNullException("Storage sprite is null!");
        FilePickerFileType gifstype = new("Animate Image")
        {
            Patterns = ["*.gif", "*.webp"],
            AppleUniformTypeIdentifiers = ["public.gif", "public.webp"],
            MimeTypes = ["image/*"]
        };
        var file = await Parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang?.FileManager_SaveGIF,
            DefaultExtension = "gif",
            FileTypeChoices = [gifstype],
            ShowOverwritePrompt = true
        });
        if (file == null)
            return;
        await using var stream = await file.OpenWriteAsync();
        AnimationExporter.SaveAnimated(sprite, stream, file.TryGetLocalPath() ?? ".gif");
    }

    public async void Export()
    {
        var sprite = App.Storage.NowSprite ?? throw new ArgumentNullException("Storage sprite is null!");
        var directories = await Parent.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Lang?.FileManager_SaveSequence,
            AllowMultiple = false
        });
        if (directories.Count > 0)
        {
            string? directory = directories[0].TryGetLocalPath() ?? throw new DirectoryNotFoundException("Can not found directory");
            FrameSequenceExporter.ExportSequence(sprite, directory);
        }
    }

    public async void SavePalette()
    {
        FilePickerFileType types = new("Palette files")
        {
            Patterns = ["*.pal", "*.gpl"],
            AppleUniformTypeIdentifiers = ["microsoft.pal", "gimp.gpl"],
            MimeTypes = ["palette/*"]
        };
        var file = await Parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang?.FileManager_SaveGIF,
            DefaultExtension = "pal",
            FileTypeChoices = [types],
            ShowOverwritePrompt = true
        });
        if (file == null)
            return;
        var palette = App.Storage.NowPalette.GetOrigin() ?? throw new ArgumentNullException("Storage palette is null!");
        await using var fs = await file.OpenWriteAsync();
        string? ext = Path.GetExtension(file.TryGetLocalPath())?.ToLower();
        string? name = Path.GetFileName(file.TryGetLocalPath());
        if (ext == null || name == null)
            return;
        switch (ext)
        {
            case ".pal":
                PaletteFileWriter.WriteRiffPal(fs, palette);
                break;
            case ".gpl":
                PaletteFileWriter.WriteGimpPal(fs, palette, name);
                break;
        }
    }

    public void Exit() => Parent.Close();

    public async void About()
    {
        var about = new AboutWindow();
        await about.ShowDialog(Parent);
    }
}

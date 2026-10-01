using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using SPRView.Net.Services;
using SPRView.Net.Storage;
using SPRView.Net.Core;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace SPRView.Net.ViewModel;

/// <summary>
/// Command bar actions: file open/save, export, zoom and dialogs.
/// </summary>
public partial class MainWindowViewModel
{
    /// <summary>File types accepted by the open dialog and by drag and drop.</summary>
    public static readonly FilePickerFileType SpriteFileType = new("GoldSrc Sprites")
    {
        Patterns = ["*.spr"],
        AppleUniformTypeIdentifiers = ["public.sprite"],
        MimeTypes = ["sprite/*"]
    };

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
        var files = await Parent.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang?.FileManager_OpenSprite,
            AllowMultiple = false,
            FileTypeFilter = [SpriteFileType]
        });
        if (files.Count >= 1)
            OpenStorageItem(files[0]);
    }

    /// <summary>Opens a local path, used by drag and drop and by the CLI argument.</summary>
    public void OpenPath(string path)
    {
        try
        {
            using FileStream stream = File.OpenRead(path);
            LoadSprite(SprDocument.Load(stream), Path.GetFileName(path));
        }
        catch (Exception e)
        {
            ShowError(e);
        }
    }

    /// <summary>
    /// Opens a dialog or dropped item. Providers that expose a local path are read
    /// directly, otherwise the stream is used (sandboxed and remote providers).
    /// </summary>
    public async void OpenStorageItem(IStorageItem item)
    {
        try
        {
            string? path = item.TryGetLocalPath();
            if (path != null)
            {
                OpenPath(path);
                return;
            }

            if (item is IStorageFile file)
            {
                await using Stream stream = await file.OpenReadAsync();
                App.LoadFile(stream, file.Name);
            }
        }
        catch (Exception e)
        {
            ShowError(e);
        }
    }

    public async void SaveFrame()
    {
        var bitmap = SPR ?? throw new ArgumentNullException("SPR is null!");
        // PNG is the only lossless choice here, and the sprite needs its alpha.
        FilePickerFileType png = new("PNG image")
        {
            Patterns = ["*.png"],
            AppleUniformTypeIdentifiers = ["public.png"],
            MimeTypes = ["image/png"]
        };
        var file = await Parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang?.FileManager_SaveImage,
            DefaultExtension = "png",
            SuggestedFileName = SuggestName("frame"),
            FileTypeChoices = [png, FilePickerFileTypes.ImageAll],
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
            SuggestedFileName = SuggestName("anim"),
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
            Title = Lang?.FileManager_SavePalette,
            DefaultExtension = "pal",
            SuggestedFileName = SuggestName("palette"),
            FileTypeChoices = [types],
            ShowOverwritePrompt = true
        });
        if (file == null)
            return;
        var palette = App.Storage.NowPalette.GetOrigin() ?? throw new ArgumentNullException("Storage palette is null!");
        await using var fs = await file.OpenWriteAsync();
        string? local = file.TryGetLocalPath();
        string? ext = Path.GetExtension(local)?.ToLowerInvariant();
        string name = Path.GetFileNameWithoutExtension(local) ?? "palette";
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

    #region Zoom

    public void ZoomIn() => StepZoom(1);
    public void ZoomOut() => StepZoom(-1);

    /// <summary>Any explicit zoom step stops the zoom from following the window.</summary>
    private void MarkManualZoom() => _autoFit = false;

    /// <summary>Moves through a fixed zoom ladder so the readout stays on round steps.</summary>
    private void StepZoom(int direction)
    {
        double[] ladder = [25, 50, 75, 100, 150, 200, 300, 400, 600, 800];
        double current = ZoomPercent;
        double target = direction > 0
            ? ladder.FirstOrDefault(v => v > current + 0.5, ladder[^1])
            : ladder.LastOrDefault(v => v < current - 0.5, ladder[0]);
        MarkManualZoom();
        ZoomPercent = target;
    }

    public void ZoomReset()
    {
        MarkManualZoom();
        NowScale = 1.0f;
    }

    /// <summary>Fills the canvas again after the user has zoomed manually.</summary>
    public void ZoomToFit()
    {
        if (m_pSpr == null)
            return;
        _autoFit = true;
        ApplyAutoFit();
    }

    #endregion

    #region Playback

    public void NextFrame()
    {
        if (App.Storage.NowSprite == null)
            return;
        NowFrame = m_iNowFrame >= MaxFrame ? 0 : m_iNowFrame + 1;
    }

    public void PreviousFrame()
    {
        if (App.Storage.NowSprite == null)
            return;
        NowFrame = m_iNowFrame <= 0 ? MaxFrame : m_iNowFrame - 1;
    }

    #endregion

    public void Exit()
    {
        StopPlayback();
        Parent.Close();
    }

    public async void About()
    {
        var about = new AboutWindow();
        about.DataContext = new AboutViewModel(Lang!);
        await about.ShowDialog(Parent);
    }

    public async void ShowError(Exception e, string? title = null)
    {
        var box = MessageBoxWindow.CreateMessageBox(e.ToString(), title ?? Lang?.Error_Title, Lang?.Shared_OK);
        box.Position = CenteredOver(Parent, box);
        await box.ShowDialog(Parent);
    }

    private static string? GetExtension(IStorageFile file)
    {
        string? local = file.TryGetLocalPath();
        return local == null ? null : Path.GetExtension(local).ToLowerInvariant();
    }

    private string SuggestName(string suffix)
    {
        string stem = Path.GetFileNameWithoutExtension(App.Storage.FileName ?? "sprite");
        return $"{stem}_{suffix}";
    }

    /// <summary>Centres a dialog over its owner so it does not jump to a corner.</summary>
    public static PixelPoint CenteredOver(Window owner, Window dialog)
    {
        var ownerSize = owner.Bounds.Size;
        var dialogSize = dialog.Bounds.Size;
        return new PixelPoint(
            owner.Position.X + (int)((ownerSize.Width - dialogSize.Width) / 2),
            owner.Position.Y + (int)((ownerSize.Height - dialogSize.Height) / 2));
    }
}

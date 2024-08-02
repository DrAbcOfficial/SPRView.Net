using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.ComponentModel;
using System.IO;

namespace SPRView.Net.ViewModel;

public class CommandViewModel : INotifyPropertyChanged
{
    private DispatcherTimer animation_timer;
    public CommandViewModel()
    {
        animation_timer = new();
        animation_timer.Tick += (object? sender, EventArgs e) =>
        {
            var viewModel = App.GetViewModel();
            var spr = App.GetAppStorage().NowSprite ?? throw new NullReferenceException("Null storage spr");
            int frame = int.Parse(viewModel.NowFrame);
            frame++;
            if (frame >= spr.Frames.Count)
            {
                frame = 0;
                if (!viewModel.IsLoopPlay)
                    animation_timer.Stop();
            }
            viewModel.NowFrame = frame.ToString();
        };
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public async void OpenFile()
    {
        var mainWindow = App.GetMainWindow();
        FilePickerFileType Sprites = new("GoldSrc Sprites")
        {
            Patterns = ["*.spr"],
            AppleUniformTypeIdentifiers = ["public.sprite"],
            MimeTypes = ["sprite/*"]
        };
        var files = await mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = App.GetViewModel().Lang?.FileManager_OpenSprite,
            AllowMultiple = false,
            FileTypeFilter = [Sprites]
        });
        if (files.Count >= 1)
        {
            var storage = App.GetAppStorage();
            var newSprite = new Lib.CSprite(files[0].TryGetLocalPath());
            storage.NowSprite = newSprite;
            var viewModel = App.GetViewModel();
            viewModel.SPR = newSprite.GetBitmap(0);
            viewModel.SprViewerSize = viewModel.SPR.Size;
        }
    }
    public async void SaveFrame()
    {
        var bitmap = App.GetViewModel().SPR ?? throw new ArgumentNullException("SPR is null!");
        var mainWindow = App.GetMainWindow();
        var file = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = App.GetViewModel().Lang?.FileManager_SaveImage,
            DefaultExtension = "bmp",
            FileTypeChoices = [FilePickerFileTypes.ImageAll],
            ShowOverwritePrompt = true
        });
        if (file == null)
            return;
        await using var stream = await file.OpenWriteAsync();
        bitmap.Save(stream);
    }
    public async void SaveGIF()
    {
        var sprite = App.GetAppStorage().NowSprite ?? throw new ArgumentNullException("Storage sprite is null!");
        var mainWindow = App.GetMainWindow();
        FilePickerFileType gifstype = new("GIF Image")
        {
            Patterns = ["*.gif"],
            AppleUniformTypeIdentifiers = ["public.gif"],
            MimeTypes = ["image/*"]
        };
        var file = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = App.GetViewModel().Lang?.FileManager_SaveGIF,
            DefaultExtension = "gif",
            FileTypeChoices = [gifstype],
            ShowOverwritePrompt = true
        });
        if (file == null)
            return;
        using var gif = new Image<Rgba32>((int)sprite.MaxFrameWidth, (int)sprite.MaxFrameHeight);
        foreach (var frame in sprite.Frames)
        {
            using var memoryStream = new MemoryStream();
            frame.GetBitmap().Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var image = Image.Load<Rgba32>(memoryStream);
            gif.Frames.AddFrame(image.Frames[0]);
        }
        await using var stream = await file.OpenWriteAsync();
        GifMetadata gifMetadata = gif.Metadata.GetGifMetadata();
        gifMetadata.RepeatCount = 0;
        gif.Save(stream, new GifEncoder());
    }
    public async void Export()
    {
        var sprite = App.GetAppStorage().NowSprite ?? throw new ArgumentNullException("Storage sprite is null!");
        var mainWindow = App.GetMainWindow();
        var directories = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = App.GetViewModel().Lang?.FileManager_SaveSequence,
            AllowMultiple = false
        });
        if (directories.Count > 0)
        {
            string sequence = "";
            string? directory = directories[0].TryGetLocalPath() ?? throw new DirectoryNotFoundException("Can not found directory");
            for (int i = 0; i < sprite.Frames.Count; i++)
            {
                using StreamWriter sw = new(Path.Combine(directory, $"{i}.bmp"));
                sprite.Frames[i].GetBitmap().Save(sw.BaseStream);
                sequence += $"./{i}.bmp\n";
            }
            using StreamWriter text = new(Path.Combine(directory, $"sequence.qc"));
            text.Write(sequence);
        }
    }
    public void Exit()
    {
        App.GetMainWindow().Close();
    }

    public void FrameStep(string arg)
    {
        int step = int.Parse(arg);
        int now = int.Parse(App.GetViewModel().NowFrame);
        App.GetViewModel().NowFrame = (step + now).ToString();
    }
    public void ScaleStep(string arg)
    {
        int step = int.Parse(arg);
        int now = int.Parse(App.GetViewModel().NowScale);
        App.GetViewModel().NowScale = (step + now).ToString();
    }


    public void ToggleAnimationTimer()
    {
        if (!animation_timer.IsEnabled)
        {
            int timer_span = App.GetAppStorage().PlaySpeed;
            animation_timer.Interval = TimeSpan.FromSeconds(1.0f / timer_span);
            animation_timer.Start();
        }
        else
            animation_timer.Stop();
    }
}

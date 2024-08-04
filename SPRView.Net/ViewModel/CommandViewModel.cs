using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;
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
            await using Stream file = await files[0].OpenReadAsync();
            App.LoadFile(file);
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
        FilePickerFileType gifstype = new("Animate Image")
        {
            Patterns = ["*.gif", "*.webp"],
            AppleUniformTypeIdentifiers = ["public.gif", "public.webp"],
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
            var img = frame.GetImage();
            gif.Frames.AddFrame(img.Frames[0]);
        }

        await using var stream = await file.OpenWriteAsync();
        if (Path.GetExtension(file.TryGetLocalPath())?.ToLower() == ".webp"){
            WebpMetadata metadata = gif.Metadata.GetWebpMetadata();
            metadata.RepeatCount = 0;
            gif.Save(stream, new WebpEncoder());
        }
        else
        {
            GifMetadata gifMetadata = gif.Metadata.GetGifMetadata();
            gifMetadata.RepeatCount = 0;
            gif.Save(stream, new GifEncoder());
        }
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
                var img = sprite.Frames[i].GetImage();
                img.SaveAsBmp(sw.BaseStream);
                sequence += $"./{i}.bmp\n";
            }
            using StreamWriter text = new(Path.Combine(directory, $"sequence.qc"));
            text.Write(sequence);
        }
    }
    public async void SavePalette()
    {
        var mainWindow = App.GetMainWindow();
        FilePickerFileType types = new("Palette files")
        {
            Patterns = ["*.pal", "*.gpl"],
            AppleUniformTypeIdentifiers = ["microsoft.pal", "gimp.gpl"],
            MimeTypes = ["palette/*"]
        };
        var file = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = App.GetViewModel().Lang?.FileManager_SaveGIF,
            DefaultExtension = "pal",
            FileTypeChoices = [types],
            ShowOverwritePrompt = true
        });
        if (file == null)
            return;
        var palette = App.GetAppStorage().NowPalette ?? throw new ArgumentNullException("Storage palette is null!");
        var orgpalette = palette.GetOrigin() ?? throw new ArgumentNullException("Storage original palette is null!");
        await using var fs = await file.OpenWriteAsync();
        string? ext = Path.GetExtension(file.TryGetLocalPath())?.ToLower();
        string? name = Path.GetFileName(file.TryGetLocalPath());
        if (ext == null || name == null)
            return;
        switch (ext)
        {
            case ".pal":
                {
                    using BinaryWriter bw = new(fs);
                    //0x00 Magic
                    bw.Write((uint)0X52494646);
                    //0x04 FileSize
                    bw.Write((uint)0x0);
                    //0x08 RIFF
                    bw.Write((uint)0X50414C20);
                    //0x0B Block Header
                    bw.Write((uint)0X64617461);
                    //0x10 Block Size
                    bw.Write((uint)0x0);
                    //0x14 Palette Size
                    bw.Write((ushort)orgpalette.Length);
                    //0x16 Palette Version
                    bw.Write((ushort)0X300);
                    //0x18 Data
                    for (int i = 0; i < orgpalette.Length; i++)
                    {
                        bw.Write(orgpalette.AtIndex(i).R);
                        bw.Write(orgpalette.AtIndex(i).G);
                        bw.Write(orgpalette.AtIndex(i).B);
                        bw.Write((byte)0x00);
                    }
                    long fileSize = bw.BaseStream.Length;
                    bw.Seek(0x04, SeekOrigin.Begin);
                    bw.Write((uint)fileSize - 8);
                    bw.Seek(0x10, SeekOrigin.Begin);
                    bw.Write((uint)fileSize - 20);
                    break;
                }
            case ".gpl":
                {
                    using StreamWriter sw = new(fs);
                    sw.WriteLine("GIMP Palette");
                    sw.WriteLine($"Name: {name}");
                    sw.WriteLine("Columns: 16");
                    for(int i = 0; i < orgpalette.Length; i++)
                    {
                        Rgba32 rgba = orgpalette.AtIndex(i);
                        sw.WriteLine($"{rgba.R} {rgba.G} {rgba.B} Index {i}");
                    }
                    break;
                }
        }        
    }
    public void Exit()
    {
        App.GetMainWindow().Close();
    }
    public async void About()
    {
        var about = new AboutWindow();
        await about.ShowDialog(App.GetMainWindow());
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

    public void ChangeLang(string lang)
    {
        App.GetViewModel().LoadLangFile(lang);
    }
}

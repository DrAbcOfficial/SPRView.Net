using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SPRView.Net.Lib.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
namespace SPRView.Net.ViewModel;

public class CreateNewViewModel : INotifyPropertyChanged
{
    public Window Parent;
    public event PropertyChangedEventHandler? PropertyChanged;

    public CreateNewViewModel(Window parent)
    {
        Parent = parent;
        animation_timer = new();
        animation_timer.Tick += (object? sender, EventArgs e) =>
        {
            int frame = Preview_Frame;
            frame++;
            if (frame >= m_aryImagePaths.Count)
            {
                animation_timer.Stop();
                frame--;
            }
            Preview_Frame = frame;
            OnPropertyChanged(nameof(Preview_Frame));
        };
    }
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public int PathSelected { get; set; } = 0;
    private List<string> m_aryImagePaths = [];
    public string[] ImagePaths
    {
        get { return m_aryImagePaths.ToArray(); }
    }

    #region Property
    public int PlaySpeed { get; set; } = 24;
    public int Type { get; set; } = 0;
    public int Format { get; set; } = 0;
    public int Sync { get; set; } = 0;

    public async void AddImage()
    {
        var files = await Parent.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang?.CreateNew_AddImage_Title,
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypes.ImageAll],
        });
        if (files.Count >= 1)
        {
            foreach (var file in files)
            {
                string? local = file.TryGetLocalPath();
                if (local != null)
                    m_aryImagePaths.Add(local);
            }
            OnPropertyChanged(nameof(ImagePaths));
            OnPropertyChanged(nameof(Preview_MaxFrame));
            OnPropertyChanged(nameof(SaveValid));
        }
        ResetPreview();
    }
    public void RemoveImage()
    {
        if (m_aryImagePaths.Count <= 0)
            return;
        m_aryImagePaths.RemoveAt(PathSelected);
        OnPropertyChanged(nameof(ImagePaths));
        OnPropertyChanged(nameof(Preview_MaxFrame));
        OnPropertyChanged(nameof(SaveValid));
        if (PathSelected + 1 < m_aryImagePaths.Count)
            PathSelected += 1;
        else
            PathSelected = 0;
        OnPropertyChanged(nameof(PathSelected));
        if (!ResetPreview() && PreviewImage != null)
            PreviewImage = null;
    }
    public void MoveupImage()
    {
        if (m_aryImagePaths.Count <= 0)
            return;
        if (PathSelected <= 0)
            return;
        string p = m_aryImagePaths[PathSelected];
        m_aryImagePaths.RemoveAt(PathSelected);
        m_aryImagePaths.Insert(PathSelected - 1, p);
        OnPropertyChanged(nameof(ImagePaths));
        ResetPreview();
    }
    public void MovedownImage()
    {
        if (m_aryImagePaths.Count <= 0)
            return;
        if (PathSelected >= m_aryImagePaths.Count - 1)
            return;
        string p = m_aryImagePaths[PathSelected];
        m_aryImagePaths.RemoveAt(PathSelected);
        m_aryImagePaths.Insert(PathSelected + 1, p);
        PathSelected += 1;
        OnPropertyChanged(nameof(ImagePaths));
        OnPropertyChanged(nameof(PathSelected));
        ResetPreview();
    }
    #endregion

    #region Export
    private DispatcherTimer animation_timer;
    public void StartPreview()
    {
        if (!animation_timer.IsEnabled)
        {
            animation_timer.Interval = TimeSpan.FromSeconds(1.0f / PlaySpeed);
            animation_timer.Start();
        }
    }

    private bool ResetPreview()
    {
        if (m_aryImagePaths.Count > 0)
        {
            PreviewImage = new Bitmap(m_aryImagePaths.First());
            m_Preview_Frame = 0;
            return true;
        }
        return false;
    }
    private Bitmap? m_previewImage;
    public Bitmap? PreviewImage
    {
        get => m_previewImage;
        set
        {
            m_previewImage?.Dispose();
            m_previewImage = value;
            OnPropertyChanged(nameof(PreviewImage));
        }
    }
    private int m_Preview_Frame = 0;
    public int Preview_Frame
    {
        get => m_Preview_Frame;
        set
        {
            m_Preview_Frame = value;
            if (PreviewImage != null)
                PreviewImage = new Bitmap(m_aryImagePaths[m_Preview_Frame]);
        }
    }
    public int Preview_MaxFrame
    {
        get
        {
            return m_aryImagePaths.Count - 1;
        }
    }
    public int Export_Width { get; set; } = 64;
    public int Export_Height { get; set; } = 64;
    public int Progress { get; set; } = 0;
    public bool SaveValid
    {
        get => m_aryImagePaths.Count > 0;
    }
    public async void SaveToSpr()
    {
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
            //量化
            //降低一个维度，以便统一量化
            Image<Rgba32> image = new(Export_Width * m_aryImagePaths.Count, Export_Height * m_aryImagePaths.Count);
            for (int i = 0; i < m_aryImagePaths.Count; i++)
            {
                var path = m_aryImagePaths[i];
                SixLabors.ImageSharp.Image frame = SixLabors.ImageSharp.Image.Load(path);
                frame.Mutate(x => x.Resize(Export_Width, Export_Height));
                image.Mutate(x => x.DrawImage(frame, new Point(Export_Width * i, Export_Height * i), 1.0f));
                frame.Dispose();
            }
            WuQuantizer quantizer = new(new QuantizerOptions
            {
                Dither = null,
                MaxColors = Format == (int)ISprite.SpriteFormat.AlphaTest ? 255 : 256
            });
            Progress = 20;
            image.Mutate(x => x.Quantize(quantizer));
            //生成色板
            Dictionary<Rgba32, byte> palette = [];
            for (int j = 0; j < image.Height; j++)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    Rgba32 rgba32 = image[i, j];
                    if (!palette.ContainsKey(rgba32))
                        palette.Add(rgba32, (byte)palette.Count);
                }
            }
            if (Format == (int)ISprite.SpriteFormat.AlphaTest)
                palette.Add(new Rgba32(0, 0, 255, 255), 255);
            Progress = 40;
            //保存
            await using Stream file = await files.OpenWriteAsync();
            using BinaryWriter writer = new(file);
            //Header
            writer.Write(0x50534449);
            //Version
            writer.Write(0x00000002);
            //Type
            writer.Write(Type);
            //Format
            writer.Write(Format);
            //BoundRadius
            float radius = (float)Math.Sqrt(Math.Pow(Export_Width, 2) + Math.Pow(Export_Height, 2)) / 2;
            writer.Write(radius);
            //Width
            writer.Write(Export_Width);
            //Height
            writer.Write(Export_Height);
            //Count
            writer.Write(m_aryImagePaths.Count);
            //BeamLength
            writer.Write(0.0f);
            //Sync
            writer.Write(Sync);
            Progress = 60;
            //Palette Size
            writer.Write((short)256);
            //Palette
            foreach (var p in palette.Keys)
            {
                writer.Write(p.R);
                writer.Write(p.G);
                writer.Write(p.B);
            }
            if (palette.Count == 255)
            {
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)255);
            }
            Progress = 100;
            //保存数据
            for (int k = 0; k < m_aryImagePaths.Count; k++)
            {
                Progress += k / m_aryImagePaths.Count * 100;
                //Group
                writer.Write(0x00000000);
                //OriginX
                writer.Write(0x00000000);
                //OriginY
                writer.Write(0x00000000);
                //Width
                writer.Write(Export_Width);
                //Height
                writer.Write(Export_Height);

                var startX = k * Export_Width;
                var startY = k * Export_Height;
                for (int j = startY; j < startX + Export_Height; j++)
                {
                    for (int i = startX; i < startX + Export_Width; i++)
                    {
                        Rgba32 rgba32 = image[i, j];
                        if (Format == (int)ISprite.SpriteFormat.AlphaTest && rgba32.A <= 128)
                            writer.Write(palette.Last().Value);
                        else
                            writer.Write(palette[rgba32]);
                    }
                }
            }
            image.Dispose();
            var box = MessageBoxWindow.CreateMessageBox("☑︎🖻⏏💾", null, "🥰", "😠");
            box.Position = new Avalonia.PixelPoint(Parent.Position.X + (int)Parent.Width / 2, Parent.Position.Y + (int)Parent.Height / 2);
            await box.ShowDialog(Parent);
        }
    }
    #endregion

    private LangViewModel? _lang = null;
    public LangViewModel? Lang
    {
        get => _lang;
        set
        {
            if (_lang != value)
                _lang = value;
        }
    }
}

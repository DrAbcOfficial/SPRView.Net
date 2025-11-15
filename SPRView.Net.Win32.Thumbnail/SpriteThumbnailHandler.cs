using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SPRView.Net.Lib.Class;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SPRView.Net.Win32.Thumbnail;

[ComVisible(true)]
[COMServerAssociation(AssociationType.ClassOfExtension, ".spr")]
[DisplayName("SPRView.Net Thumbnail Handler")]
[Guid("684bf059-777d-1342-d33e-029672c1c25b")]
public class SpriteThumbnailHandler : SharpThumbnailHandler
{
    protected override Bitmap? GetThumbnailImage(uint w)
    {
        try
        {
            using StreamReader reader = new(SelectedItemStream);
            CSprite spr = new(reader.BaseStream);
            if (spr.Frames.FirstOrDefault()?.GetImage() is not Image<Rgba32> img)
                return null;
            float ratio = w / img.Width;
            //resize img
            img.Mutate(x => x.Resize((int)(img.Width * ratio), (int)(img.Height * ratio)));
            int width = img.Width;
            int height = img.Height;
            using Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Rectangle rect = new(0, 0, width, height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            ImageFrame<Rgba32> frame = img.Frames[0];
            try
            {
                unsafe
                {
                    for (int y = 0; y < height; y++)
                    {
                        Span<Rgba32> imageRow = frame.PixelBuffer.DangerousGetRowSpan(y);
                        nint bmpRowPtr = bmpData.Scan0 + y * bmpData.Stride;
                        MemoryMarshal.AsBytes(imageRow).CopyTo(new Span<byte>(bmpRowPtr.ToPointer(), width * 4));
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
            return new Bitmap(bitmap);
        }
        catch (Exception exception)
        {
            LogError("An exception occurred opening the file.", exception);
            return null;
        }
    }
}

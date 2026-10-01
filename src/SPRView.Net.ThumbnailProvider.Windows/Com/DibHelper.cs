using System.Runtime.InteropServices;

namespace SPRView.Net.ThumbnailProvider.Windows.Com;

/// <summary>
/// 32bpp ARGB DIB helpers for handing bitmaps to the shell.
/// </summary>
internal static unsafe class DibHelper
{
    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfoHeader
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo
    {
        public BitmapInfoHeader bmiHeader;
        public fixed uint bmiColors[1];
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern nint CreateDIBSection(nint hdc, BitmapInfo* pbmi, uint iUsage, void** ppvBits, nint hSection, uint dwOffset);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern nint CreateDIBSection(nint hdc, BitmapInfo* pbmi, uint iUsage, out nint ppvBits, nint hSection, uint dwOffset);

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint hwnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint hwnd, nint hdc);

    private const uint DIB_RGB_COLORS = 0;

    /// <summary>
    /// Create a top-down 32bpp ARGB DIB and fill it from tightly packed RGBA
    /// pixel data (swizzled to the BGRA memory layout GDI expects).
    /// </summary>
    public static nint Create32bppArgbDib(int width, int height, byte[] rgba)
    {
        nint hdc = GetDC(0);
        try
        {
            BitmapInfo bmi = default;
            bmi.bmiHeader.biSize = (uint)sizeof(BitmapInfoHeader);
            bmi.bmiHeader.biWidth = width;
            bmi.bmiHeader.biHeight = -height; // top-down
            bmi.bmiHeader.biPlanes = 1;
            bmi.bmiHeader.biBitCount = 32;
            bmi.bmiHeader.biCompression = 0; // BI_RGB

            nint bits;
            nint bitmap = CreateDIBSection(hdc, &bmi, DIB_RGB_COLORS, out bits, 0, 0);
            if (bitmap == 0 || bits == 0)
                return 0;

            try
            {
                int pixelCount = width * height;
                uint* pixels = (uint*)bits;
                fixed (byte* source = rgba)
                {
                    for (int i = 0; i < pixelCount; i++)
                    {
                        byte r = source[(i * 4) + 0];
                        byte g = source[(i * 4) + 1];
                        byte b = source[(i * 4) + 2];
                        byte a = source[(i * 4) + 3];
                        pixels[i] = (uint)((b << 16) | (g << 8) | r | (a << 24));
                    }
                }
            }
            catch
            {
                DeleteObject(bitmap);
                throw;
            }
            return bitmap;
        }
        finally
        {
            ReleaseDC(0, hdc);
        }
    }

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint hObject);
}

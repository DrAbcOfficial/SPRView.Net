#include "dib_helper.h"

namespace {

struct BitmapInfo
{
    BITMAPINFOHEADER header;
    RGBQUAD colors[1];
};

} // namespace

HBITMAP Create32bppArgbDib(int width, int height, std::span<const std::byte> rgba) noexcept
{
    if (width <= 0 || height <= 0)
        return nullptr;

    BitmapInfo bmi{
        .header =
            BITMAPINFOHEADER{
                .biSize = sizeof(BITMAPINFOHEADER),
                .biWidth = width,
                .biHeight = -height, // top-down
                .biPlanes = 1,
                .biBitCount = 32,
                .biCompression = BI_RGB,
            },
    };

    HDC screen = GetDC(nullptr);
    if (screen == nullptr)
        return nullptr;

    void* bits = nullptr;
    HBITMAP bitmap = CreateDIBSection(screen, reinterpret_cast<const BITMAPINFO*>(&bmi), DIB_RGB_COLORS, &bits, nullptr, 0);
    ReleaseDC(nullptr, screen);
    if (bitmap == nullptr || bits == nullptr)
    {
        if (bitmap != nullptr)
            DeleteObject(bitmap);
        return nullptr;
    }

    const auto pixels = static_cast<uint32_t*>(bits);
    const size_t pixelCount = static_cast<size_t>(width) * static_cast<size_t>(height);
    if (rgba.size() < pixelCount * 4)
    {
        DeleteObject(bitmap);
        return nullptr;
    }

    const auto* source = reinterpret_cast<const uint8_t*>(rgba.data());
    for (size_t i = 0; i < pixelCount; ++i)
    {
        const uint8_t r = source[i * 4 + 0];
        const uint8_t g = source[i * 4 + 1];
        const uint8_t b = source[i * 4 + 2];
        const uint8_t a = source[i * 4 + 3];
        pixels[i] = static_cast<uint32_t>(b << 16 | g << 8 | r | a << 24);
    }
    return bitmap;
}

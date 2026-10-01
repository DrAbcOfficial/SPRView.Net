#pragma once

#include <windows.h>

#include <cstddef>
#include <span>

/// Creates a top-down 32bpp ARGB DIB filled from tightly packed RGBA8888
/// pixels (swizzled to the BGRA memory layout GDI expects).
[[nodiscard]] HBITMAP Create32bppArgbDib(int width, int height,
    std::span<const std::byte> rgba) noexcept;

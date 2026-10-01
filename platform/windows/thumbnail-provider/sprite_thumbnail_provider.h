#pragma once

#include "module.h"

#include <thumbcache.h>

#include <vector>
#include <cstddef>

/// The shell extension object: receives the .spr contents as a stream and
/// renders the first frame through the NativeAOT sprview_core library into a
/// 32bpp ARGB DIB for Explorer.
class SpriteThumbnailProvider final : public IInitializeWithStream,
                                     public IThumbnailProvider
{
public:
    SpriteThumbnailProvider() noexcept : m_cRef(1) { ModuleAddRef(); }

    // IUnknown
    IFACEMETHODIMP QueryInterface(REFIID riid, void** ppv) noexcept override;
    IFACEMETHODIMP_(ULONG) AddRef() noexcept override;
    IFACEMETHODIMP_(ULONG) Release() noexcept override;

    // IInitializeWithStream
    IFACEMETHODIMP Initialize(IStream* stream, DWORD grfMode) noexcept override;

    // IThumbnailProvider
    IFACEMETHODIMP GetThumbnail(UINT cx, HBITMAP* phbmp, WTS_ALPHATYPE* pdwAlpha) noexcept override;

private:
    ~SpriteThumbnailProvider() { ModuleRelease(); }

    long m_cRef;
    std::vector<std::byte> m_sprBytes;
};

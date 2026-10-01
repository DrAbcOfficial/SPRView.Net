#include "sprite_thumbnail_provider.h"
#include "com_ids.h"
#include "dib_helper.h"
#include "sprview_core_loader.h"

namespace {

constexpr size_t kMaxSprBytes = 256 * 1024 * 1024;
constexpr size_t kDrainChunkBytes = 64 * 1024;

/// Drains the shell stream completely; a short final chunk is reported by
/// IStream::Read as S_FALSE, which simply ends the loop.
[[nodiscard]] bool DrainStream(IStream* stream, std::vector<std::byte>& out) noexcept
{
    std::byte chunk[kDrainChunkBytes];
    ULONG bytesRead = 0;
    for (;;)
    {
        if (FAILED(stream->Read(chunk, sizeof(chunk), &bytesRead)))
            return false;
        if (bytesRead == 0)
            return true;
        if (out.size() + bytesRead > kMaxSprBytes)
            return false;
        out.insert(out.end(), chunk, chunk + bytesRead);
    }
}

} // namespace

IFACEMETHODIMP SpriteThumbnailProvider::QueryInterface(REFIID riid, void** ppv) noexcept
{
    if (ppv == nullptr)
        return E_POINTER;
    if (riid == IID_IUnknown)
        *ppv = static_cast<IUnknown*>(static_cast<IInitializeWithStream*>(this));
    else if (riid == __uuidof(IInitializeWithStream))
        *ppv = static_cast<IInitializeWithStream*>(this);
    else if (riid == __uuidof(IThumbnailProvider))
        *ppv = static_cast<IThumbnailProvider*>(this);
    else
    {
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    AddRef();
    return S_OK;
}

IFACEMETHODIMP_(ULONG) SpriteThumbnailProvider::AddRef() noexcept
{
    return InterlockedIncrement(&m_cRef);
}

IFACEMETHODIMP_(ULONG) SpriteThumbnailProvider::Release() noexcept
{
    const ULONG cRef = InterlockedDecrement(&m_cRef);
    if (cRef == 0)
        delete this;
    return cRef;
}

IFACEMETHODIMP SpriteThumbnailProvider::Initialize(IStream* stream, DWORD) noexcept
{
    if (stream == nullptr)
        return E_UNEXPECTED;
    if (!m_sprBytes.empty())
        return HRESULT_FROM_WIN32(ERROR_ALREADY_INITIALIZED);

    // The core library must be usable before we accept work; failing here
    // makes the shell fall back to another handler.
    if (!sprview::CoreLoader::Load().has_value())
        return E_FAIL;

    std::vector<std::byte> bytes;
    if (!DrainStream(stream, bytes) || bytes.size() < sizeof(spr_info))
        return E_FAIL;
    m_sprBytes = std::move(bytes);
    return S_OK;
}

IFACEMETHODIMP SpriteThumbnailProvider::GetThumbnail(UINT cx, HBITMAP* phbmp, WTS_ALPHATYPE* pdwAlpha) noexcept
{
    if (phbmp == nullptr || pdwAlpha == nullptr)
        return E_POINTER;
    *phbmp = nullptr;
    *pdwAlpha = WTSAT_UNKNOWN;

    // cx is the requested box size; sprites are tiny pixel art, so the native
    // frame size is handed back unresampled (the shell scales it down).
    if (m_sprBytes.empty())
        return E_UNEXPECTED;

    const auto core = sprview::CoreLoader::Load();
    if (!core.has_value())
        return S_FALSE;

    spr_handle handle = core->open_memory(
        reinterpret_cast<const uint8_t*>(m_sprBytes.data()),
        static_cast<intptr_t>(m_sprBytes.size()));
    if (handle == nullptr)
        return S_FALSE;

    int32_t width = 0;
    int32_t height = 0;
    int32_t originX = 0;
    int32_t originY = 0;
    std::vector<std::byte> rgba;
    HBITMAP bitmap = nullptr;

    if (core->get_frame_info(handle, 0, &width, &height, &originX, &originY) == SPR_RESULT_OK &&
        width > 0 && height > 0)
    {
        rgba.resize(static_cast<size_t>(width) * static_cast<size_t>(height) * 4);
        if (core->read_frame_rgba(handle, 0, reinterpret_cast<uint8_t*>(rgba.data()),
                static_cast<intptr_t>(rgba.size())) == SPR_RESULT_OK)
        {
            bitmap = Create32bppArgbDib(width, height, rgba);
        }
    }
    core->close(handle);

    if (bitmap == nullptr)
        return S_FALSE;

    *phbmp = bitmap;
    *pdwAlpha = WTSAT_ARGB;
    return S_OK;
}

#include "pch.h"
#include "ThumbnailProvider.h"

#include <filesystem>
#include <fstream>
#include <shellapi.h>
#include <Shlwapi.h>
#include <string>

extern HINSTANCE g_hInst;
extern long g_cDllRef;

SprThumbnailProvider::SprThumbnailProvider() :
    m_cRef(1), m_pStream(NULL), m_process(NULL)
{
    InterlockedIncrement(&g_cDllRef);
}

SprThumbnailProvider::~SprThumbnailProvider()
{
    InterlockedDecrement(&g_cDllRef);
}

#pragma region IUnknown

IFACEMETHODIMP SprThumbnailProvider::QueryInterface(REFIID riid, void** ppv)
{
    static const QITAB qit[] = {
        QITABENT(SprThumbnailProvider, IThumbnailProvider),
        QITABENT(SprThumbnailProvider, IInitializeWithStream),
        { 0 },
    };
    return QISearch(this, qit, riid, ppv);
}

IFACEMETHODIMP_(ULONG)
SprThumbnailProvider::AddRef()
{
    return InterlockedIncrement(&m_cRef);
}

IFACEMETHODIMP_(ULONG)
SprThumbnailProvider::Release()
{
    ULONG cRef = InterlockedDecrement(&m_cRef);
    if (0 == cRef)
    {
        delete this;
    }
    return cRef;
}

#pragma endregion

#pragma region IInitializationWithStream

IFACEMETHODIMP SprThumbnailProvider::Initialize(IStream* pStream, DWORD grfMode)
{
    HRESULT hr = E_INVALIDARG;
    if (pStream)
    {
        // Initialize can be called more than once, so release existing valid
        // m_pStream.
        if (m_pStream)
        {
            m_pStream->Release();
            m_pStream = NULL;
        }

        m_pStream = pStream;
        m_pStream->AddRef();
        hr = S_OK;
    }
    return hr;
}

#pragma endregion

#pragma region IThumbnailProvider

IFACEMETHODIMP SprThumbnailProvider::GetThumbnail(UINT cx, HBITMAP* phbmp, WTS_ALPHATYPE* pdwAlpha)
{
    STATSTG statstg;
    if (FAILED(m_pStream->Stat(&statstg, STATFLAG_NONAME)))
        return S_FALSE;
    ULARGE_INTEGER uliSize = statstg.cbSize;
    std::vector<byte> buffer(static_cast<size_t>(uliSize.QuadPart));
    ULONG bytesRead;
    if (FAILED(m_pStream->Read(buffer.data(), static_cast<ULONG>(buffer.size()), &bytesRead)))
        return S_FALSE;
    m_pStream->Release();
    m_pStream = NULL;
    //fuck, not spr
    if (buffer.size() < 34)
        return S_FALSE;
    std::istringstream input(std::string(buffer.begin(), buffer.end()), std::ios::binary);
    int tempint = 0;
    input.read(reinterpret_cast<char*>(&tempint), sizeof(tempint));
    if (tempint != 0x50534449)
        return S_FALSE;
    input.read(reinterpret_cast<char*>(&tempint), sizeof(tempint));
    if (tempint != 0x00000002)
        return S_FALSE;
    input.seekg(32, std::ios::cur);
    short palettesize = 0;
    input.read(reinterpret_cast<char*>(&palettesize), sizeof(palettesize));
    using Color = struct
    {
        byte R;
        byte G;
        byte B;
    };
    std::vector<Color*> colors = {};
    for (short i = 0; i < palettesize; i++) {
        Color* c = new Color();
        input.read(reinterpret_cast<char*>(c), sizeof(Color));
    }
    std::streampos currentPos = input.tellg();
    input.seekg(12, std::ios::cur);
    //width
    int width = 0;
    input.read(reinterpret_cast<char*>(&width), sizeof(width));
    //height
    int height = 0;
    input.read(reinterpret_cast<char*>(&height), sizeof(height));  
    BITMAPINFO bmi;
    ZeroMemory(&bmi, sizeof(BITMAPINFO));
    bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
    bmi.bmiHeader.biWidth = width;
    bmi.bmiHeader.biHeight = -height;
    bmi.bmiHeader.biPlanes = 1;
    bmi.bmiHeader.biBitCount = 24;
    bmi.bmiHeader.biCompression = BI_RGB;
    HDC hdc = GetDC(NULL);
    void* pBits = NULL;
    HBITMAP hBitmap = CreateDIBSection(hdc, &bmi, DIB_RGB_COLORS, &pBits, NULL, 0);
    if (hBitmap && pBits) {
        int rowBytes = ((width * 3 + 3) & ~3);
        BYTE* pRow = (BYTE*)pBits;
        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x) {
                byte data = 0;
                input.read(reinterpret_cast<char*>(&data), sizeof(data));
                Color* color = colors[data];
                pRow[x * 3 + 0] = color->B;
                pRow[x * 3 + 1] = color->G;
                pRow[x * 3 + 2] = color->R;
            }
            pRow += rowBytes;
        }
    }
    ReleaseDC(NULL, hdc);
    *phbmp = hBitmap;
    *pdwAlpha = WTS_ALPHATYPE::WTSAT_RGB;
    for (auto iter = colors.begin(); iter != colors.end(); iter++) {
        delete* iter;
    }
    colors.clear();
    return S_OK;
}

#pragma endregion

#pragma region Helper Functions

#pragma endregion
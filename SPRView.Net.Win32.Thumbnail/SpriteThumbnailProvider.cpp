#include "SpriteThumbnailProvider.h"
#include <Shlwapi.h>
#include <vector>
#include <string>
#include <sstream>

#pragma comment(lib, "Shlwapi.lib")


extern HINSTANCE g_hInst;
extern long g_cRefModule;


SpriteThumbnailProvider::SpriteThumbnailProvider() : m_cRef(1), m_pStream(NULL)
{
    InterlockedIncrement(&g_cRefModule);
}

HRESULT SpriteThumbnailProvider::CreateInstance(REFIID riid, void** ppv)
{
    SpriteThumbnailProvider* pNew = new (std::nothrow) SpriteThumbnailProvider();
    HRESULT hr = pNew ? S_OK : E_OUTOFMEMORY;
    if (SUCCEEDED(hr))
    {
        hr = pNew->QueryInterface(riid, ppv);
        pNew->Release();
    }
    return hr;
}


SpriteThumbnailProvider::~SpriteThumbnailProvider()
{
    InterlockedDecrement(&g_cRefModule);
}


#pragma region IUnknown

// Query to the interface the component supported.
IFACEMETHODIMP SpriteThumbnailProvider::QueryInterface(REFIID riid, void** ppv)
{
    static const QITAB qit[] =
    {
        QITABENT(SpriteThumbnailProvider, IThumbnailProvider),
        QITABENT(SpriteThumbnailProvider, IInitializeWithStream),
        { 0 },
    };
    return QISearch(this, qit, riid, ppv);
}

// Increase the reference count for an interface on an object.
IFACEMETHODIMP_(ULONG) SpriteThumbnailProvider::AddRef()
{
    return InterlockedIncrement(&m_cRef);
}

// Decrease the reference count for an interface on an object.
IFACEMETHODIMP_(ULONG) SpriteThumbnailProvider::Release()
{
    ULONG cRef = InterlockedDecrement(&m_cRef);
    if (0 == cRef)
    {
        delete this;
    }

    return cRef;
}

#pragma endregion


#pragma region IInitializeWithStream

// Initializes the thumbnail handler with a stream.
IFACEMETHODIMP SpriteThumbnailProvider::Initialize(IStream* pStream, DWORD grfMode)
{
    // A handler instance should be initialized only once in its lifetime. 
    HRESULT hr = HRESULT_FROM_WIN32(ERROR_ALREADY_INITIALIZED);
    if (m_pStream == NULL)
    {
        // Take a reference to the stream if it has not been initialized yet.
        hr = pStream->QueryInterface(&m_pStream);
    }
    return hr;
}

#pragma endregion


#pragma region IThumbnailProvider

// Gets a thumbnail image and alpha type. The GetThumbnail is called with the 
// largest desired size of the image, in pixels. Although the parameter is 
// called cx, this is used as the maximum size of both the x and y dimensions. 
// If the retrieved thumbnail is not square, then the longer axis is limited 
// by cx and the aspect ratio of the original image respected. On exit, 
// GetThumbnail provides a handle to the retrieved image. It also provides a 
// value that indicates the color format of the image and whether it has 
// valid alpha information.
IFACEMETHODIMP SpriteThumbnailProvider::GetThumbnail(UINT cx, HBITMAP* phbmp, WTS_ALPHATYPE* pdwAlpha)
{
    if (m_pStream == NULL)
        return E_UNEXPECTED;

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
        return E_UNEXPECTED;
    std::istringstream input(std::string(buffer.begin(), buffer.end()), std::ios::binary);
    int tempint = 0;
    input.read(reinterpret_cast<char*>(&tempint), sizeof(tempint));
    if (tempint != 0x49445350)  //IDSP
    return S_FALSE;
    input.read(reinterpret_cast<char*>(&tempint), sizeof(tempint));
    if (tempint != 0x00000002) //Version 2
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
    std::vector<Color> colors;
	for (short i = 0; i < palettesize; i++) {
        Color c{};
		input.read(reinterpret_cast<char*>(&c), sizeof(Color));
		colors.push_back(c);
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
				byte data;
				if (!input.read(reinterpret_cast<char*>(&data), sizeof(data)))
					return E_FAIL;
				if (data >= colors.size())
					return E_FAIL;
				const Color& color = colors[data];
				pRow[x * 3 + 0] = color.B;
				pRow[x * 3 + 1] = color.G;
				pRow[x * 3 + 2] = color.R;
			}
			pRow += rowBytes;
		}
    }
    ReleaseDC(NULL, hdc);
    *phbmp = hBitmap;
    *pdwAlpha = WTS_ALPHATYPE::WTSAT_RGB;
    colors.clear();
    return S_OK;
}

#pragma endregion
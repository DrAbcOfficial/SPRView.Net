#include "pch.h"
#include "ThumbnailProvider.h"

#include <filesystem>
#include <fstream>
#include <shellapi.h>
#include <Shlwapi.h>
#include <string>
#include <wincrypt.h>

#include <wil/com.h>


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


std::wstring ExecuteAndCaptureOutput(const std::wstring& command, const std::wstring& params) {
    SECURITY_ATTRIBUTES sa;
    sa.nLength = sizeof(SECURITY_ATTRIBUTES);
    sa.bInheritHandle = TRUE;
    sa.lpSecurityDescriptor = NULL;

    HANDLE hRead, hWrite;
    if (!CreatePipe(&hRead, &hWrite, &sa, 0)) {
        return L"";
    }

    STARTUPINFO si = { sizeof(STARTUPINFO) };
    si.dwFlags = STARTF_USESTDHANDLES | STARTF_USESHOWWINDOW;
    si.hStdOutput = hWrite;
    si.hStdError = hWrite;
    si.hStdInput = NULL;
    si.wShowWindow = SW_HIDE;

    PROCESS_INFORMATION pi;
    std::wstring commandLine = command + L" " + params;
    if (!CreateProcess(NULL, const_cast<wchar_t*>(commandLine.c_str()), NULL, NULL, TRUE, 0, NULL, NULL, &si, &pi)) {
        CloseHandle(hRead);
        CloseHandle(hWrite);
        return L"";
    }
    CloseHandle(hWrite);
    wchar_t buffer[4096];
    DWORD bytesRead;
    std::wstring result;
    while (ReadFile(hRead, buffer, sizeof(buffer) - 1, &bytesRead, NULL) && bytesRead > 0) {
        buffer[bytesRead] = L'\0';
        result += buffer;
    }
    CloseHandle(hRead);
    CloseHandle(pi.hProcess);
    CloseHandle(pi.hThread);
    return result;
}
std::vector<BYTE> Base64Decode(const std::wstring& base64) {
    DWORD decodedLength = 0;
    if (!CryptStringToBinaryW(base64.c_str(), base64.length(), CRYPT_STRING_BASE64, NULL, &decodedLength, NULL, NULL))
        throw std::runtime_error("Failed to calculate decoded length.");
    std::vector<BYTE> decodedData(decodedLength);
    if (!CryptStringToBinaryW(base64.c_str(), base64.length(), CRYPT_STRING_BASE64, decodedData.data(), &decodedLength, NULL, NULL))
        throw std::runtime_error("Failed to decode base64 string.");
    return decodedData;
}
HBITMAP CreateHBitmapFromBase64(const std::wstring& base64) {
    std::vector<BYTE> decodedData = Base64Decode(base64);
    BITMAPFILEHEADER* pFileHeader = reinterpret_cast<BITMAPFILEHEADER*>(decodedData.data());
    BITMAPINFOHEADER* pInfoHeader = reinterpret_cast<BITMAPINFOHEADER*>(decodedData.data() + sizeof(BITMAPFILEHEADER));
    void* pBitmapData = decodedData.data() + pFileHeader->bfOffBits;
    HDC hdc = GetDC(NULL);
    HBITMAP hBitmap = CreateDIBitmap(hdc, pInfoHeader, CBM_INIT, pBitmapData, reinterpret_cast<BITMAPINFO*>(pInfoHeader), DIB_RGB_COLORS);
    ReleaseDC(NULL, hdc);
    if (!hBitmap) 
        throw std::runtime_error("Failed to create HBITMAP.");
    return hBitmap;
}


IFACEMETHODIMP SprThumbnailProvider::GetThumbnail(UINT cx, HBITMAP* phbmp, WTS_ALPHATYPE* pdwAlpha)
{
    // Read stream into the buffer
    char buffer[4096];
    ULONG cbRead;
    GUID guid;
    if (CoCreateGuid(&guid) == S_OK)
    {
        wil::unique_cotaskmem_string guidString;
        if (SUCCEEDED(StringFromCLSID(guid, &guidString)))
        {
            // {CLSID} -> CLSID
            std::wstring guid = std::wstring(guidString.get()).substr(1, std::wstring(guidString.get()).size() - 2);
            std::wstring filePath = L".\\Temp\\";
            if (!std::filesystem::exists(filePath))
                std::filesystem::create_directories(filePath);
            std::wstring fileName = filePath + guid + L".spr";
            // Write data to tmp file
            std::fstream file;
            file.open(fileName, std::ios_base::out | std::ios_base::binary);
            if (!file.is_open())
                return 0;
            while (true)
            {
                auto result = m_pStream->Read(buffer, 4096, &cbRead);
                file.write(buffer, cbRead);
                if (result == S_FALSE)
                {
                    break;
                }
            }
            file.close();
            m_pStream->Release();
            m_pStream = NULL;

            std::wstring appPath = L".\\SPRView.Net.CLI.exe ";
            std::wstring cmdLine{ L"\"" + fileName + L"\"" };
            
            std::wstring base64 = ExecuteAndCaptureOutput(appPath, cmdLine);
            std::filesystem::remove(fileName);
            
            *phbmp = CreateHBitmapFromBase64(base64);
            *pdwAlpha = WTS_ALPHATYPE::WTSAT_ARGB;
        }
    }

    // ensure releasing the stream (not all if branches contain it)
    if (m_pStream)
    {
        m_pStream->Release();
        m_pStream = NULL;
    }

    return S_OK;
}

#pragma endregion

#pragma region Helper Functions

#pragma endregion
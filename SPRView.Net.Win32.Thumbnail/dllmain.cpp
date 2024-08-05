// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "ClassFactory.h"
#include "Reg.h"
#include "ShlObj.h"
#include "olectl.h"

HINSTANCE g_hInst = NULL;
long g_cDllRef = 0;

// {C7657C4A-9F68-40fa-A4DF-96BC08EB3551}
static const GUID CLSID_SprThumbnailProvider = 
{ 0xC7657C4A, 0x9F68, 0x40fa, {0xA4, 0xDF, 0x96, 0xBC, 0x08, 0xEB, 0x35, 0x51} };
// {8F45BF90-A84F-4BB5-8043-65F5E62F89AE}
static const GUID APPID_SprThumbnailProvider =
{ 0x8f45bf90, 0xa84f, 0x4bb5, { 0x80, 0x43, 0x65, 0xf5, 0xe6, 0x2f, 0x89, 0xae } };

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD ul_reason_for_call,
    LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        g_hInst = hModule;
        DisableThreadLibraryCalls(hModule);
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

//
//   FUNCTION: DllGetClassObject
//
//   PURPOSE: Create the class factory and query to the specific interface.
//
//   PARAMETERS:
//   * rclsid - The CLSID that will associate the correct data and code.
//   * riid - A reference to the identifier of the interface that the caller
//     is to use to communicate with the class object.
//   * ppv - The address of a pointer variable that receives the interface
//     pointer requested in riid. Upon successful return, *ppv contains the
//     requested interface pointer. If an error occurs, the interface pointer
//     is NULL.
//
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, void** ppv)
{
    HRESULT hr = CLASS_E_CLASSNOTAVAILABLE;

    if (IsEqualCLSID(CLSID_SprThumbnailProvider, rclsid))
    {
        hr = E_OUTOFMEMORY;

        ClassFactory* pClassFactory = new ClassFactory();
        if (pClassFactory)
        {
            hr = pClassFactory->QueryInterface(riid, ppv);
            pClassFactory->Release();
        }
    }

    return hr;
}

//
//   FUNCTION: DllCanUnloadNow
//
//   PURPOSE: Check if we can unload the component from the memory.
//
//   NOTE: The component can be unloaded from the memory when its reference
//   count is zero (i.e. nobody is still using the component).
//
STDAPI DllCanUnloadNow(void)
{
    return g_cDllRef > 0 ? S_FALSE : S_OK;
}

//
//   FUNCTION: DllRegisterServer
//
//   PURPOSE: Register the COM server and the thumbnail handler.
// 
STDAPI DllRegisterServer(void)
{
    HRESULT hr;

    wchar_t szModule[MAX_PATH];
    if (GetModuleFileName(g_hInst, szModule, ARRAYSIZE(szModule)) == 0)
    {
        hr = HRESULT_FROM_WIN32(GetLastError());
        return hr;
    }

    // Register the component.
    hr = RegisterInprocServer(szModule, CLSID_SprThumbnailProvider,
        L"SPRView.Net.Win32.Thumbnail.SprThumbnailProvider Class",
        L"Apartment");
    if (SUCCEEDED(hr))
    {
        // Register the thumbnail handler. The thumbnail handler is associated
        // with the .recipe file class.
        hr = RegisterShellExtThumbnailHandler(L"spr",
            CLSID_SprThumbnailProvider);
        if (SUCCEEDED(hr))
        {
            // This tells the shell to invalidate the thumbnail cache. It is 
            // important because any .recipe files viewed before registering 
            // this handler would otherwise show cached blank thumbnails.
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, NULL, NULL);
        }
    }

    return hr;
}
//
//   FUNCTION: DllUnregisterServer
//
//   PURPOSE: Unregister the COM server and the thumbnail handler.
// 
STDAPI DllUnregisterServer(void)
{
    HRESULT hr = S_OK;

    wchar_t szModule[MAX_PATH];
    if (GetModuleFileName(g_hInst, szModule, ARRAYSIZE(szModule)) == 0)
    {
        hr = HRESULT_FROM_WIN32(GetLastError());
        return hr;
    }

    // Unregister the component.
    hr = UnregisterInprocServer(CLSID_SprThumbnailProvider);
    if (SUCCEEDED(hr))
    {
        // Unregister the thumbnail handler.
        hr = UnregisterShellExtThumbnailHandler(L"spr");
    }

    return hr;
}
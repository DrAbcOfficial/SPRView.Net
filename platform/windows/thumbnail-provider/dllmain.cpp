#include "module.h"

HINSTANCE g_hInst = nullptr;
long g_cRefModule = 0;

void ModuleAddRef() noexcept { InterlockedIncrement(&g_cRefModule); }
void ModuleRelease() noexcept { InterlockedDecrement(&g_cRefModule); }

BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID reserved) noexcept
{
    switch (reason)
    {
    case DLL_PROCESS_ATTACH:
        g_hInst = module;
        DisableThreadLibraryCalls(module);
        break;
    case DLL_PROCESS_DETACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    }
    return TRUE;
}

// The NativeAOT sprview_core.dll loaded through this extension cannot be
// unloaded safely, but it simply stays resident; the shell may still unload
// this dll itself.
STDAPI DllCanUnloadNow() noexcept
{
    return g_cRefModule == 0 ? S_OK : S_FALSE;
}

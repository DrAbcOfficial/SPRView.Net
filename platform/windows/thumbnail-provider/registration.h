#pragma once

#include <windows.h>

// Self registration under HKCU (no elevation required), also reachable via
// `regsvr32 sprview-thumbnailer-win.dll`.
STDAPI DllRegisterServer() noexcept;
STDAPI DllUnregisterServer() noexcept;

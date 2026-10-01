#pragma once

#include <windows.h>

// Defined in dllmain.cpp; shared by every translation unit of the extension.
extern HINSTANCE g_hInst;

void ModuleAddRef() noexcept;
void ModuleRelease() noexcept;

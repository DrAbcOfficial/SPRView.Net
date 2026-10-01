#pragma once

#include <guiddef.h>

// Class id of the SPRView thumbnail provider. Kept identical to the historic
// implementations so existing registrations upgrade in place.
inline constexpr GUID CLSID_SpriteThumbnailProvider = {
    0x4D555153, 0x67DE, 0x4350, {0x86, 0x0D, 0x67, 0x1B, 0x76, 0x18, 0xB8, 0x3B}};
inline constexpr wchar_t CLSID_SpriteThumbnailProviderString[] =
    L"{4D555153-67DE-4350-860D-671B7618B83B}";
inline constexpr wchar_t ProviderDisplayName[] = L"SPRView Thumbnail Preview";

// Registry location selecting the thumbnail handler for .spr files.
inline constexpr wchar_t ShellExThumbnailHandlerKey[] =
    L"Software\\Classes\\.spr\\ShellEx\\{e357fccd-a995-4576-b01f-234630154e96}";

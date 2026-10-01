#include "registration.h"
#include "com_ids.h"
#include "module.h"

#include <shlobj.h>

#include <filesystem>
#include <string>

namespace {

constexpr wchar_t ClsidKeyFormat[] = L"Software\\Classes\\CLSID\\{4D555153-67DE-4350-860D-671B7618B83B}";

void NotifyShellAssociationChanged() noexcept
{
    SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, nullptr, nullptr);
}

[[nodiscard]] std::wstring GetOwnModulePath() noexcept
{
    wchar_t path[MAX_PATH]{};
    if (GetModuleFileNameW(g_hInst, path, MAX_PATH) == 0)
        return {};
    return path;
}

void CreateValue(const wchar_t* keyPath, const wchar_t* valueName, const wchar_t* data)
{
    HKEY key = nullptr;
    const LSTATUS status = RegCreateKeyExW(HKEY_CURRENT_USER, keyPath, 0, nullptr,
        REG_OPTION_NON_VOLATILE, KEY_SET_VALUE, nullptr, &key, nullptr);
    if (status != ERROR_SUCCESS)
        throw std::system_error(status, std::system_category(), "RegCreateKeyExW");
    const LSTATUS setValue = RegSetValueExW(key, valueName, 0, REG_SZ,
        reinterpret_cast<const BYTE*>(data),
        static_cast<DWORD>((wcslen(data) + 1) * sizeof(wchar_t)));
    RegCloseKey(key);
    if (setValue != ERROR_SUCCESS)
        throw std::system_error(setValue, std::system_category(), "RegSetValueExW");
}

void DeleteTree(const wchar_t* keyPath) noexcept
{
    // A missing key is not an error when unregistering.
    RegDeleteTreeW(HKEY_CURRENT_USER, keyPath);
}

} // namespace

STDAPI DllRegisterServer() noexcept
{
    try
    {
        const std::wstring modulePath = GetOwnModulePath();
        if (modulePath.empty())
            return E_FAIL;

        const std::wstring clsidKey = ClsidKeyFormat;
        CreateValue(clsidKey.c_str(), nullptr, ProviderDisplayName);
        CreateValue((std::filesystem::path(clsidKey) / L"InProcServer32").wstring().c_str(),
            nullptr, modulePath.c_str());
        CreateValue((std::filesystem::path(clsidKey) / L"InProcServer32").wstring().c_str(),
            L"ThreadingModel", L"Apartment");
        CreateValue(ShellExThumbnailHandlerKey, nullptr, CLSID_SpriteThumbnailProviderString);

        NotifyShellAssociationChanged();
        return S_OK;
    }
    catch (...)
    {
        return E_FAIL;
    }
}

STDAPI DllUnregisterServer() noexcept
{
    DeleteTree(ClsidKeyFormat);
    DeleteTree(ShellExThumbnailHandlerKey);
    NotifyShellAssociationChanged();
    return S_OK;
}

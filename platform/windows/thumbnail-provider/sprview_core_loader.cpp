#include "sprview_core_loader.h"
#include "module.h"

#include <filesystem>

namespace sprview {
namespace {

HMODULE g_coreModule = nullptr;
CoreFunctions g_functions{};

/// Resolves one export; the whole load fails when any is missing.
template <typename T>
bool ResolveExport(HMODULE module, const char* name, T& out) noexcept
{
    out = reinterpret_cast<T>(GetProcAddress(module, name));
    return out != nullptr;
}

} // namespace

std::wstring CoreLoader::CoreLibraryPath() noexcept
{
    wchar_t modulePath[MAX_PATH]{};
    if (GetModuleFileNameW(g_hInst, modulePath, MAX_PATH) == 0)
        return {};
    std::filesystem::path directory(modulePath);
    directory.remove_filename();
    return (directory / L"sprview_core.dll").wstring();
}

std::optional<CoreFunctions> CoreLoader::Load() noexcept
{
    if (g_coreModule != nullptr)
        return g_functions;

    const std::wstring libraryPath = CoreLibraryPath();
    if (libraryPath.empty())
        return std::nullopt;

    // LOAD_WITH_ALTERED_SEARCH_PATH: the relative lookup must happen next to
    // this extension, not next to whatever host process loaded us (explorer).
    HMODULE module = LoadLibraryExW(libraryPath.c_str(), nullptr, LOAD_WITH_ALTERED_SEARCH_PATH);
    if (module == nullptr)
        return std::nullopt;

    CoreFunctions functions{};
    if (!ResolveExport(module, "sprview_abi_version", functions.abi_version) ||
        !ResolveExport(module, "sprview_open_memory", functions.open_memory) ||
        !ResolveExport(module, "sprview_get_info", functions.get_info) ||
        !ResolveExport(module, "sprview_get_frame_info", functions.get_frame_info) ||
        !ResolveExport(module, "sprview_read_frame_rgba", functions.read_frame_rgba) ||
        !ResolveExport(module, "sprview_render_png", functions.render_png) ||
        !ResolveExport(module, "sprview_close", functions.close) ||
        !ResolveExport(module, "sprview_free_buffer", functions.free_buffer) ||
        functions.abi_version() < SPRVIEW_ABI_VERSION)
    {
        FreeLibrary(module);
        return std::nullopt;
    }

    g_coreModule = module;
    g_functions = functions;
    return g_functions;
}

} // namespace sprview

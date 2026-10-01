#pragma once

#include "sprview_core.h"

#include <optional>
#include <string>

namespace sprview {

/// Function pointers into the NativeAOT sprview_core shared library.
/// Types come straight from the shared sprview_core.h so the ABI cannot drift.
struct CoreFunctions
{
    decltype(&sprview_abi_version) abi_version;
    decltype(&sprview_open_memory) open_memory;
    decltype(&sprview_get_info) get_info;
    decltype(&sprview_get_frame_info) get_frame_info;
    decltype(&sprview_read_frame_rgba) read_frame_rgba;
    decltype(&sprview_render_png) render_png;
    decltype(&sprview_close) close;
    decltype(&sprview_free_buffer) free_buffer;
};

/// Loads sprview_core.dll lazily from this extension's own directory.
class CoreLoader
{
public:
    /// Returns the resolved entry points, or nullopt when the library is
    /// missing, incompatible or incomplete.
    [[nodiscard]] static std::optional<CoreFunctions> Load() noexcept;

private:
    [[nodiscard]] static std::wstring CoreLibraryPath() noexcept;
};

} // namespace sprview

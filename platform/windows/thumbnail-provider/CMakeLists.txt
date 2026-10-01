cmake_minimum_required(VERSION 3.21)
project(sprview_thumbnailer_win LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

add_library(sprview-thumbnailer-win SHARED
    dllmain.cpp
    com_ids.h
    class_factory.h
    class_factory.cpp
    sprite_thumbnail_provider.h
    sprite_thumbnail_provider.cpp
    sprview_core_loader.h
    sprview_core_loader.cpp
    dib_helper.h
    dib_helper.cpp
    registration.h
    registration.cpp
    export_functions.def)

# The COM surface (sprview_core.h) is shared with the NativeAOT core: it is
# the single source of truth for the C ABI this extension calls at runtime.
target_include_directories(sprview-thumbnailer-win PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}/../../../src/SPRView.Net.Core/Native)

target_compile_definitions(sprview-thumbnailer-win PRIVATE
    _UNICODE UNICODE WIN32_LEAN_AND_MEAN NOMINMAX)

if(MSVC)
    target_compile_options(sprview-thumbnailer-win PRIVATE /W4 /permissive-)
endif()

target_link_libraries(sprview-thumbnailer-win PRIVATE
    user32 gdi32 advapi32 shell32)

set_target_properties(sprview-thumbnailer-win PROPERTIES
    OUTPUT_NAME "sprview-thumbnailer-win"
    PREFIX "")

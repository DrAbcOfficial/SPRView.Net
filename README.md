
# SPRView.Net<img src="readme/icon.png" align="right" width="120"/>

Yet another Sprite viewer for half-life series game (counter-strike, sven co-op, etc.)

![Downloads](https://img.shields.io/github/downloads/DrAbcOfficial/SPRView.Net/total?style=for-the-badge)
![Repo Size](https://img.shields.io/github/repo-size/DrAbcOfficial/SPRView.Net?style=for-the-badge)
![Last Commit](https://img.shields.io/github/last-commit/DrAbcOfficial/SPRView.Net?style=for-the-badge)


----
# ✅Getting Start

- [中文](READMECN.md)
-  Grab prebuild binaries from [release](https://github.com/DrAbcOfficial/SPRView.Net/releases) — x64 and arm64 for macOS/Linux, x64 for Windows — everything is published with <img src="https://raw.githubusercontent.com/dotnet/brand/main/logo/dotnet-logo.svg" width="24"/> NativeAOT, no .NET runtime install needed
-  To build from source you need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (plus `clang` and `zlib1g-dev` on Linux)

----
# 🧩 Architecture

```
src/
  SPRView.Net.Core                       spr parsing & rendering library
    ├─ Spr/         document, header, frame, palette, enums
    ├─ IO/          SprReader / SprWriter (binary format)
    ├─ Rendering/   frame decoding, image composition & quantization
    └─ Native/      C ABI exports (sprview_core.h) for NativeAOT shared lib
  SPRView.Net/                           Avalonia cross platform viewer
    ├─ Themes/      WinUI 3 design tokens, vector icons, control themes
    ├─ Window/      the four windows, caption and resize chrome
    ├─ ViewModel/   viewer / create-new / localization state
    └─ Storage/     per instance document and palette state
  SPRView.Net.CLI/                       command line tool
  SPRView.Net.Thumbnailer/               sprview-thumbnailer executable
platform/
  linux/    XDG thumbnailer entry, mime type, install scripts
  macos/    Quick Look extension sources (Swift) + install script
  windows/  Explorer shell extension (C++20 COM) + register scripts
tools/
  UiSnapshot/  renders every window headlessly to PNG for layout review
```

## 🎨 Interface

The viewer is built on one design language — WinUI 3 / Fluent 2 — rather than
the host theme, so the three platforms render identically:

- **Design tokens** in `Themes/Tokens.axaml` mirror the Fluent 2 resource
  names (`SolidBackgroundFillColorBaseBrush`, `TextFillColorPrimaryBrush`,
  `AccentFillColorDefaultBrush`, …) with a light and a dark table; the app
  follows the OS theme and gets correct contrast in both.
- **Icons** in `Themes/Icons.axaml` are hand-drawn `StreamGeometry` on a 20×20
  grid. No emoji and no platform icon fonts, because neither is available
  everywhere.
- **Chrome** is app-drawn: the caption, the command bar and the resize grips
  come from `Window/WindowChrome.cs`, so the window looks the same instead of
  picking up the native title bar on each OS.
- **Layout** is a caption, a command bar, a checkerboard canvas, a collapsible
  side panel and a transport bar. Small sprites are auto-fitted on whole-number
  zoom steps, so pixel art stays sharp.
- **Transparency** is applied per the sprite format by default. The toggle in
  the command bar turns that off to show the frames exactly as the file stores
  them, background and all, which is what you want when checking what an artist
  painted underneath the artwork.

`tools/UiSnapshot` renders every window off screen in both themes, which is how
the layout is reviewed without a desktop session:

```sh
dotnet run --project tools/UiSnapshot -- <output dir> [file.spr] [lang]
```

`SPRView.Net.Core` is the single implementation of the GoldSrc sprite format.
It is consumed as a managed library by the GUI, CLI and thumbnailer, and can
also be published as a native shared library exporting a plain C ABI:

```sh
dotnet publish src/SPRView.Net.Core -c Release -r <rid> -p:NativeLib=Shared -p:PublishAot=true
# -> sprview_core.dll / .so / .dylib, see src/SPRView.Net.Core/Native/sprview_core.h
```

All three platform thumbnail providers sit under `platform/` and share that
C ABI: the Windows Explorer extension is an independent C++20 project that
loads `sprview_core.dll` at runtime, and the macOS QuickLook extension calls
the same library (or shells out to `sprview-thumbnailer`).

# 🖼️ Thumbnail providers

Double-click viewing is not the only way to see a sprite any more — file
manager thumbnails are supported on all three platforms:

- **Windows** — run `install.ps1 -DllPath .\sprview-thumbnailer-win.dll`
  from the release archive to register the Explorer thumbnail handler for
  `.spr` (HKCU, no admin needed); `uninstall.ps1` removes it. The extension
  is a C++20 COM dll; see
  [thumbnail-provider/README.md](platform/windows/thumbnail-provider/README.md)
  to build it.
- **Linux (XDG)** — `sudo ./thumbnailer-linux/install.sh` installs the
  `sprview-thumbnailer` binary plus the XDG thumbnailer entry and the
  `application/x-spr` mime type; `--user` installs to `~/.local` instead.
- **macOS (Quick Look)** — install the renderer with
  `sudo ./thumbnailer-macos/install.sh`, then follow
  [thumbnailer-macos/quicklook/README.md](platform/macos/quicklook/README.md)
  to build the Quick Look extension in Xcode.


----

# ❓️Why

- My favourite tool is too old.
- Newer tools I don't like.
- The newer tools that I like are too ugly.
- And no cross-platform

# 💡Function

This tool only provides the function to double-click and view spr, and more likely will not add the function to view/make a `wad`, view/make a `pak`, etc.

If you need, I recommend the following repositories:

- [GIMP-hl-sprite-plugin](https://github.com/Psycrow101/GIMP-hl-sprite-plugin)
- [HL-Texture-Tools](https://github.com/yuraj11/HL-Texture-Tools)
- [WadMaker](https://github.com/pwitvoet/wadmaker)

# 🖼️ Screenshot

|Viewer|Create new|
|--|--|
|<img src="readme/main-light.png" width="420"/>|<img src="readme/createnew-light.png" width="420"/>|

|Dark theme|Empty state|
|--|--|
|<img src="readme/main-dark.png" width="420"/>|<img src="readme/empty-light.png" width="420"/>|


# This repository used:

1. [AvaloniaUI](https://avaloniaui.net/)
2. [ImageSharp](https://github.com/SixLabors/ImageSharp)
3. [.NET 10 / C# 14 / NativeAOT](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
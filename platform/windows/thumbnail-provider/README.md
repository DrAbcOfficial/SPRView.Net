# Windows Explorer thumbnail provider (C++20)

`sprview-thumbnailer-win.dll` is an in-proc COM server implementing the shell
thumbnail handler contract (`IClassFactory` + `IInitializeWithStream` +
`IThumbnailProvider`) for `.spr` files.

It contains no sprite decoding at all: rendering is delegated to the
NativeAOT `sprview_core` shared library through the shared C ABI declared in
[`src/SPRView.Net.Core/Native/sprview_core.h`](../../../src/SPRView.Net.Core/Native/sprview_core.h),
which this project includes directly. The same ABI serves the macOS QuickLook
extension and any other native consumer; `sprview_core.dll` must sit next to
this dll at runtime (the release archive already does).

## Build

Requires CMake ≥ 3.21 and the MSVC toolchain (any Visual Studio 2019+ with
C++ workload):

```sh
cmake -S platform/windows/thumbnail-provider -B build/cpp-provider -A x64
cmake --build build/cpp-provider --config Release
# -> build/cpp-provider/Release/sprview-thumbnailer-win.dll
```

`scripts/publish-all.ps1 -os windows` runs this automatically.

## Install / uninstall

```powershell
powershell -File install.ps1 -DllPath .\sprview-thumbnailer-win.dll
powershell -File ..\uninstall.ps1
```

Both write only to `HKCU`, so no elevation is required. The dll also supports
`regsvr32 sprview-thumbnailer-win.dll` / `regsvr32 /u` if you prefer.

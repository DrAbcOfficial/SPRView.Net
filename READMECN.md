
# SPRView.Net<img src="readme/icon.png" align="right" width="120"/>

是的又是一个 half-life 系列游戏 (counter-strike, sven co-op, etc.) spr查看器

![Downloads](https://img.shields.io/github/downloads/DrAbcOfficial/SPRView.Net/total?style=for-the-badge)
![Repo Size](https://img.shields.io/github/repo-size/DrAbcOfficial/SPRView.Net?style=for-the-badge)
![Last Commit](https://img.shields.io/github/last-commit/DrAbcOfficial/SPRView.Net?style=for-the-badge)


----
# ✅开始

-  从 [release](https://github.com/DrAbcOfficial/SPRView.Net/releases)获得预先编译的二进制文件：macOS/Linux 提供 x64 与 arm64，Windows 提供 x64。所有产物均以 <img src="https://raw.githubusercontent.com/dotnet/brand/main/logo/dotnet-logo.svg" width="24"/> NativeAOT 编译为原生代码，无需安装 .NET 运行时
-  从源码构建需要 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)（Linux 上还需要 `clang` 和 `zlib1g-dev`）

----
# 🧩 架构

```
src/
  SPRView.Net.Core                       spr 解析与渲染核心库
    ├─ Spr/         文档、文件头、帧、调色板、枚举
    ├─ IO/          SprReader / SprWriter（二进制格式）
    ├─ Rendering/   帧解码、图像合成与量化
    └─ Native/      C ABI 导出（sprview_core.h），可发布为原生共享库
  SPRView.Net/                           Avalonia 跨平台查看器
    ├─ Themes/      WinUI 3 设计令牌、矢量图标、控件主题
    ├─ Window/      四个窗口，以及标题栏与缩放边框
    ├─ ViewModel/   查看器 / 新建 / 本地化状态
    └─ Storage/     实例级的文档与调色板状态
  SPRView.Net.CLI/                       命令行工具
  SPRView.Net.Thumbnailer/               sprview-thumbnailer 可执行程序
platform/
  linux/    XDG thumbnailer 入口、MIME 类型、安装脚本
  macos/    Quick Look 扩展源码（Swift）+ 安装脚本
  windows/  Explorer 缩略图扩展（C++20 COM）+ 注册脚本
tools/
  UiSnapshot/  离屏渲染所有窗口为 PNG，用于界面评审
```

## 🎨 界面

界面只使用一套设计语言 —— WinUI 3 / Fluent 2 —— 而不跟随宿主主题，
因此三个平台渲染结果一致：

- `Themes/Tokens.axaml` 中的**设计令牌**沿用 Fluent 2 的资源命名
  （`SolidBackgroundFillColorBaseBrush`、`TextFillColorPrimaryBrush`、
  `AccentFillColorDefaultBrush` 等），并分别提供亮色与暗色表；
  应用跟随系统主题，两种主题下对比度都正确。
- `Themes/Icons.axaml` 中的**图标**是手绘 `StreamGeometry`，基于 20×20 网格。
  不使用 emoji，也不使用平台图标字体 —— 因为它们并非处处可用。
- **窗口外框**由应用自绘：标题栏、命令栏与缩放边框都来自
  `Window/WindowChrome.cs`，因此不会在各系统上变成原生标题栏。
- **布局**为：标题栏、命令栏、棋盘格画布、侧栏、底部控制条。
  侧栏默认收起，让画面占满窗口；命令栏最左侧的按钮可将它展开。
  侧栏内的信息与调色板区块可各自通过标题行收起，不需要看元数据时
  可让 256 色色板占满整个侧栏。命令栏默认只显示图标，窗口足够宽时才
  展开文字标签，避免裁切。
  小尺寸精灵自动以整数倍缩放适应窗口，像素画保持锐利。
- **版本号**统一由仓库根目录的 `version.txt` 提供。MSBuild 在编译期把它与
  编译时间一并写入二进制，关于页面再从程序集中读回，因此单文件发布版本
  也能显示自身来源。需要可重现构建时传 `-p:IncludeBuildTime=false`。
- **透明背景**默认按精灵图格式的规则剔除。命令栏的开关可关闭此行为，
  按文件存储的像素原样显示（包含背景），便于查看作者在画面下实际绘制了什么。

`tools/UiSnapshot` 可离屏渲染每个窗口的亮/暗主题，便于在无桌面环境下评审：

```sh
dotnet run --project tools/UiSnapshot -- <输出目录> [file.spr] [语言]
```

`SPRView.Net.Core` 是 GoldSrc sprite 格式的唯一实现。GUI、CLI 和缩略图程序
以托管库方式引用它；它还可以发布为导出纯 C ABI 的原生共享库：

```sh
dotnet publish src/SPRView.Net.Core -c Release -r <rid> -p:NativeLib=Shared -p:PublishAot=true
# -> sprview_core.dll / .so / .dylib，接口见 src/SPRView.Net.Core/Native/sprview_core.h
```

三个平台的缩略图提供者都位于 `platform/` 并共享该 C ABI：Windows Explorer
扩展是独立的 C++20 工程，运行时加载 `sprview_core.dll`；macOS QuickLook
扩展调用同一库（或调起 `sprview-thumbnailer`）。

# 🖼️ 缩略图支持

除了双击查看，三大平台的文件管理器缩略图也已支持：

- **Windows** — 在发布包中执行 `install.ps1 -DllPath .\sprview-thumbnailer-win.dll`
  注册 Explorer 缩略图处理器（写入 HKCU，无需管理员）；`uninstall.ps1` 卸载。
  扩展是 C++20 COM DLL，构建说明见
  [thumbnail-provider/README.md](platform/windows/thumbnail-provider/README.md)。
- **Linux (XDG)** — `sudo ./thumbnailer-linux/install.sh` 安装
  `sprview-thumbnailer`、XDG thumbnailer 入口和 `application/x-spr` MIME 类型；
  加 `--user` 则安装到 `~/.local`。
- **macOS (Quick Look)** — 先 `sudo ./thumbnailer-macos/install.sh` 安装渲染器，
  再按 [thumbnailer-macos/quicklook/README.md](platform/macos/quicklook/README.md)
  在 Xcode 中构建 Quick Look 扩展。


----

# ❓️为什么

- 我最喜欢的工具太老
- 较新的工具我不喜欢
- 较新我又喜欢的太丑
- 而且基本都不跨平台

# 💡功能

这个工具只提供双击后查看spr的能力, 所以以后大概率不会替换类似于查看/制作 `wad`, `pak`, 之类文件的功能.

如果你有需求我推荐使用:

- [GIMP-hl-sprite-plugin](https://github.com/Psycrow101/GIMP-hl-sprite-plugin)
- [HL-Texture-Tools](https://github.com/yuraj11/HL-Texture-Tools)
- [WadMaker](https://github.com/pwitvoet/wadmaker)

# 🖼️ 截图

|查看器|新建精灵图|
|--|--|
|<img src="readme/main-light.png" width="420"/>|<img src="readme/createnew-light.png" width="420"/>|

|暗色主题|关于|
|--|--|
|<img src="readme/main-dark.png" width="420"/>|<img src="readme/about-light.png" width="280"/>|


# 第三方库:

1. [AvaloniaUI](https://avaloniaui.net/)
2. [ImageSharp](https://github.com/SixLabors/ImageSharp)
3. [.NET 10 / C# 14 / NativeAOT](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
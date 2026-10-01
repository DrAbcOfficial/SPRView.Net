param(
    [Parameter()]
    [String]$arch = "x64",
    [String]$os = "windows"
)

# Publishes every deliverable for one platform as NativeAOT binaries:
#
#   build/
#     SPRView.Net(.exe)                     Avalonia viewer
#     SPRView.Net.CLI(.exe)                 command line tool
#     sprview-thumbnailer(.exe)             cross platform thumbnail renderer
#     sprview-thumbnailer-win.dll           Explorer shell extension (win only)
#     install.ps1 / uninstall.ps1           shell extension registration (win only)
#     sprview_core.<dll|so|dylib>           C ABI shared library (sprview_core.h)
#     thumbnailer-linux/                    XDG integration files (linux only)
#     thumbnailer-macos/                    QuickLook extension sources (macos only)
$ErrorActionPreference = "Stop"

$rid = switch ($os)
{
    "windows" { "win-$arch" }
    "macos"   { "osx-$arch" }
    "ubuntu"  { "linux-$arch" }
    default   { throw "unknown os '$os'" }
}

$repo = Resolve-Path (Join-Path $PSScriptRoot "..")
$build = Join-Path $repo "build"
if (Test-Path -Path $build -PathType Container) {
    Remove-Item $build -Recurse -Force
}
New-Item -ItemType Directory -Path $build | Out-Null

foreach ($project in "SPRView.Net", "SPRView.Net.CLI", "SPRView.Net.Thumbnailer") {
    $projectDir = Join-Path $build $project
    dotnet publish (Join-Path $repo "src\$project\$project.csproj") -c Release -r $rid -o $projectDir
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Get-ChildItem -Path $projectDir -Exclude *.pdb | Move-Item -Destination $build -Force
    Remove-Item $projectDir -Recurse -Force
}

# C ABI shared library for native consumers (Swift, C/C++, ...).
& (Join-Path $PSScriptRoot "publish-native-core.ps1") -rid $rid
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$coreOut = Join-Path $build "native\$rid"
Copy-Item (Join-Path $coreOut "sprview_core.*") $build -Force

switch ($os)
{
    "windows" {
        $comDir = Join-Path $build "com-provider"
        dotnet publish (Join-Path $repo "src\SPRView.Net.ThumbnailProvider.Windows\SPRView.Net.ThumbnailProvider.Windows.csproj") -c Release -r $rid -o $comDir
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        Copy-Item (Join-Path $comDir "sprview-thumbnailer-win.dll") $build -Force
        Copy-Item (Join-Path $repo "platform\windows\install.ps1") $build -Force
        Copy-Item (Join-Path $repo "platform\windows\uninstall.ps1") $build -Force
        Remove-Item $comDir -Recurse -Force
    }
    "ubuntu" {
        $target = Join-Path $build "thumbnailer-linux"
        New-Item -ItemType Directory -Path $target | Out-Null
        Copy-Item (Join-Path $repo "platform\linux\*") $target -Force
    }
    "macos" {
        $target = Join-Path $build "thumbnailer-macos"
        New-Item -ItemType Directory -Path $target | Out-Null
        Copy-Item (Join-Path $repo "platform\macos\install.sh") $target -Force
        Copy-Item (Join-Path $repo "platform\macos\quicklook") $target -Recurse -Force
    }
}

Write-Host "Published NativeAOT deliverables to $build"

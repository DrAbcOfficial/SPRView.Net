param(
    [Parameter()]
    [String]$rid = "win-x64",
    [String]$outputDir = "build\native"
)

# Publishes SPRView.Net.Core as a NativeAOT shared library exporting the C ABI
# (see src/SPRView.Net.Core/Native/sprview_core.h).
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$coreProj = Join-Path $repoRoot "src\SPRView.Net.Core\SPRView.Net.Core.csproj"
$out = Join-Path (Join-Path $repoRoot $outputDir) $rid
New-Item -ItemType Directory -Force -Path $out | Out-Null

dotnet publish $coreProj -c Release -r $rid -p:NativeLib=Shared -p:PublishAot=true -p:SelfContained=true -o $out
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Normalize the exported library name per platform.
switch -Wildcard ($rid) {
    "win-*" {
        Rename-Item (Join-Path $out "SPRView.Net.Core.dll") "sprview_core.dll" -Force
        if (Test-Path (Join-Path $out "SPRView.Net.Core.pdb")) {
            Rename-Item (Join-Path $out "SPRView.Net.Core.pdb") "sprview_core.pdb" -Force
        }
    }
    "linux-*" {
        Rename-Item (Join-Path $out "SPRView.Net.Core.so") "sprview_core.so" -Force
    }
    "osx-*" {
        Rename-Item (Join-Path $out "SPRView.Net.Core.dylib") "sprview_core.dylib" -Force
    }
}

Write-Host "sprview_core published to $out"

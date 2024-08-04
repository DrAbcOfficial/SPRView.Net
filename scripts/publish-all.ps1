param(
    [Parameter()]
    [String]$arch = "x64",
    [String]$os = "windows"
)
$sharpos = ""
switch ($os)
{
    "windows" {$sharpos = "win"}
    "macos" {$sharpos = "osx"}
    "ubuntu" {$sharpos = "linux"}
}
$sharpr = @("$sharpos-$arch")

Set-Location ".."
$callparam = "-r", $sharpr, "-c", "Release", "-p:PublishReadyToRun=true", "-p:PublishSingleFile=false", "-o", ".\build"
$sharpproj = "SPRView.Net", "SPRView.Net.CLI"
if (Test-Path -Path ".\build" -PathType Container) {
    Remove-Item ".\build" -Force -Recurse
}
New-Item ".\build" -ItemType "directory"
foreach($proj in $sharpproj){
    Set-Location $proj
    &"dotnet" "publish" $callparam
    Copy-Item -Path @("build") -Destination ".." -Recurse -Exclude "*.pdb" -Force
    Set-Location ".."
}

switch ($os)
{
    "windows" {
        $ProgramFiles = [Environment]::GetEnvironmentVariable("ProgramFiles(x86)")
        $vsLocation= &@("$ProgramFiles\Microsoft Visual Studio\Installer\vswhere.exe") "-latest" "-products" "*" "-requires" "Microsoft.VisualStudio.Component.VC.Tools.x86.x64" "-property" "installationPath"

        &"nuget" "restore"
        $sharpproj = "SPRView.Net.Win32.Thumbnail"
        foreach($proj in $sharpproj){
            Set-Location $proj
            if(Test-Path("$($vsLocation)\Common7\Tools\vsdevcmd.bat")){
                &"$($vsLocation)\Common7\Tools\vsdevcmd.bat" "-arch=x64"
                &"$($vsLocation)\Msbuild\Current\Bin\MSBuild.exe" "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)/../SPRView.Net.Win32.Thumbnail/SPRView.Net.Win32.Thumbnail.vcxproj" /p:Configuration="Release" /p:Platform="x64"
            }
            Copy-Item -Path ".\x64\Release\SPRView.Net.Win32.Thumbnail.dll" -Destination "..\build" -Force
            Set-Location ".."
        }
    }
    "macos" {
        
    }
    "ubuntu" {

    }
}
param(
    [Parameter(Mandatory = $true)]
    [String]$DllPath
)

# Registers the NativeAOT thumbnail provider for the current user (HKCU),
# no elevation required. Equivalent of running: regsvr32 /n /i:user <dll>
$ErrorActionPreference = "Stop"

$dll = Resolve-Path $DllPath
$clsid = "{4D555153-67DE-4350-860D-671B7618B83B}"
$clsidKey = "HKCU:\Software\Classes\CLSID\$clsid"
$shellexKey = "HKCU:\Software\Classes\.spr\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}"

New-Item -Path $clsidKey -Force | Out-Null
Set-ItemProperty -Path $clsidKey -Name "(Default)" -Value "SPRView Thumbnail Preview"
New-Item -Path "$clsidKey\InProcServer32" -Force | Out-Null
Set-ItemProperty -Path "$clsidKey\InProcServer32" -Name "(Default)" -Value $dll
Set-ItemProperty -Path "$clsidKey\InProcServer32" -Name "ThreadingModel" -Value "Apartment"
New-Item -Path $shellexKey -Force | Out-Null
Set-ItemProperty -Path $shellexKey -Name "(Default)" -Value $clsid

# Tell the shell the .spr association changed so icons refresh.
$signature = @'
[DllImport("shell32.dll")]
public static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
'@
Add-Type -MemberDefinition $signature -Name Shell -Namespace SprView
[SprView.Shell]::SHChangeNotify(0x08000000, 0, [IntPtr]::Zero, [IntPtr]::Zero)

Write-Host "SPRView thumbnail provider registered for the current user:"
Write-Host "  $dll"

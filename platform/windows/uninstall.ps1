$ErrorActionPreference = "Continue"

# Removes the SPRView thumbnail provider registration (HKCU only).
$clsid = "{4D555153-67DE-4350-860D-671B7618B83B}"
Remove-Item -Path "HKCU:\Software\Classes\CLSID\$clsid" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "HKCU:\Software\Classes\.spr\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" -Recurse -Force -ErrorAction SilentlyContinue

$signature = @'
[DllImport("shell32.dll")]
public static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
'@
Add-Type -MemberDefinition $signature -Name Shell -Namespace SprView
[SprView.Shell]::SHChangeNotify(0x08000000, 0, [IntPtr]::Zero, [IntPtr]::Zero)

Write-Host "SPRView thumbnail provider unregistered."

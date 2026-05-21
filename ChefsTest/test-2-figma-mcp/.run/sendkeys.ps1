param([string]$Keys, [string]$ProcName = "ChefsTest2")

$proc = Get-Process | Where-Object { $_.ProcessName -eq $ProcName } | Select-Object -First 1
if ($null -eq $proc) { Write-Error "Process not found"; exit 1 }
$h = $proc.MainWindowHandle

Add-Type @"
using System;
using System.Runtime.InteropServices;
public class W32S {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr hWnd);
}
"@

[W32S]::ShowWindowAsync($h, 9) | Out-Null
[W32S]::BringWindowToTop($h) | Out-Null
[W32S]::SetForegroundWindow($h) | Out-Null
Start-Sleep -Milliseconds 250

[System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms") | Out-Null
[System.Windows.Forms.SendKeys]::SendWait($Keys)
Start-Sleep -Milliseconds 300
Write-Output "sent: $Keys"

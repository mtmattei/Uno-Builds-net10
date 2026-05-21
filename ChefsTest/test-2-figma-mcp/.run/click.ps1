param([int]$X, [int]$Y, [string]$ProcName = "ChefsTest2")

Add-Type -AssemblyName System.Windows.Forms
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class W32C {
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll", SetLastError=true)] public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] public struct MOUSEINPUT {
        public int dx, dy; public uint mouseData, dwFlags, time; public IntPtr dwExtraInfo;
    }
    [StructLayout(LayoutKind.Explicit)] public struct INPUT_UNION {
        [FieldOffset(0)] public MOUSEINPUT mi;
    }
    [StructLayout(LayoutKind.Sequential)] public struct INPUT {
        public uint type; public INPUT_UNION u;
    }
}
"@

$proc = Get-Process | Where-Object { $_.ProcessName -eq $ProcName } | Select-Object -First 1
if ($null -eq $proc) { Write-Error "Process not found"; exit 1 }
$h = $proc.MainWindowHandle
[W32C]::ShowWindowAsync($h, 9) | Out-Null
[W32C]::BringWindowToTop($h) | Out-Null
[W32C]::SetForegroundWindow($h) | Out-Null
Start-Sleep -Milliseconds 200

[W32C+RECT]$r = New-Object W32C+RECT
[W32C]::GetWindowRect($h, [ref]$r) | Out-Null

$absX = $r.Left + $X
$absY = $r.Top + $Y
[System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point($absX, $absY)
Start-Sleep -Milliseconds 100

# Mouse left down + up via SendInput
$DOWN = 0x0002
$UP   = 0x0004
$inputs = @()
$d = New-Object W32C+INPUT
$d.type = 0
$mi = New-Object W32C+MOUSEINPUT
$mi.dwFlags = $DOWN
$d.u = New-Object W32C+INPUT_UNION
$d.u.mi = $mi
$inputs += $d
$u = New-Object W32C+INPUT
$u.type = 0
$umi = New-Object W32C+MOUSEINPUT
$umi.dwFlags = $UP
$u.u = New-Object W32C+INPUT_UNION
$u.u.mi = $umi
$inputs += $u
$null = [W32C]::SendInput([uint32]$inputs.Length, $inputs, [System.Runtime.InteropServices.Marshal]::SizeOf([type][W32C+INPUT]))

Start-Sleep -Milliseconds 200
Write-Output "clicked $X,$Y -> abs $absX,$absY"

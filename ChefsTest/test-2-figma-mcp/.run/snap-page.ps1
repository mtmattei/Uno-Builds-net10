param([string]$Route, [string]$Out, [int]$WarmupMs = 5000)

# Kill any prior ChefsTest2 process
Get-Process -Name "ChefsTest2" -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
Start-Sleep -Milliseconds 500

$env:CHEFSTEST2_INITIAL_ROUTE = $Route
$exe = "C:\Users\Platform006\OneDrive - Uno Platform\Desktop\unOS\AI-builds\ChefsFT-test\test-2-figma-mcp\ChefsTest2\bin\Debug\net10.0-desktop\ChefsTest2.exe"
if (-not (Test-Path $exe)) { Write-Error "Exe not found: $exe"; exit 1 }

$p = Start-Process -FilePath $exe -PassThru
Start-Sleep -Milliseconds $WarmupMs

# Capture screenshot via existing snap.ps1
& "C:\Users\Platform006\OneDrive - Uno Platform\Desktop\unOS\AI-builds\ChefsFT-test\test-2-figma-mcp\.run\snap.ps1" -OutPath $Out
Start-Sleep -Milliseconds 200
Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
Write-Output "captured route=$Route -> $Out"

# Batch 5: final tractable fixes
# - Wellmetrix at correct triple-nested path, strip -windows

$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo
$logFile = "$repo\UPGRADE-LOG.md"
$blockedFile = "$repo\BLOCKED.md"
$iosTfmsToStrip = @('net10.0-ios','net10.0-iossimulator','net9.0-ios','net9.0-iossimulator','net10.0-maccatalyst','net9.0-maccatalyst')

function Normalize-TFMs {
  param([string]$csproj, [string[]]$AlsoStrip = @())
  $t = Get-Content $csproj -Raw
  $m = [regex]::Match($t, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
  if (-not $m.Success) { return $false }
  $old = $m.Groups[1].Value
  $tokens = ($old -split ';') | ForEach-Object { $_.Trim() } | Where-Object { $_ }
  $kept = @()
  foreach ($tok in $tokens) {
    if ($iosTfmsToStrip -contains $tok) { continue }
    if ($AlsoStrip -contains $tok) { continue }
    if ($tok -match '^net9\.0(.*)$') { $kept += "net10.0$($Matches[1])" } else { $kept += $tok }
  }
  $new = $kept -join ';'
  if ($new -eq $old) { return $false }
  $newTag = "<TargetFrameworks>$new</TargetFrameworks>"
  $t2 = $t -replace [regex]::Escape($m.Value), $newTag
  Set-Content -Path $csproj -Value $t2 -NoNewline
  Write-Host "    TFMs: $old -> $new" -ForegroundColor Yellow
  return $true
}

$name = 'Wellmetrix'
$csprojFull = "$repo\Wellmetrix\Wellmetrix\Wellmetrix\Wellmetrix.csproj"
$projDir = Split-Path -Parent $csprojFull
$base = "$repo\$name"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Fix5 :: $name" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

$gjFiles = Get-ChildItem -Path $base -Filter global.json -Recurse -ErrorAction SilentlyContinue
$current = ([regex]::Match((Get-Content $gjFiles[0].FullName -Raw), '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
Write-Host "  current: $current"

Normalize-TFMs -csproj $csprojFull -AlsoStrip @('net10.0-windows10.0.26100') | Out-Null

foreach ($gjf in $gjFiles) {
  $t = Get-Content $gjf.FullName -Raw
  $cur = ([regex]::Match($t, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  if ($cur -ne '6.5.33') {
    $n = $t -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
    Set-Content -Path $gjf.FullName -Value $n -NoNewline
  }
}

$csText = Get-Content $csprojFull -Raw
$m = [regex]::Match($csText, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
$tfms = ($m.Groups[1].Value -split ';') | ForEach-Object { $_.Trim() } | Where-Object { $_ }
Write-Host "  TFMs to build: $($tfms -join ', ')"

Push-Location $projDir
$results = @()
$failTfm = $null; $failDetail = $null
try {
  Write-Host "  dotnet restore..." -ForegroundColor Gray
  $out = & dotnet restore $csprojFull 2>&1
  if ($LASTEXITCODE -ne 0) {
    $failTfm = 'restore'; $failDetail = ($out | Select-Object -Last 20 | ForEach-Object { $_.ToString() }) -join ' / '
  } else {
    foreach ($tfm in $tfms) {
      Write-Host "  dotnet build -f $tfm..." -ForegroundColor Gray
      $out = & dotnet build $csprojFull -f $tfm --no-restore --nologo 2>&1
      if ($LASTEXITCODE -ne 0) {
        $failTfm = $tfm; $failDetail = ($out | Select-Object -Last 25 | ForEach-Object { $_.ToString() }) -join ' / '
        break
      }
      Write-Host "  OK ($tfm)" -ForegroundColor Green
      $results += "$tfm`:ok"
    }
  }
}
finally { Pop-Location }

Set-Location $repo

if ($failTfm) {
  & git checkout -- .
  Write-Host "  STILL BLOCKED ($failTfm)" -ForegroundColor Red
  $d = ($failDetail -replace '\|','/' -replace '`','').Substring(0, [Math]::Min(300, $failDetail.Length))
  Add-Content $blockedFile "| fix5 | $name | $current / $failTfm | build failed | $d |"
  Add-Content $logFile "| fix5 | $name | $current | 6.5.33 | STILL BLOCKED ($failTfm) | - |"
} else {
  & git add $csprojFull
  foreach ($gjf in $gjFiles) { & git add $gjf.FullName }
  & git checkout -- .
  $builds = $results -join ', '
  $msg = "fix($name): bump to Uno.Sdk 6.5.33 + strip -windows TFM`n`nWas $current. Windows TFM stripped due to MSB3073 XamlCompiler interop failure (same root cause as SantaTracker).`nbuilds: $builds"
  & git commit -m $msg | Out-Null
  $sha = (& git rev-parse --short HEAD).Trim()
  Write-Host "  committed $sha" -ForegroundColor Green
  Add-Content $logFile "| fix5 | $name | $current | 6.5.33 | ok ($builds) | $sha |"
}

Get-ChildItem -Path $base -Directory -Recurse -Force -ErrorAction SilentlyContinue |
  Where-Object { $_.Name -in 'bin','obj' } |
  ForEach-Object { try { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue } catch {} }

Write-Host "Disk: $((Get-PSDrive C).Free / 1GB) GB"
& git log --oneline -10

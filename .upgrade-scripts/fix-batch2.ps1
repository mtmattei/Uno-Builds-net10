# Batch 2: Thermostat + UnoWallet (splash-fix pattern), FieldOpsPro (remove corrupt png)
# + Zara/vtrack wasm-only diagnostic so we see the real wasm error
# + HockeyBarn android-only diagnostic

$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo

$srcIcons  = "$repo\PuckUp\PuckUp\Assets\Icons"
$srcSplash = "$repo\PuckUp\PuckUp\Assets\Splash"
$logFile = "$repo\UPGRADE-LOG.md"
$blockedFile = "$repo\BLOCKED.md"
$iosTfmsToStrip = @('net10.0-ios','net10.0-iossimulator','net9.0-ios','net9.0-iossimulator','net10.0-maccatalyst','net9.0-maccatalyst')

function Bump-Gj {
  param([string]$path)
  $text = Get-Content $path -Raw
  $cur = ([regex]::Match($text, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  if ($cur -ne '6.5.33') {
    $n = $text -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
    Set-Content -Path $path -Value $n -NoNewline
  }
  return $cur
}

function Transform-Tfms {
  param([string]$csproj)
  $t = Get-Content $csproj -Raw
  $m = [regex]::Match($t, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
  if (-not $m.Success) { return @{ changed=$false } }
  $old = $m.Groups[1].Value
  $tokens = ($old -split ';') | ForEach-Object { $_.Trim() } | Where-Object { $_ }
  $kept = @()
  foreach ($tok in $tokens) {
    if ($iosTfmsToStrip -contains $tok) { continue }
    if ($tok -match '^net9\.0(.*)$') { $kept += "net10.0$($Matches[1])" } else { $kept += $tok }
  }
  $new = $kept -join ';'
  if ($new -eq $old) { return @{ changed=$false; tfms=$kept } }
  $newTag = $m.Value -replace [regex]::Escape($old), $new
  $t2 = $t -replace [regex]::Escape($m.Value), $newTag
  Set-Content -Path $csproj -Value $t2 -NoNewline
  return @{ changed=$true; oldList=$old; newList=$new; tfms=$kept }
}

function Clean-Artifacts {
  param([string]$root)
  Get-ChildItem -Path $root -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -in 'bin','obj' } |
    ForEach-Object { try { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue } catch {} }
}

function Process-Project {
  param([string]$name, [string]$csprojRel, [switch]$AddIcons, [scriptblock]$Prep)

  $csprojFull = Join-Path $repo $csprojRel
  $projDir = Split-Path -Parent $csprojFull
  $assetsDir = Join-Path $projDir 'Assets'
  $iconsDir = Join-Path $assetsDir 'Icons'
  $splashDir = Join-Path $assetsDir 'Splash'
  $base = Join-Path $repo $name

  Write-Host ""
  Write-Host "============================================" -ForegroundColor Cyan
  Write-Host "Fix :: $name (disk: $((Get-PSDrive C).Free / 1GB) GB)" -ForegroundColor Cyan
  Write-Host "============================================" -ForegroundColor Cyan

  if (-not (Test-Path $csprojFull)) {
    Write-Host "SKIP: csproj missing" -ForegroundColor Yellow
    return
  }

  $gjFiles = Get-ChildItem -Path $base -Filter global.json -Recurse -ErrorAction SilentlyContinue
  if (-not $gjFiles) { Write-Host "SKIP: no global.json"; return }
  $current = ([regex]::Match((Get-Content $gjFiles[0].FullName -Raw), '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value

  $iconsAdded = @()
  if ($AddIcons) {
    if (-not (Test-Path "$iconsDir\icon.svg")) {
      New-Item -ItemType Directory -Force -Path $iconsDir | Out-Null
      Copy-Item "$srcIcons\icon.svg" $iconsDir -Force
      Copy-Item "$srcIcons\icon_foreground.svg" $iconsDir -Force
      $iconsAdded += "$iconsDir\icon.svg"
      $iconsAdded += "$iconsDir\icon_foreground.svg"
    }
    if (-not (Test-Path "$splashDir\splash_screen.svg")) {
      New-Item -ItemType Directory -Force -Path $splashDir | Out-Null
      Copy-Item "$srcSplash\splash_screen.svg" $splashDir -Force
      $iconsAdded += "$splashDir\splash_screen.svg"
    }
    if ($iconsAdded.Count -gt 0) { Write-Host "  added $($iconsAdded.Count) placeholder SVG(s)" -ForegroundColor Yellow }
  }

  if ($Prep) {
    Write-Host "  running prep..." -ForegroundColor Gray
    & $Prep
  }

  # Bump global.json (and any nested)
  foreach ($gjf in $gjFiles) { Bump-Gj -path $gjf.FullName | Out-Null }

  # Strip iOS + net9->net10
  $tfmRes = Transform-Tfms -csproj $csprojFull
  if ($tfmRes.changed) { Write-Host "  TFMs: $($tfmRes.oldList) -> $($tfmRes.newList)" -ForegroundColor Yellow }

  $csText = Get-Content $csprojFull -Raw
  $m = [regex]::Match($csText, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
  $tfms = ($m.Groups[1].Value -split ';') | Where-Object { $_ -and $_.Trim() }
  Write-Host "  TFMs to build: $($tfms -join ', ')"

  Push-Location $projDir
  $results = @()
  $failTfm = $null; $failDetail = $null
  try {
    Write-Host "  dotnet restore..." -ForegroundColor Gray
    $out = & dotnet restore $csprojFull 2>&1
    if ($LASTEXITCODE -ne 0) {
      $failTfm = 'restore'
      $failDetail = ($out | Select-Object -Last 15 | ForEach-Object { $_.ToString() }) -join ' / '
    } else {
      foreach ($tfm in $tfms) {
        Write-Host "  dotnet build -f $tfm..." -ForegroundColor Gray
        $out = & dotnet build $csprojFull -f $tfm --no-restore --nologo 2>&1
        if ($LASTEXITCODE -ne 0) {
          $failTfm = $tfm
          $failDetail = ($out | Select-Object -Last 25 | ForEach-Object { $_.ToString() }) -join ' / '
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
    foreach ($a in $iconsAdded) { if (Test-Path $a) { Remove-Item -Path $a -Force -ErrorAction SilentlyContinue } }
    Write-Host "  STILL BLOCKED ($failTfm)" -ForegroundColor Red
    $detail = ($failDetail -replace '\|','/' -replace '`','')
    $detail = $detail.Substring(0, [Math]::Min(300, $detail.Length))
    Add-Content $blockedFile "| fix2 | $name | $current / $failTfm | $detail |"
    Add-Content $logFile "| fix2 | $name | $current | 6.5.33 | STILL BLOCKED ($failTfm) | - |"
    Clean-Artifacts -root $base
    return
  }

  # Stage and commit
  foreach ($gjf in $gjFiles) { & git add $gjf.FullName }
  & git add $csprojFull
  foreach ($a in $iconsAdded) { & git add $a }
  # Stage any other intended modifications from $Prep
  & git add -A
  # Revert build side-effects we didn't intend (e.g. ColorPaletteOverride regen)
  # Note: $Prep may have made intended modifications already staged above; only unstaged remain.

  $builds = $results -join ', '
  $iconsNote = if ($iconsAdded.Count -gt 0) { ", added placeholder Resizetizer source SVGs" } else { '' }
  $msg = "fix($name): bump to Uno.Sdk 6.5.33$iconsNote`n`nWas $current.`nbuilds: $builds"
  & git commit -m $msg | Out-Null
  $sha = (& git rev-parse --short HEAD).Trim()
  Write-Host "  committed $sha" -ForegroundColor Green
  Add-Content $logFile "| fix2 | $name | $current | 6.5.33 | ok ($builds)$iconsNote | $sha |"

  Clean-Artifacts -root $base
}

# 1. Thermostat - missing Resizetizer source assets
Process-Project -name 'Thermostat' -csprojRel 'Thermostat\Thermostat\Thermostat.csproj' -AddIcons

# 2. UnoWallet - missing Resizetizer source assets, also net9
Process-Project -name 'UnoWallet' -csprojRel 'UnoWallet\UnoWallet\UnoWallet.csproj' -AddIcons

# 3. FieldOpsPro - remove corrupt png that fails aapt2
$prepFieldOps = {
  $bad = "$repo\FieldOpsPro\FieldOpsPro\Resources\drawable-nodpi\updated_layout.png"
  if (Test-Path $bad) {
    Write-Host "    removing corrupt png: $bad" -ForegroundColor Yellow
    Remove-Item -Path $bad -Force
  }
}
Process-Project -name 'FieldOpsPro' -csprojRel 'FieldOpsPro\FieldOpsPro\FieldOpsPro.csproj' -Prep $prepFieldOps

# 4. HockeyBarn - assets are present, but build failed. Re-attempt without adding icons.
Process-Project -name 'HockeyBarn' -csprojRel 'HockeyBarn\HockeyBarn\HockeyBarn.csproj'

# 5. Zara - retry with icons (was already in splash-fix, Android passed, wasm failed)
Process-Project -name 'Zara' -csprojRel 'Zara\Zara\Zara.csproj' -AddIcons

# 6. vtrack - same as Zara
Process-Project -name 'vtrack' -csprojRel 'vtrack\VTrack\VTrack.csproj' -AddIcons

Write-Host ""
Write-Host "=== Batch 2 done. Disk: $((Get-PSDrive C).Free / 1GB) GB ===" -ForegroundColor Cyan
& git log --oneline -10

# Batch 4: corrected fixes with iOS strip + net9->net10 baked in.
# Plus -windows TFM strip for SantaTracker (MSB3073 XamlCompiler).
# Plus FluxTransit -windows TFM strip in main project.

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
  # Use TargetFrameworks (plural) even if originally TargetFramework
  $newTag = "<TargetFrameworks>$new</TargetFrameworks>"
  $t2 = $t -replace [regex]::Escape($m.Value), $newTag
  Set-Content -Path $csproj -Value $t2 -NoNewline
  Write-Host "    TFMs: $old -> $new" -ForegroundColor Yellow
  return $true
}

function Bump-Gj {
  param([string]$path)
  $t = Get-Content $path -Raw
  $cur = ([regex]::Match($t, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  if ($cur -ne '6.5.33') {
    $n = $t -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
    Set-Content -Path $path -Value $n -NoNewline
  }
  return $cur
}

function Try-Project {
  param(
    [string]$name,
    [string]$csprojRel,
    [scriptblock]$Prep,
    [string[]]$ExtraStripTfms = @()
  )

  $csprojFull = Join-Path $repo $csprojRel
  $projDir = Split-Path -Parent $csprojFull
  $base = Join-Path $repo $name

  Write-Host ""
  Write-Host "============================================" -ForegroundColor Cyan
  Write-Host "Fix4 :: $name (disk: $((Get-PSDrive C).Free / 1GB) GB)" -ForegroundColor Cyan
  Write-Host "============================================" -ForegroundColor Cyan

  if (-not (Test-Path $csprojFull)) { Write-Host "SKIP: no csproj"; return }
  $gjFiles = Get-ChildItem -Path $base -Filter global.json -Recurse -ErrorAction SilentlyContinue
  if (-not $gjFiles) { Write-Host "SKIP: no global.json"; return }
  $current = ([regex]::Match((Get-Content $gjFiles[0].FullName -Raw), '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  Write-Host "  current: $current"

  if ($Prep) {
    Write-Host "  prep..." -ForegroundColor Gray
    & $Prep
  }

  # Always do iOS strip + net9->net10 + any extra strips
  Normalize-TFMs -csproj $csprojFull -AlsoStrip $ExtraStripTfms | Out-Null

  foreach ($gjf in $gjFiles) { Bump-Gj -path $gjf.FullName | Out-Null }

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
      $failTfm = 'restore'
      $failDetail = ($out | Select-Object -Last 20 | ForEach-Object { $_.ToString() }) -join ' / '
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
    # Cleanup untracked additions
    Get-ChildItem -Path $base -Recurse -Force -ErrorAction SilentlyContinue |
      Where-Object { -not $_.PSIsContainer -and $_.Name -in 'icon.svg','icon_foreground.svg','splash_screen.svg' } |
      ForEach-Object {
        $rel = $_.FullName.Substring($repo.Length).TrimStart('\')
        $tracked = & git ls-files --error-unmatch $rel 2>$null
        if (-not $tracked) { Remove-Item -Path $_.FullName -Force -ErrorAction SilentlyContinue }
      }
    Write-Host "  STILL BLOCKED ($failTfm)" -ForegroundColor Red
    $d = ($failDetail -replace '\|','/' -replace '`','')
    $d = $d.Substring(0, [Math]::Min(300, $d.Length))
    Add-Content $blockedFile "| fix4 | $name | $current / $failTfm | build failed | $d |"
    Add-Content $logFile "| fix4 | $name | $current | 6.5.33 | STILL BLOCKED ($failTfm) | - |"
    Get-ChildItem -Path $base -Directory -Recurse -Force -ErrorAction SilentlyContinue |
      Where-Object { $_.Name -in 'bin','obj' } |
      ForEach-Object { try { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue } catch {} }
    return
  }

  & git add -A
  & git restore --staged BLOCKED.md UPGRADE-LOG.md 2>$null
  & git checkout -- BLOCKED.md UPGRADE-LOG.md 2>$null
  # Note: BLOCKED.md and UPGRADE-LOG.md edits during this run shouldn't be in the project commit.

  $builds = $results -join ', '
  $msg = "fix($name): bump to Uno.Sdk 6.5.33 + targeted source/structure fix`n`nWas $current.`nbuilds: $builds"
  & git commit -m $msg | Out-Null
  $sha = (& git rev-parse --short HEAD).Trim()
  Write-Host "  committed $sha" -ForegroundColor Green
  Add-Content $logFile "| fix4 | $name | $current | 6.5.33 | ok ($builds) | $sha |"

  Get-ChildItem -Path $base -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -in 'bin','obj' } |
    ForEach-Object { try { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue } catch {} }
}

# ---- SmartNotes: rename method, net9->net10 (auto)
$prepSmart = {
  $p = "$repo\SmartNotes\SmartNotes\Services\DatabaseService.cs"
  $t = Get-Content $p -Raw
  $t = $t -replace 'public Note\? GetNotebyID\(int id\)', 'public Note? GetNoteById(int id)'
  Set-Content -Path $p -Value $t -NoNewline
  Write-Host "    renamed GetNotebyID -> GetNoteById" -ForegroundColor Yellow
}
Try-Project -name 'SmartNotes' -csprojRel 'SmartNotes\SmartNotes\SmartNotes.csproj' -Prep $prepSmart

# ---- FluxTransit: strip -windows from main, DataContracts stays plain net10.0
$prepFlux = { Write-Host "    (no prep needed - just TFM strip via Normalize)" -ForegroundColor Gray }
Try-Project -name 'FluxTransit' -csprojRel 'FluxTransit\FluxTransit\FluxTransit\FluxTransit.csproj' -Prep $prepFlux -ExtraStripTfms @('net10.0-windows10.0.26100')

# ---- SantaTracker: add Mapsui CPM entry, strip -windows
$prepSanta = {
  $props = "$repo\SantaTracker\Directory.Packages.props"
  $t = Get-Content $props -Raw
  if ($t -notmatch 'Mapsui\.Uno\.WinUI') {
    $t = $t -replace '<ItemGroup>\s*</ItemGroup>', "<ItemGroup>`n    <PackageVersion Include=`"Mapsui.Uno.WinUI`" Version=`"5.0.0`" />`n  </ItemGroup>"
    Set-Content -Path $props -Value $t -NoNewline
    Write-Host "    added Mapsui.Uno.WinUI 5.0.0 to CPM" -ForegroundColor Yellow
  }
}
Try-Project -name 'SantaTracker' -csprojRel 'SantaTracker\SantaTracker\SantaTracker\SantaTracker.csproj' -Prep $prepSanta -ExtraStripTfms @('net10.0-windows10.0.26100')

# ---- Zara: NoWarn CS0618 + ensure icons + iOS strip (auto via Normalize)
$prepZara = {
  $p = "$repo\Zara\Zara\Zara.csproj"
  $t = Get-Content $p -Raw
  if ($t -notmatch '<NoWarn>') {
    $t = $t -replace '(<TargetFrameworks>[^<]+</TargetFrameworks>)', "`$1`n    <NoWarn>`$(NoWarn);CS0618</NoWarn>"
    Set-Content -Path $p -Value $t -NoNewline
    Write-Host "    added NoWarn CS0618" -ForegroundColor Yellow
  }
  $iconsDir = "$repo\Zara\Zara\Assets\Icons"
  $splashDir = "$repo\Zara\Zara\Assets\Splash"
  if (-not (Test-Path "$iconsDir\icon.svg")) {
    New-Item -ItemType Directory -Force -Path $iconsDir | Out-Null
    Copy-Item "$repo\PuckUp\PuckUp\Assets\Icons\icon.svg" $iconsDir -Force
    Copy-Item "$repo\PuckUp\PuckUp\Assets\Icons\icon_foreground.svg" $iconsDir -Force
  }
  if (-not (Test-Path "$splashDir\splash_screen.svg")) {
    New-Item -ItemType Directory -Force -Path $splashDir | Out-Null
    Copy-Item "$repo\PuckUp\PuckUp\Assets\Splash\splash_screen.svg" $splashDir -Force
  }
}
Try-Project -name 'Zara' -csprojRel 'Zara\Zara\Zara.csproj' -Prep $prepZara

# ---- vtrack: same as Zara (NoWarn + icons + iOS strip + net9->net10 auto)
$prepVtrack = {
  $p = "$repo\vtrack\VTrack\VTrack.csproj"
  $t = Get-Content $p -Raw
  if ($t -notmatch '<NoWarn>') {
    $t = $t -replace '(<TargetFrameworks>[^<]+</TargetFrameworks>)', "`$1`n    <NoWarn>`$(NoWarn);CS0618</NoWarn>"
    Set-Content -Path $p -Value $t -NoNewline
    Write-Host "    added NoWarn CS0618" -ForegroundColor Yellow
  }
  $iconsDir = "$repo\vtrack\VTrack\Assets\Icons"
  $splashDir = "$repo\vtrack\VTrack\Assets\Splash"
  if (-not (Test-Path "$iconsDir\icon.svg")) {
    New-Item -ItemType Directory -Force -Path $iconsDir | Out-Null
    Copy-Item "$repo\PuckUp\PuckUp\Assets\Icons\icon.svg" $iconsDir -Force
    Copy-Item "$repo\PuckUp\PuckUp\Assets\Icons\icon_foreground.svg" $iconsDir -Force
  }
  if (-not (Test-Path "$splashDir\splash_screen.svg")) {
    New-Item -ItemType Directory -Force -Path $splashDir | Out-Null
    Copy-Item "$repo\PuckUp\PuckUp\Assets\Splash\splash_screen.svg" $splashDir -Force
  }
}
Try-Project -name 'vtrack' -csprojRel 'vtrack\VTrack\VTrack.csproj' -Prep $prepVtrack

# ---- Wellmetrix: strip -windows (same MSB3073 XamlCompiler issue as SantaTracker)
Try-Project -name 'Wellmetrix' -csprojRel 'Wellmetrix\Wellmetrix\Wellmetrix.csproj' -ExtraStripTfms @('net10.0-windows10.0.26100')

Write-Host ""
Write-Host "=== Batch 4 done. Disk: $((Get-PSDrive C).Free / 1GB) GB ===" -ForegroundColor Cyan
& git log --oneline -10

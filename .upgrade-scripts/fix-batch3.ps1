# Batch 3: source/structure fixes
# 1. SmartNotes  - rename DatabaseService.GetNotebyID -> GetNoteById (casing fix)
# 2. FluxTransit - add net10.0-windows10.0.26100 to DataContracts TFMs
# 3. QuoteCraft  - bump SkiaSharp pin 3.119.1 -> 3.119.2 (resolve NU1605 downgrade)
# 4. Gridform    - strip net10.0-windows TFM (SKCanvasElement signature mismatch on WinAppSDK)
# 5. SantaTracker - add Mapsui.Uno.WinUI PackageVersion to CPM
# 6. Zara        - add NoWarn CS0618 (SKPaint.TextSize obsolete-as-error on wasm)
# 7. vtrack      - same as Zara

$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo
$logFile = "$repo\UPGRADE-LOG.md"
$blockedFile = "$repo\BLOCKED.md"

function Try-Project {
  param([string]$name, [string]$csprojRel, [scriptblock]$Prep, [string[]]$ExtraStageFiles = @())

  $csprojFull = Join-Path $repo $csprojRel
  $projDir = Split-Path -Parent $csprojFull
  $base = Join-Path $repo $name

  Write-Host ""
  Write-Host "============================================" -ForegroundColor Cyan
  Write-Host "Fix3 :: $name (disk: $((Get-PSDrive C).Free / 1GB) GB)" -ForegroundColor Cyan
  Write-Host "============================================" -ForegroundColor Cyan

  if (-not (Test-Path $csprojFull)) { Write-Host "SKIP: no csproj at $csprojFull"; return }

  $gjFiles = Get-ChildItem -Path $base -Filter global.json -Recurse -ErrorAction SilentlyContinue
  if (-not $gjFiles) { Write-Host "SKIP: no global.json"; return }
  $current = ([regex]::Match((Get-Content $gjFiles[0].FullName -Raw), '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value

  # Run the prep (the source/csproj/CPM fix)
  if ($Prep) {
    Write-Host "  running prep..." -ForegroundColor Gray
    & $Prep
  }

  # Bump global.json (and nested)
  foreach ($gjf in $gjFiles) {
    $t = Get-Content $gjf.FullName -Raw
    $cur = ([regex]::Match($t, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
    if ($cur -ne '6.5.33') {
      $n = $t -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
      Set-Content -Path $gjf.FullName -Value $n -NoNewline
    }
  }

  # Re-read TFMs from csproj (some preps modify it)
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
          $failDetail = ($out | Select-Object -Last 30 | ForEach-Object { $_.ToString() }) -join ' / '
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
    $d = ($failDetail -replace '\|','/' -replace '`','')
    $d = $d.Substring(0, [Math]::Min(300, $d.Length))
    Add-Content $blockedFile "| fix3 | $name | $current / $failTfm | $d |"
    Add-Content $logFile "| fix3 | $name | $current | 6.5.33 | STILL BLOCKED ($failTfm) | - |"
    Get-ChildItem -Path $base -Directory -Recurse -Force -ErrorAction SilentlyContinue |
      Where-Object { $_.Name -in 'bin','obj' } |
      ForEach-Object { try { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue } catch {} }
    return
  }

  foreach ($gjf in $gjFiles) { & git add $gjf.FullName }
  & git add $csprojFull
  foreach ($f in $ExtraStageFiles) { & git add $f }
  & git add -A  # catch any other intended edits
  & git checkout -- .  # revert build side effects (e.g. ColorPaletteOverride regen)

  $builds = $results -join ', '
  $msg = "fix($name): bump to Uno.Sdk 6.5.33 + targeted source/structure fix`n`nWas $current.`nbuilds: $builds"
  & git commit -m $msg | Out-Null
  $sha = (& git rev-parse --short HEAD).Trim()
  Write-Host "  committed $sha" -ForegroundColor Green
  Add-Content $logFile "| fix3 | $name | $current | 6.5.33 | ok ($builds) | $sha |"

  Get-ChildItem -Path $base -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -in 'bin','obj' } |
    ForEach-Object { try { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue } catch {} }
}

# ---- SmartNotes: rename method casing
$prepSmartNotes = {
  $p = "$repo\SmartNotes\SmartNotes\Services\DatabaseService.cs"
  $t = Get-Content $p -Raw
  $t = $t -replace 'public Note\? GetNotebyID\(int id\)', 'public Note? GetNoteById(int id)'
  Set-Content -Path $p -Value $t -NoNewline
  Write-Host "    renamed GetNotebyID -> GetNoteById" -ForegroundColor Yellow
}
Try-Project -name 'SmartNotes' -csprojRel 'SmartNotes\SmartNotes\SmartNotes.csproj' -Prep $prepSmartNotes -ExtraStageFiles @("$repo\SmartNotes\SmartNotes\Services\DatabaseService.cs")

# ---- FluxTransit: add net10.0-windows10.0.26100 to DataContracts
$prepFlux = {
  $p = "$repo\FluxTransit\FluxTransit\FluxTransit.DataContracts\FluxTransit.DataContracts.csproj"
  $t = Get-Content $p -Raw
  $t = $t -replace '<TargetFramework>net10\.0</TargetFramework>', '<TargetFrameworks>net10.0;net10.0-windows10.0.26100;net10.0-browserwasm;net10.0-desktop</TargetFrameworks>'
  Set-Content -Path $p -Value $t -NoNewline
  Write-Host "    expanded DataContracts TFMs" -ForegroundColor Yellow
}
Try-Project -name 'FluxTransit' -csprojRel 'FluxTransit\FluxTransit\FluxTransit\FluxTransit.csproj' -Prep $prepFlux -ExtraStageFiles @("$repo\FluxTransit\FluxTransit\FluxTransit.DataContracts\FluxTransit.DataContracts.csproj")

# ---- QuoteCraft: bump SkiaSharp pin to 3.119.2
$prepQuote = {
  $p = "$repo\QuoteCraft\src\Directory.Packages.props"
  $t = Get-Content $p -Raw
  $t = $t -replace 'PackageVersion Include="SkiaSharp" Version="3\.119\.1"', 'PackageVersion Include="SkiaSharp" Version="3.119.2"'
  Set-Content -Path $p -Value $t -NoNewline
  Write-Host "    bumped SkiaSharp pin 3.119.1 -> 3.119.2" -ForegroundColor Yellow
}
Try-Project -name 'QuoteCraft' -csprojRel 'QuoteCraft\src\QuoteCraft\QuoteCraft.csproj' -Prep $prepQuote -ExtraStageFiles @("$repo\QuoteCraft\src\Directory.Packages.props")

# ---- Gridform: strip -windows TFM (SKCanvasElement incompatible with WinAppSDK)
$prepGrid = {
  $p = "$repo\Gridform\GridForm\GridForm\GridForm.csproj"
  $t = Get-Content $p -Raw
  $m = [regex]::Match($t, '<TargetFrameworks>([^<]+)</TargetFrameworks>')
  if ($m.Success) {
    $old = $m.Groups[1].Value
    $kept = ($old -split ';') | Where-Object { $_ -notmatch '^net10\.0-windows' }
    $new = $kept -join ';'
    if ($new -ne $old) {
      $t2 = $t -replace [regex]::Escape($m.Value), "<TargetFrameworks>$new</TargetFrameworks>"
      Set-Content -Path $p -Value $t2 -NoNewline
      Write-Host "    stripped -windows TFM: $old -> $new" -ForegroundColor Yellow
    }
  }
}
Try-Project -name 'Gridform' -csprojRel 'Gridform\GridForm\GridForm\GridForm.csproj' -Prep $prepGrid

# ---- SantaTracker: add Mapsui.Uno.WinUI PackageVersion to CPM
$prepSanta = {
  $p = "$repo\SantaTracker\Directory.Packages.props"
  $t = Get-Content $p -Raw
  if ($t -notmatch 'Mapsui\.Uno\.WinUI') {
    $t = $t -replace '<ItemGroup>\s*</ItemGroup>', "<ItemGroup>`n    <PackageVersion Include=`"Mapsui.Uno.WinUI`" Version=`"5.0.0`" />`n  </ItemGroup>"
    Set-Content -Path $p -Value $t -NoNewline
    Write-Host "    added Mapsui.Uno.WinUI 5.0.0 to CPM" -ForegroundColor Yellow
  }
}
Try-Project -name 'SantaTracker' -csprojRel 'SantaTracker\SantaTracker\SantaTracker\SantaTracker.csproj' -Prep $prepSanta -ExtraStageFiles @("$repo\SantaTracker\Directory.Packages.props")

# ---- Zara: add NoWarn CS0618 (SKPaint.TextSize obsolete-as-error)
$prepZara = {
  $p = "$repo\Zara\Zara\Zara.csproj"
  $t = Get-Content $p -Raw
  if ($t -notmatch '<NoWarn>') {
    # Insert NoWarn into the first PropertyGroup
    $t = $t -replace '(<PropertyGroup>[^<]*<TargetFrameworks>[^<]+</TargetFrameworks>)', "`$1`n    <NoWarn>`$(NoWarn);CS0618</NoWarn>"
    Set-Content -Path $p -Value $t -NoNewline
    Write-Host "    added NoWarn CS0618" -ForegroundColor Yellow
  }
  # Also need to ensure icon assets are present (this project hit the same splash issue earlier)
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

# ---- vtrack: same as Zara
$prepVtrack = {
  $p = "$repo\vtrack\VTrack\VTrack.csproj"
  $t = Get-Content $p -Raw
  if ($t -notmatch '<NoWarn>') {
    $t = $t -replace '(<PropertyGroup>[^<]*<TargetFrameworks>[^<]+</TargetFrameworks>)', "`$1`n    <NoWarn>`$(NoWarn);CS0618</NoWarn>"
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

Write-Host ""
Write-Host "=== Batch 3 done. Disk: $((Get-PSDrive C).Free / 1GB) GB ===" -ForegroundColor Cyan
& git log --oneline -10

# Wave 2 -- Group A, multi-TFM (desktop + wasm + -windows where declared).
# Skips ConfPass (blocked, see BLOCKED.md).
# ToolkitBench + Meridian already committed (9274a0c, ae06d15). Removed.
# Stops on first build failure. Commits per project.

$ErrorActionPreference = 'Stop'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo

# (name, global.json absolute path, dir to run dotnet in)
# Already committed: Composer 38f8b5c, MSYouTube 622eeba, ClaudeDash ef38bf3.
# Blocked: FluxTransit (see BLOCKED.md).
$projects = @(
  @{ name='Liveline';       gj='Liveline\global.json';                     buildDir='Liveline' },
  @{ name='VoxelWarehouse'; gj='VoxelWarehouse\src\VoxelWarehouse\global.json'; buildDir='VoxelWarehouse\src\VoxelWarehouse' },
  @{ name='liquidMorph';    gj='liquidMorph\global.json';                  buildDir='liquidMorph' }
)

$logFile = Join-Path $repo 'UPGRADE-LOG.md'

function Find-PrimaryCsproj {
  param([string]$buildDir)
  $full = Join-Path $repo $buildDir
  Get-ChildItem -Path $full -Filter *.csproj -Recurse |
    Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\' } |
    Select-Object -First 1 -ExpandProperty FullName
}

foreach ($p in $projects) {
  $name = $p.name
  $gj = Join-Path $repo $p.gj
  $buildDir = Join-Path $repo $p.buildDir

  Write-Host ""
  Write-Host "============================================" -ForegroundColor Cyan
  Write-Host "Wave 2 :: $name" -ForegroundColor Cyan
  Write-Host "============================================" -ForegroundColor Cyan

  if (-not (Test-Path $gj)) { Write-Host "SKIP: no global.json at $gj" -ForegroundColor Yellow; continue }
  $csprojPath = Find-PrimaryCsproj -buildDir $p.buildDir
  if (-not $csprojPath) { Write-Host "SKIP: no csproj under $buildDir" -ForegroundColor Yellow; continue }
  Write-Host "  csproj: $csprojPath"

  # Current SDK
  $gjText = Get-Content $gj -Raw
  $current = ([regex]::Match($gjText, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  Write-Host "  current Uno.Sdk: $current"

  if ($current -eq '6.5.33') {
    Write-Host "  already at 6.5.33, skipping (no commit)" -ForegroundColor Yellow
    Add-Content $logFile "| 2 | $name | $current | 6.5.33 | skipped (already 6.5.33) | - |"
    continue
  }

  # Read TFM list
  $csText = Get-Content $csprojPath -Raw
  $tfmMatch = [regex]::Match($csText, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
  if (-not $tfmMatch.Success) {
    Write-Host "  ERROR: cannot read TargetFrameworks from csproj" -ForegroundColor Red
    throw "no TFMs in $csprojPath"
  }
  $tfms = ($tfmMatch.Groups[1].Value -split ';') | Where-Object { $_ -and $_.Trim() }
  Write-Host "  TFMs to build: $($tfms -join ', ')"

  # Bump global.json
  $newGj = $gjText -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
  Set-Content -Path $gj -Value $newGj -NoNewline

  # Restore + build each TFM
  Push-Location $buildDir
  $tfmResults = @()
  try {
    Write-Host "  dotnet restore..." -ForegroundColor Gray
    $restoreOut = & dotnet restore 2>&1
    if ($LASTEXITCODE -ne 0) {
      Write-Host "  RESTORE FAILED" -ForegroundColor Red
      $restoreOut | Select-Object -Last 25 | ForEach-Object { Write-Host $_ }
      throw "restore failed for $name"
    }

    foreach ($tfm in $tfms) {
      Write-Host "  dotnet build -f $tfm..." -ForegroundColor Gray
      $buildOut = & dotnet build -f $tfm --no-restore --nologo 2>&1
      if ($LASTEXITCODE -ne 0) {
        Write-Host "  BUILD FAILED ($tfm)" -ForegroundColor Red
        $buildOut | Select-Object -Last 40 | ForEach-Object { Write-Host $_ }
        throw "build failed for $name on $tfm"
      }
      Write-Host "  build OK ($tfm)" -ForegroundColor Green
      $tfmResults += "$tfm`:ok"
    }
  }
  finally { Pop-Location }

  # Commit (stage only global.json; revert any side-effect file changes from build)
  Set-Location $repo
  & git add $gj
  # Revert any unrelated working-tree changes the build process may have written
  # (e.g. ColorPaletteOverride.xaml regenerations -- see KNOWN-ISSUES.md)
  & git checkout -- .

  $builds = $tfmResults -join ', '
  $commitMsg = "chore($name): bump to Uno.Sdk 6.5.33`n`nWas $current. Wave 2 -- Group A SDK bump, multi-TFM build.`nbuilds: $builds"
  & git commit -m $commitMsg | Out-Null
  $sha = (& git rev-parse --short HEAD).Trim()
  Write-Host "  committed $sha" -ForegroundColor Green
  Add-Content $logFile "| 2 | $name | $current | 6.5.33 | ok ($builds) | $sha |"
}

Write-Host ""
Write-Host "=== Wave 2 done ===" -ForegroundColor Cyan
& git log --oneline -25

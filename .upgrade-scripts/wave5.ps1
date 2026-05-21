# Wave 5 -- Group B multi-platform (net9 -> net10) + iOS/MacCatalyst strip across all projects.
# Library-first: HorizontalCalendarControl before HorizontalCalendar (spec section 7.7).
# Continues past per-project failures, logs to BLOCKED.md. Per-project bin/obj cleanup.

$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo

$projects = @(
  # Library first
  @{ name='HorizontalCalendarControl'; gj='HorizontalCalendarControl\global.json'; buildDir='HorizontalCalendarControl' },
  @{ name='HorizontalCalendar';        gj='HorizontalCalendar\global.json';        buildDir='HorizontalCalendar' },

  @{ name='AdaptiveInput';             gj='AdaptiveInput\global.json';             buildDir='AdaptiveInput';             extraGj=@('AdaptiveInput\AdaptiveInputDemo\global.json') },
  @{ name='ADE';                       gj='ADE\global.json';                       buildDir='ADE';                       extraGj=@('ADE\AdTokensIDE\global.json') },
  @{ name='ExtendedSplashHDdemo';      gj='ExtendedSplashHDdemo\global.json';      buildDir='ExtendedSplashHDdemo' },
  @{ name='HockeyBarn';                gj='HockeyBarn\global.json';                buildDir='HockeyBarn' },
  @{ name='Pens';                      gj='Pens\global.json';                      buildDir='Pens' },
  @{ name='YUL';                       gj='YUL\global.json';                       buildDir='YUL' },
  @{ name='ZaraApp';                   gj='ZaraApp\global.json';                   buildDir='ZaraApp' },
  @{ name='matrix';                    gj='matrix\global.json';                    buildDir='matrix' },
  @{ name='radial-action-menu';        gj='radial-action-menu\global.json';        buildDir='radial-action-menu';        extraGj=@('radial-action-menu\src\global.json') },
  @{ name='split-flap';                gj='split-flap\global.json';                buildDir='split-flap';                extraGj=@('split-flap\SplitFlap\global.json') },
  @{ name='vtrack';                    gj='vtrack\global.json';                    buildDir='vtrack' },

  # Highest risk last (per spec section 7.6 -- big delta from 6.0.67)
  @{ name='PuckUp';                    gj='PuckUp\global.json';                    buildDir='PuckUp' }
)

$logFile = Join-Path $repo 'UPGRADE-LOG.md'
$blockedFile = Join-Path $repo 'BLOCKED.md'
$iosTfmsToStrip = @('net10.0-ios','net10.0-iossimulator','net9.0-ios','net9.0-iossimulator','net10.0-maccatalyst','net9.0-maccatalyst')

function Find-PrimaryCsproj {
  param([string]$buildDir)
  $full = Join-Path $repo $buildDir
  Get-ChildItem -Path $full -Filter *.csproj -Recurse |
    Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\' } |
    Select-Object -First 1 -ExpandProperty FullName
}

function Update-GlobalJson {
  param([string]$path)
  $text = Get-Content $path -Raw
  $current = ([regex]::Match($text, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  if ($current -ne '6.5.33') {
    $new = $text -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
    Set-Content -Path $path -Value $new -NoNewline
  }
  return $current
}

function Transform-TfmList {
  param([string]$csprojPath, [string[]]$iosToStrip)
  $text = Get-Content $csprojPath -Raw
  $m = [regex]::Match($text, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
  if (-not $m.Success) { return @{ changed=$false } }

  $oldList = $m.Groups[1].Value
  # Normalize: trim and drop empties (matrix had trailing ';')
  $tokens = ($oldList -split ';') | ForEach-Object { $_.Trim() } | Where-Object { $_ }

  $kept = @()
  foreach ($t in $tokens) {
    if ($iosToStrip -contains $t) { continue }
    if ($t -match '^net9\.0(.*)$') { $kept += "net10.0$($Matches[1])" } else { $kept += $t }
  }
  $newList = $kept -join ';'

  if ($newList -eq $oldList) { return @{ changed=$false; oldList=$oldList; newList=$newList } }
  $newTag = $m.Value -replace [regex]::Escape($oldList), $newList
  $newText = $text -replace [regex]::Escape($m.Value), $newTag
  Set-Content -Path $csprojPath -Value $newText -NoNewline
  return @{ changed=$true; oldList=$oldList; newList=$newList }
}

function Clear-ProjectArtifacts {
  param([string]$projectRoot)
  $full = Join-Path $repo $projectRoot
  Get-ChildItem -Path $full -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -in 'bin','obj' } |
    ForEach-Object { try { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue } catch {} }
}

function Get-ProjectRoot { param([string]$buildDir) ($buildDir -split '\\')[0] }

foreach ($p in $projects) {
  $name = $p.name
  $gj = Join-Path $repo $p.gj
  $buildDir = Join-Path $repo $p.buildDir
  $projectRoot = Get-ProjectRoot -buildDir $p.buildDir

  Write-Host ""
  Write-Host "============================================" -ForegroundColor Cyan
  Write-Host "Wave 5 :: $name (disk free: $((Get-PSDrive C).Free / 1GB) GB)" -ForegroundColor Cyan
  Write-Host "============================================" -ForegroundColor Cyan

  if (-not (Test-Path $gj)) {
    Write-Host "SKIP: no global.json" -ForegroundColor Yellow
    Add-Content $logFile "| 5 | $name | - | - | skipped (no global.json) | - |"
    continue
  }

  $csprojPath = Find-PrimaryCsproj -buildDir $p.buildDir
  if (-not $csprojPath) {
    Write-Host "SKIP: no csproj" -ForegroundColor Yellow
    Add-Content $logFile "| 5 | $name | - | - | skipped (no csproj) | - |"
    continue
  }
  Write-Host "  csproj: $csprojPath"

  $gjText = Get-Content $gj -Raw
  $current = ([regex]::Match($gjText, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  Write-Host "  current Uno.Sdk: $current"

  $tfmRes = Transform-TfmList -csprojPath $csprojPath -iosToStrip $iosTfmsToStrip
  if ($tfmRes.changed) {
    Write-Host "  TFMs: $($tfmRes.oldList) -> $($tfmRes.newList)" -ForegroundColor Yellow
  } else {
    Write-Host "  TFMs unchanged: $($tfmRes.oldList)" -ForegroundColor Gray
  }
  $tfms = ($tfmRes.newList -split ';') | Where-Object { $_ -and $_.Trim() }

  Update-GlobalJson -path $gj | Out-Null

  $extraGjFull = @()
  if ($p.extraGj) {
    foreach ($e in $p.extraGj) {
      $ep = Join-Path $repo $e
      if (Test-Path $ep) {
        Update-GlobalJson -path $ep | Out-Null
        $extraGjFull += $ep
        Write-Host "  bumped nested: $e" -ForegroundColor Yellow
      }
    }
  }

  Push-Location $buildDir
  $tfmResults = @()
  $failedTfm = $null
  $failedDetail = $null
  try {
    Write-Host "  dotnet restore..." -ForegroundColor Gray
    $restoreOut = & dotnet restore 2>&1
    if ($LASTEXITCODE -ne 0) {
      $failedTfm = 'restore'
      $failedDetail = ($restoreOut | Select-Object -Last 15 | ForEach-Object { $_.ToString() }) -join ' / '
      Write-Host "  RESTORE FAILED" -ForegroundColor Red
    } else {
      foreach ($tfm in $tfms) {
        Write-Host "  dotnet build -f $tfm..." -ForegroundColor Gray
        $buildOut = & dotnet build -f $tfm --no-restore --nologo 2>&1
        if ($LASTEXITCODE -ne 0) {
          Write-Host "  BUILD FAILED ($tfm)" -ForegroundColor Red
          $failedTfm = $tfm
          $failedDetail = ($buildOut | Select-Object -Last 15 | ForEach-Object { $_.ToString() }) -join ' / '
          break
        }
        Write-Host "  build OK ($tfm)" -ForegroundColor Green
        $tfmResults += "$tfm`:ok"
      }
    }
  }
  finally { Pop-Location }

  Set-Location $repo

  if ($failedTfm) {
    & git checkout -- .
    Write-Host "  BLOCKED ($failedTfm). Reverted." -ForegroundColor Red
    $detailLen = [Math]::Min(220, $failedDetail.Length)
    $detailEsc = ($failedDetail -replace '\|','/' -replace '`','').Substring(0, $detailLen)
    Add-Content $blockedFile "| 5 | $name | $current / $failedTfm | build failed | $detailEsc |"
    Add-Content $logFile "| 5 | $name | $current | 6.5.33 | BLOCKED ($failedTfm) | - |"
    Clear-ProjectArtifacts -projectRoot $projectRoot
    continue
  }

  & git add $gj $csprojPath
  foreach ($e in $extraGjFull) { & git add $e }
  & git checkout -- .

  $builds = $tfmResults -join ', '
  $tfmNote = if ($tfmRes.changed) { "; TFMs $($tfmRes.oldList) -> $($tfmRes.newList)" } else { '' }
  $commitMsg = "chore($name): bump to Uno.Sdk 6.5.33$($tfmNote -replace ';',',')`n`nWas $current. Wave 5 -- Group B multi-platform.`nbuilds: $builds"
  & git commit -m $commitMsg | Out-Null
  $sha = (& git rev-parse --short HEAD).Trim()
  Write-Host "  committed $sha" -ForegroundColor Green
  Add-Content $logFile "| 5 | $name | $current | 6.5.33 | ok ($builds)$tfmNote | $sha |"

  Clear-ProjectArtifacts -projectRoot $projectRoot
}

Write-Host ""
Write-Host "=== Wave 5 done. Disk free: $((Get-PSDrive C).Free / 1GB) GB ===" -ForegroundColor Cyan
& git log --oneline -20

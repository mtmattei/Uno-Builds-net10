# Wave 4 -- Group B (net9 -> net10 TFM + SDK bump).
# All single-TFM net9.0-desktop except Thermostat (android + desktop).
# Continues past per-project failures, logs to BLOCKED.md.
# Per-project bin/obj cleanup.

$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo

$projects = @(
  @{ name='DepthCard';           gj='DepthCard\global.json';            buildDir='DepthCard';           extraGj=@('DepthCard\DepthCardDemo\global.json') },
  @{ name='EnterpriseDashboard'; gj='EnterpriseDashboard\global.json';  buildDir='EnterpriseDashboard' },
  @{ name='MPE';                 gj='MPE\global.json';                  buildDir='MPE' },
  @{ name='SmartNotes';          gj='SmartNotes\global.json';           buildDir='SmartNotes' },
  @{ name='Thermostat';          gj='Thermostat\global.json';           buildDir='Thermostat' },
  @{ name='Unoblueprint';        gj='Unoblueprint\global.json';         buildDir='Unoblueprint' },
  @{ name='heatmap';             gj='heatmap\global.json';              buildDir='heatmap' },
  @{ name='msn';                 gj='msn\global.json';                  buildDir='msn' }
)

$logFile = Join-Path $repo 'UPGRADE-LOG.md'
$blockedFile = Join-Path $repo 'BLOCKED.md'

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

function Update-Net9ToNet10 {
  param([string]$csprojPath)
  $text = Get-Content $csprojPath -Raw
  $tfmMatch = [regex]::Match($text, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
  if (-not $tfmMatch.Success) { return @{ changed=$false; oldList=$null; newList=$null } }
  $oldList = $tfmMatch.Groups[1].Value
  # Token-level replacement to avoid net9.0 -> net10.0 messing with already-net10 tokens
  $tokens = ($oldList -split ';') | ForEach-Object {
    if ($_ -match '^net9\.0(.*)$') { "net10.0$($Matches[1])" } else { $_ }
  }
  $newList = $tokens -join ';'
  if ($newList -eq $oldList) { return @{ changed=$false; oldList=$oldList; newList=$newList } }
  $newTag = $tfmMatch.Value -replace [regex]::Escape($oldList), $newList
  $newText = $text -replace [regex]::Escape($tfmMatch.Value), $newTag
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
  Write-Host "Wave 4 :: $name (disk free: $((Get-PSDrive C).Free / 1GB) GB)" -ForegroundColor Cyan
  Write-Host "============================================" -ForegroundColor Cyan

  if (-not (Test-Path $gj)) {
    Write-Host "SKIP: no global.json" -ForegroundColor Yellow
    Add-Content $logFile "| 4 | $name | - | - | skipped (no global.json) | - |"
    continue
  }

  $csprojPath = Find-PrimaryCsproj -buildDir $p.buildDir
  if (-not $csprojPath) {
    Write-Host "SKIP: no csproj" -ForegroundColor Yellow
    Add-Content $logFile "| 4 | $name | - | - | skipped (no csproj) | - |"
    continue
  }
  Write-Host "  csproj: $csprojPath"

  $gjText = Get-Content $gj -Raw
  $current = ([regex]::Match($gjText, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  Write-Host "  current Uno.Sdk: $current"

  # TFM upgrade net9 -> net10
  $tfmRes = Update-Net9ToNet10 -csprojPath $csprojPath
  if ($tfmRes.changed) {
    Write-Host "  TFMs: $($tfmRes.oldList) -> $($tfmRes.newList)" -ForegroundColor Yellow
  } else {
    Write-Host "  TFMs unchanged: $($tfmRes.oldList)" -ForegroundColor Gray
  }
  $tfms = ($tfmRes.newList -split ';') | Where-Object { $_ -and $_.Trim() }

  # global.json bump
  Update-GlobalJson -path $gj | Out-Null

  # Nested gj bump
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
    Add-Content $blockedFile "| 4 | $name | $current / $failedTfm | build failed | $detailEsc |"
    Add-Content $logFile "| 4 | $name | $current | 6.5.33 | BLOCKED ($failedTfm) | - |"
    Clear-ProjectArtifacts -projectRoot $projectRoot
    continue
  }

  & git add $gj $csprojPath
  foreach ($e in $extraGjFull) { & git add $e }
  & git checkout -- .

  $builds = $tfmResults -join ', '
  $commitMsg = "chore($name): bump to Uno.Sdk 6.5.33 + net10.0 TFMs`n`nWas $current on $($tfmRes.oldList). Wave 4 -- Group B (net9 -> net10).`nbuilds: $builds"
  & git commit -m $commitMsg | Out-Null
  $sha = (& git rev-parse --short HEAD).Trim()
  Write-Host "  committed $sha" -ForegroundColor Green
  Add-Content $logFile "| 4 | $name | $current | 6.5.33 | ok ($builds); net9->net10 | $sha |"

  Clear-ProjectArtifacts -projectRoot $projectRoot
}

Write-Host ""
Write-Host "=== Wave 4 done. Disk free: $((Get-PSDrive C).Free / 1GB) GB ===" -ForegroundColor Cyan
& git log --oneline -15

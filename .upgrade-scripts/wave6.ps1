# Wave 6 -- specials per spec section 7.
# KineticSculptor stays on dev line (6.7.0-dev.x). Others blanket to 6.5.33.

$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo

$kineticSdk = '6.7.0-dev.52'  # latest 6.7.0-dev.x per spec section 7.1 (verified 2026-05-19)

$projects = @(
  @{ name='KineticSculptor';              gj='KineticSculptor\global.json';              buildDir='KineticSculptor';              targetSdk=$kineticSdk },
  @{ name='AnimatedExtendedSplashScreen'; gj='AnimatedExtendedSplashScreen\global.json'; buildDir='AnimatedExtendedSplashScreen'; targetSdk='6.5.33' },
  @{ name='UnoEnterpriseApp';             gj='UnoEnterpriseApp\global.json';             buildDir='UnoEnterpriseApp';             targetSdk='6.5.33' },
  @{ name='UnoWallet';                    gj='UnoWallet\global.json';                    buildDir='UnoWallet';                    targetSdk='6.5.33' },
  @{ name='AgentNotifier';                gj='AgentNotifier\global.json';                buildDir='AgentNotifier';                targetSdk='6.5.33' }
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

function Update-GlobalJsonTo {
  param([string]$path, [string]$target)
  $text = Get-Content $path -Raw
  $current = ([regex]::Match($text, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  if ($current -ne $target) {
    $new = $text -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', "`"Uno.Sdk`": `"$target`""
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

foreach ($p in $projects) {
  $name = $p.name
  $gj = Join-Path $repo $p.gj
  $buildDir = Join-Path $repo $p.buildDir
  $projectRoot = ($p.buildDir -split '\\')[0]
  $target = $p.targetSdk

  Write-Host ""
  Write-Host "============================================" -ForegroundColor Cyan
  Write-Host "Wave 6 :: $name -> $target (disk free: $((Get-PSDrive C).Free / 1GB) GB)" -ForegroundColor Cyan
  Write-Host "============================================" -ForegroundColor Cyan

  if (-not (Test-Path $gj)) {
    Add-Content $logFile "| 6 | $name | - | $target | skipped (no global.json) | - |"
    continue
  }

  $csprojPath = Find-PrimaryCsproj -buildDir $p.buildDir
  if (-not $csprojPath) {
    Add-Content $logFile "| 6 | $name | - | $target | skipped (no csproj) | - |"
    continue
  }
  Write-Host "  csproj: $csprojPath"

  $gjText = Get-Content $gj -Raw
  $current = ([regex]::Match($gjText, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  Write-Host "  current Uno.Sdk: $current"

  $tfmRes = Transform-TfmList -csprojPath $csprojPath -iosToStrip $iosTfmsToStrip
  if ($tfmRes.changed) {
    Write-Host "  TFMs: $($tfmRes.oldList) -> $($tfmRes.newList)" -ForegroundColor Yellow
  }
  $tfms = ($tfmRes.newList -split ';') | Where-Object { $_ -and $_.Trim() }

  Update-GlobalJsonTo -path $gj -target $target | Out-Null

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
    Add-Content $blockedFile "| 6 | $name | $current / $failedTfm | build failed | $detailEsc |"
    Add-Content $logFile "| 6 | $name | $current | $target | BLOCKED ($failedTfm) | - |"
    Clear-ProjectArtifacts -projectRoot $projectRoot
    continue
  }

  & git add $gj $csprojPath
  & git checkout -- .

  $builds = $tfmResults -join ', '
  $tfmNote = if ($tfmRes.changed) { ", TFM changes" } else { '' }
  $commitMsg = "chore($name): bump to Uno.Sdk $target$tfmNote`n`nWas $current. Wave 6 -- special case per spec section 7.`nbuilds: $builds"
  & git commit -m $commitMsg | Out-Null
  $sha = (& git rev-parse --short HEAD).Trim()
  Write-Host "  committed $sha" -ForegroundColor Green
  Add-Content $logFile "| 6 | $name | $current | $target | ok ($builds) | $sha |"

  Clear-ProjectArtifacts -projectRoot $projectRoot
}

Write-Host ""
Write-Host "=== Wave 6 done. Disk free: $((Get-PSDrive C).Free / 1GB) GB ===" -ForegroundColor Cyan
& git log --oneline -10

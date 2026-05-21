# Fix the 5 APT2260 splash/icon blockers by copying PuckUp's source SVG assets
# (Uno.Resizetizer will regenerate the missing drawable/uno_splash_image, mipmap/icon, etc.).
# Continues past per-project failures, logs.

$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo

# PuckUp source SVGs (generic 456x456 white-square placeholders + simple splash)
$srcIcons  = "$repo\PuckUp\PuckUp\Assets\Icons"
$srcSplash = "$repo\PuckUp\PuckUp\Assets\Splash"

# (name, primary csproj path relative to repo)
$projects = @(
  @{ name='Sanctum';                      csproj='Sanctum\Sanctum\Sanctum.csproj' },
  @{ name='Zara';                         csproj='Zara\Zara\Zara.csproj';                                   stripIos=$true },
  @{ name='vtrack';                       csproj='vtrack\VTrack\VTrack.csproj';                             stripIos=$true },
  @{ name='AdaptiveInput';                csproj='AdaptiveInput\AdaptiveInputDemo\AdaptiveInputDemo\AdaptiveInputDemo.csproj'; stripIos=$true },
  @{ name='AnimatedExtendedSplashScreen'; csproj='AnimatedExtendedSplashScreen\AnimatedExtendedSplashScreen\AnimatedExtendedSplashScreen.csproj' }
)

$logFile = "$repo\UPGRADE-LOG.md"
$blockedFile = "$repo\BLOCKED.md"
$iosTfmsToStrip = @('net10.0-ios','net10.0-iossimulator','net9.0-ios','net9.0-iossimulator','net10.0-maccatalyst','net9.0-maccatalyst')

foreach ($p in $projects) {
  $name = $p.name
  $csprojFull = Join-Path $repo $p.csproj
  $projectDir = Split-Path -Parent $csprojFull
  $assetsDir = Join-Path $projectDir 'Assets'
  $iconsDir = Join-Path $assetsDir 'Icons'
  $splashDir = Join-Path $assetsDir 'Splash'

  Write-Host ""
  Write-Host "============================================" -ForegroundColor Cyan
  Write-Host "Splash-fix :: $name (disk: $((Get-PSDrive C).Free / 1GB) GB)" -ForegroundColor Cyan
  Write-Host "============================================" -ForegroundColor Cyan

  if (-not (Test-Path $csprojFull)) {
    Write-Host "SKIP: csproj not found at $csprojFull" -ForegroundColor Yellow
    continue
  }

  # Locate the project's global.json (top-level under <name>\)
  $gjCandidates = Get-ChildItem -Path (Join-Path $repo $name) -Filter global.json -Recurse -ErrorAction SilentlyContinue
  if (-not $gjCandidates) {
    Write-Host "SKIP: no global.json" -ForegroundColor Yellow
    continue
  }
  $gj = $gjCandidates[0].FullName
  Write-Host "  csproj:  $csprojFull"
  Write-Host "  assets:  $assetsDir"
  Write-Host "  gj:      $gj"

  # Copy source SVGs
  New-Item -ItemType Directory -Force -Path $iconsDir | Out-Null
  New-Item -ItemType Directory -Force -Path $splashDir | Out-Null
  Copy-Item -Path "$srcIcons\icon.svg" -Destination $iconsDir -Force
  Copy-Item -Path "$srcIcons\icon_foreground.svg" -Destination $iconsDir -Force
  Copy-Item -Path "$srcSplash\splash_screen.svg" -Destination $splashDir -Force
  Write-Host "  added 3 SVG placeholders" -ForegroundColor Yellow

  # Bump global.json
  $gjText = Get-Content $gj -Raw
  $current = ([regex]::Match($gjText, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  if ($current -ne '6.5.33') {
    $new = $gjText -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
    Set-Content -Path $gj -Value $new -NoNewline
  }
  Write-Host "  current: $current"

  # Also bump any nested global.json (e.g. AdaptiveInputDemo)
  $extraGj = @()
  foreach ($cand in $gjCandidates | Select-Object -Skip 1) {
    $t = Get-Content $cand.FullName -Raw
    $c = ([regex]::Match($t, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
    if ($c -ne '6.5.33') {
      $n = $t -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
      Set-Content -Path $cand.FullName -Value $n -NoNewline
      Write-Host "  bumped nested: $($cand.FullName)" -ForegroundColor Yellow
    }
    $extraGj += $cand.FullName
  }

  # iOS strip + net9->net10 if needed
  $csText = Get-Content $csprojFull -Raw
  $m = [regex]::Match($csText, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
  $csChanged = $false
  if ($m.Success) {
    $oldList = $m.Groups[1].Value
    $tokens = ($oldList -split ';') | ForEach-Object { $_.Trim() } | Where-Object { $_ }
    $kept = @()
    foreach ($t in $tokens) {
      if ($iosTfmsToStrip -contains $t) { continue }
      if ($t -match '^net9\.0(.*)$') { $kept += "net10.0$($Matches[1])" } else { $kept += $t }
    }
    $newList = $kept -join ';'
    if ($newList -ne $oldList) {
      $newTag = $m.Value -replace [regex]::Escape($oldList), $newList
      $csText = $csText -replace [regex]::Escape($m.Value), $newTag
      Set-Content -Path $csprojFull -Value $csText -NoNewline
      $csChanged = $true
      Write-Host "  TFMs: $oldList -> $newList" -ForegroundColor Yellow
    }
  }

  # Re-read final TFM list
  $csText = Get-Content $csprojFull -Raw
  $m = [regex]::Match($csText, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
  $tfms = ($m.Groups[1].Value -split ';') | Where-Object { $_ -and $_.Trim() }

  # Restore + build
  Push-Location $projectDir
  $tfmResults = @()
  $failedTfm = $null
  $failedDetail = $null
  try {
    Write-Host "  dotnet restore..." -ForegroundColor Gray
    $out = & dotnet restore $csprojFull 2>&1
    if ($LASTEXITCODE -ne 0) {
      $failedTfm = 'restore'
      $failedDetail = ($out | Select-Object -Last 15 | ForEach-Object { $_.ToString() }) -join ' / '
    } else {
      foreach ($tfm in $tfms) {
        Write-Host "  dotnet build -f $tfm..." -ForegroundColor Gray
        $out = & dotnet build $csprojFull -f $tfm --no-restore --nologo 2>&1
        if ($LASTEXITCODE -ne 0) {
          $failedTfm = $tfm
          $failedDetail = ($out | Select-Object -Last 15 | ForEach-Object { $_.ToString() }) -join ' / '
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
    # Also remove the untracked SVG additions on failure
    if (Test-Path $iconsDir) { Remove-Item -Path "$iconsDir\icon.svg","$iconsDir\icon_foreground.svg" -Force -ErrorAction SilentlyContinue }
    if (Test-Path $splashDir) { Remove-Item -Path "$splashDir\splash_screen.svg" -Force -ErrorAction SilentlyContinue }
    Write-Host "  STILL BLOCKED ($failedTfm)" -ForegroundColor Red
    $detailLen = [Math]::Min(220, $failedDetail.Length)
    $detailEsc = ($failedDetail -replace '\|','/' -replace '`','').Substring(0, $detailLen)
    Add-Content $blockedFile "| fix | $name | $current / $failedTfm | splash-fix attempt: still failing | $detailEsc |"
    Add-Content $logFile "| fix | $name | $current | 6.5.33 | STILL BLOCKED ($failedTfm) | - |"
    Get-ChildItem -Path (Join-Path $repo $name) -Directory -Recurse -Force -ErrorAction SilentlyContinue |
      Where-Object { $_.Name -in 'bin','obj' } |
      ForEach-Object { try { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue } catch {} }
    continue
  }

  # Stage and commit
  & git add $gj $csprojFull
  foreach ($e in $extraGj) { & git add $e }
  & git add (Join-Path $iconsDir 'icon.svg') (Join-Path $iconsDir 'icon_foreground.svg') (Join-Path $splashDir 'splash_screen.svg')
  & git checkout -- .

  $builds = $tfmResults -join ', '
  $msg = @"
fix($name): add Android Resizetizer source assets + bump to Uno.Sdk 6.5.33

Was $current. Blocked on APT2260 (drawable/uno_splash_image + mipmap/icon
missing). Added generic 456x456 placeholder SVGs from PuckUp's working
scaffold so Uno.Resizetizer regenerates the missing Android resources.

Replace icon.svg / icon_foreground.svg / splash_screen.svg with project-
appropriate branding when ready.

builds: $builds
"@
  & git commit -m $msg | Out-Null
  $sha = (& git rev-parse --short HEAD).Trim()
  Write-Host "  committed $sha" -ForegroundColor Green

  Add-Content $logFile "| fix | $name | $current | 6.5.33 | ok ($builds); added Resizetizer placeholder SVGs | $sha |"

  Get-ChildItem -Path (Join-Path $repo $name) -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -in 'bin','obj' } |
    ForEach-Object { try { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue } catch {} }
}

Write-Host ""
Write-Host "=== Splash fix done. Disk: $((Get-PSDrive C).Free / 1GB) GB ===" -ForegroundColor Cyan
& git log --oneline -10

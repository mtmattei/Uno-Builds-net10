# Wave 3 -- Group A, multi-platform (desktop + wasm + android + windows where declared).
# Strips iOS/MacCatalyst TFMs per spec section 2.3 for affected projects.
# Updates nested global.json files when present.
# Continues past per-project failures, logs them to BLOCKED.md automatically.

$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo

# stripIos=$true triggers iOS/MacCatalyst TFM removal in primary csproj.
# extraGj is a list of additional global.json paths to also update (nested ones).
$projects = @(
  @{ name='CCUI';            gj='CCUI\CameraCaptureUISample\global.json';            buildDir='CCUI\CameraCaptureUISample';            stripIos=$true; extraGj=@('CCUI\Uno.Samples\UI\CameraCaptureUI\global.json') },
  @{ name='FibonacciSphere'; gj='FibonacciSphere\global.json';                        buildDir='FibonacciSphere';                        stripIos=$true },
  @{ name='FieldOpsPro';     gj='FieldOpsPro\global.json';                            buildDir='FieldOpsPro';                            stripIos=$true },
  @{ name='FriendSonar';     gj='FriendSonar\global.json';                            buildDir='FriendSonar' },
  @{ name='Gridform';        gj='Gridform\GridForm\global.json';                      buildDir='Gridform\GridForm' },
  @{ name='InfiniteImage';   gj='InfiniteImage\global.json';                          buildDir='InfiniteImage';                          extraGj=@('InfiniteImage\InfiniteImage\global.json') },
  @{ name='PrecisionDial';   gj='PrecisionDial\global.json';                          buildDir='PrecisionDial';                          stripIos=$true },
  @{ name='QuoteCraft';      gj='QuoteCraft\src\global.json';                         buildDir='QuoteCraft\src' },
  @{ name='ReservoomUno';    gj='ReservoomUno\global.json';                           buildDir='ReservoomUno';                           stripIos=$true },
  @{ name='RivTes';          gj='RivTes\src\PhosphorProtocol\global.json';            buildDir='RivTes\src\PhosphorProtocol' },
  @{ name='SalesDashboard';  gj='SalesDashboard\global.json';                         buildDir='SalesDashboard';                         stripIos=$true },
  @{ name='Sanctum';         gj='Sanctum\global.json';                                buildDir='Sanctum' },
  @{ name='SantaTracker';    gj='SantaTracker\global.json';                           buildDir='SantaTracker' },
  @{ name='SpaceXhistory';   gj='SpaceXhistory\global.json';                          buildDir='SpaceXhistory';                          stripIos=$true; extraGj=@('SpaceXhistory\SpaceXhistory\global.json') },
  @{ name='Sweather';        gj='Sweather\global.json';                               buildDir='Sweather' },
  @{ name='UnoVox';          gj='UnoVox\global.json';                                 buildDir='UnoVox' },
  @{ name='Wellmetrix';      gj='Wellmetrix\global.json';                             buildDir='Wellmetrix';                             extraGj=@('Wellmetrix\Wellmetrix\global.json') },
  @{ name='Zara';            gj='Zara\global.json';                                   buildDir='Zara';                                   stripIos=$true },
  @{ name='FormaEspresso';   gj='FormaEspresso\global.json';                          buildDir='FormaEspresso' },
  @{ name='ADTest';          gj='ADTest\ADTest\global.json';                          buildDir='ADTest\ADTest' }
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

foreach ($p in $projects) {
  $name = $p.name
  $gj = Join-Path $repo $p.gj
  $buildDir = Join-Path $repo $p.buildDir

  Write-Host ""
  Write-Host "============================================" -ForegroundColor Cyan
  Write-Host "Wave 3 :: $name" -ForegroundColor Cyan
  Write-Host "============================================" -ForegroundColor Cyan

  if (-not (Test-Path $gj)) {
    Write-Host "SKIP: no global.json at $gj" -ForegroundColor Yellow
    Add-Content $logFile "| 3 | $name | - | - | skipped (no global.json) | - |"
    continue
  }

  $csprojPath = Find-PrimaryCsproj -buildDir $p.buildDir
  if (-not $csprojPath) {
    Write-Host "SKIP: no csproj under $buildDir" -ForegroundColor Yellow
    Add-Content $logFile "| 3 | $name | - | - | skipped (no csproj) | - |"
    continue
  }
  Write-Host "  csproj: $csprojPath"

  # Read current SDK
  $gjText = Get-Content $gj -Raw
  $current = ([regex]::Match($gjText, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  Write-Host "  current Uno.Sdk: $current"

  if ($current -eq '6.5.33') {
    Write-Host "  already at 6.5.33, skipping (no commit)" -ForegroundColor Yellow
    Add-Content $logFile "| 3 | $name | $current | 6.5.33 | skipped (already 6.5.33) | - |"
    continue
  }

  # iOS/MacCatalyst strip
  $stripNote = ''
  $csChanged = $false
  if ($p.stripIos) {
    $csText = Get-Content $csprojPath -Raw
    $tfmMatch = [regex]::Match($csText, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
    if ($tfmMatch.Success) {
      $tag = $tfmMatch.Value
      $list = $tfmMatch.Groups[1].Value
      $kept = ($list -split ';') | Where-Object { $iosTfmsToStrip -notcontains $_ }
      $newList = $kept -join ';'
      if ($newList -ne $list) {
        $newTag = $tag -replace [regex]::Escape($list), $newList
        $csText = $csText -replace [regex]::Escape($tag), $newTag
        Set-Content -Path $csprojPath -Value $csText -NoNewline
        $stripNote = "; stripped iOS/MacCatalyst"
        $csChanged = $true
        Write-Host "  stripped: $list -> $newList" -ForegroundColor Yellow
      }
    }
  }

  # Read final TFM list (post strip)
  $csText = Get-Content $csprojPath -Raw
  $tfmMatch = [regex]::Match($csText, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
  $tfms = ($tfmMatch.Groups[1].Value -split ';') | Where-Object { $_ -and $_.Trim() }
  Write-Host "  TFMs to build: $($tfms -join ', ')"

  # Bump primary global.json
  Update-GlobalJson -path $gj | Out-Null

  # Bump nested global.json files
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

  # Restore + build
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
    # Revert all working-tree changes for this project
    & git checkout -- .
    Write-Host "  BLOCKED ($failedTfm). Reverted." -ForegroundColor Red
    $detailEsc = ($failedDetail -replace '\|','/' -replace '`','').Substring(0, [Math]::Min(220, $failedDetail.Length))
    Add-Content $blockedFile "| 3 | $name | $current / $failedTfm | build failed | $detailEsc |"
    Add-Content $logFile "| 3 | $name | $current | 6.5.33 | BLOCKED ($failedTfm) | - |"
    continue
  }

  # Stage and commit
  & git add $gj
  if ($csChanged) { & git add $csprojPath }
  foreach ($e in $extraGjFull) { & git add $e }
  # Discard build side effects (e.g. ColorPaletteOverride.xaml regen)
  & git checkout -- .

  $builds = $tfmResults -join ', '
  $stripSummary = if ($csChanged) { "`n`nStripped iOS/MacCatalyst TFMs per upgrade spec section 2.3." } else { '' }
  $commitMsg = "chore($name): bump to Uno.Sdk 6.5.33$stripSummary`n`nWas $current. Wave 3 -- Group A multi-platform SDK bump.`nbuilds: $builds"
  & git commit -m $commitMsg | Out-Null
  $sha = (& git rev-parse --short HEAD).Trim()
  Write-Host "  committed $sha" -ForegroundColor Green
  Add-Content $logFile "| 3 | $name | $current | 6.5.33 | ok ($builds)$stripNote | $sha |"
}

Write-Host ""
Write-Host "=== Wave 3 done ===" -ForegroundColor Cyan
& git log --oneline -30

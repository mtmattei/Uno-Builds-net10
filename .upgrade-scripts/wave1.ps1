# Wave 1 -- 10 desktop-only projects, SDK bump only
# Stops on first build failure. Commits per project.

$ErrorActionPreference = 'Stop'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo

# Project root (where global.json lives), display name for commit
# Caffe already committed (19808af) -- removed from list.
$projects = @(
  @{ name='Nexus';          root='Nexus' },
  @{ name='Olea';           root='Olea' },
  @{ name='Orbital';        root='Orbital' },
  @{ name='Riviera';        root='Riviera/RivieraHome' },
  @{ name='WinampClassic';  root='WinampClassic' },
  @{ name='MCP-blog';       root='MCP-blog' },
  @{ name='listhold';       root='listhold' },
  @{ name='fitnesstracker'; root='fitnesstracker' },
  @{ name='Vitalis';        root='Vitalis' }
)

$logFile = Join-Path $repo 'UPGRADE-LOG.md'
if (-not (Test-Path $logFile)) {
  Set-Content $logFile "# Upgrade log`n`n| Wave | Project | From | To | Result | Commit |`n|---|---|---|---|---|---|`n"
}

foreach ($p in $projects) {
  $name = $p.name
  $rootRel = $p.root
  $root = Join-Path $repo $rootRel
  $gj = Join-Path $root 'global.json'

  Write-Host ""
  Write-Host "============================================" -ForegroundColor Cyan
  Write-Host "Wave 1 :: $name" -ForegroundColor Cyan
  Write-Host "============================================" -ForegroundColor Cyan

  if (-not (Test-Path $gj)) { Write-Host "SKIP: no global.json at $gj" -ForegroundColor Yellow; continue }

  # Capture current version
  $gjText = Get-Content $gj -Raw
  $current = ([regex]::Match($gjText, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  Write-Host "  current Uno.Sdk: $current"

  if ($current -eq '6.5.33') {
    Write-Host "  already at 6.5.33, skipping" -ForegroundColor Yellow
    Add-Content $logFile "| 1 | $name | $current | 6.5.33 | skipped (already 6.5.33) | - |"
    continue
  }

  # Update global.json
  $newText = $gjText -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
  Set-Content -Path $gj -Value $newText -NoNewline

  # Restore + build (desktop only for Wave 1)
  Push-Location $root
  try {
    Write-Host "  dotnet restore..." -ForegroundColor Gray
    $restoreOut = & dotnet restore 2>&1
    if ($LASTEXITCODE -ne 0) {
      Write-Host "  RESTORE FAILED" -ForegroundColor Red
      $restoreOut | Select-Object -Last 20 | Write-Host
      throw "restore failed for $name"
    }

    Write-Host "  dotnet build -f net10.0-desktop..." -ForegroundColor Gray
    $buildOut = & dotnet build -f net10.0-desktop --no-restore --nologo 2>&1
    if ($LASTEXITCODE -ne 0) {
      Write-Host "  BUILD FAILED" -ForegroundColor Red
      $buildOut | Select-Object -Last 30 | Write-Host
      throw "build failed for $name"
    }
    Write-Host "  build OK" -ForegroundColor Green
  }
  finally { Pop-Location }

  # Commit
  Set-Location $repo
  & git add $gj
  $commitMsg = "chore($name): bump to Uno.Sdk 6.5.33`n`nWas $current. Wave 1 -- desktop-only SDK bump, no TFM change.`ndesktop build: ok"
  & git commit -m $commitMsg | Out-Null
  $sha = (& git rev-parse --short HEAD).Trim()
  Write-Host "  committed $sha" -ForegroundColor Green
  Add-Content $logFile "| 1 | $name | $current | 6.5.33 | ok (desktop) | $sha |"
}

Write-Host ""
Write-Host "=== Wave 1 done ===" -ForegroundColor Cyan
& git log --oneline -15

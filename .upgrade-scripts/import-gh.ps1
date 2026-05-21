# Import 6 GitHub repos into Uno-Builds-net10/
# Skips dups by default. Replaces broken Thermostat/ with fresh clone of Thermostat-Build.
# Attempts upgrade (bump global.json) where a global.json is found in root or one level deep.

$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
$tempClones = 'C:\temp\imports'
Set-Location $repo
$logFile = "$repo\UPGRADE-LOG.md"

if (-not (Test-Path $tempClones)) { New-Item -ItemType Directory -Path $tempClones | Out-Null }

# (github name, archive folder name, attempt upgrade?)
$imports = @(
  @{ gh='Hive';             archive='Hive';        upgrade=$true  },
  @{ gh='Carousel';         archive='Carousel';    upgrade=$true  },
  @{ gh='SnackBar';         archive='SnackBar';    upgrade=$true  },
  @{ gh='ChefsTest';        archive='ChefsTest';   upgrade=$false }, # methodology study, multi-subproject
  @{ gh='DigitalFidget';    archive='DigitalFidget'; upgrade=$true },
  @{ gh='Thermostat-Build'; archive='Thermostat';  upgrade=$true; replaceArchive=$true }
)

function Try-Upgrade {
  param([string]$archiveDir)
  # Find a global.json no deeper than 3 levels
  $gj = Get-ChildItem -Path $archiveDir -Filter global.json -Recurse -ErrorAction SilentlyContinue |
    Where-Object { ($_.FullName.Split('\').Count - $archiveDir.Split('\').Count) -le 3 } |
    Select-Object -First 1
  if (-not $gj) { Write-Host "    no global.json; not upgrading" -ForegroundColor Yellow; return $null }

  $t = Get-Content $gj.FullName -Raw
  $cur = ([regex]::Match($t, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
  if (-not $cur) { Write-Host "    global.json has no Uno.Sdk; skipping" -ForegroundColor Yellow; return $null }
  Write-Host "    current Uno.Sdk: $cur" -ForegroundColor Gray
  if ($cur -ne '6.5.33') {
    $n = $t -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
    Set-Content -Path $gj.FullName -Value $n -NoNewline
    Write-Host "    bumped global.json -> 6.5.33" -ForegroundColor Yellow
  }
  return $cur
}

foreach ($imp in $imports) {
  $ghName = $imp.gh
  $archName = $imp.archive
  $dest = Join-Path $repo $archName
  $cloneTo = Join-Path $tempClones $ghName

  Write-Host ""
  Write-Host "============================================" -ForegroundColor Cyan
  Write-Host "Import :: $ghName -> $archName (disk: $((Get-PSDrive C).Free / 1GB) GB)" -ForegroundColor Cyan
  Write-Host "============================================" -ForegroundColor Cyan

  # Clean any prior clone
  if (Test-Path $cloneTo) { Remove-Item -Path $cloneTo -Recurse -Force }

  # Clone via gh
  Write-Host "  cloning..." -ForegroundColor Gray
  & gh repo clone "mtmattei/$ghName" $cloneTo 2>&1 | Select-Object -Last 3 | ForEach-Object { Write-Host "    $_" }
  if (-not (Test-Path $cloneTo)) {
    Write-Host "  CLONE FAILED, skipping $ghName" -ForegroundColor Red
    continue
  }

  # Strip .git
  $gitDir = Join-Path $cloneTo '.git'
  if (Test-Path $gitDir) {
    # Use -Force in case some files are read-only (gh clone marks pack files RO)
    & cmd /c "rmdir /s /q `"$gitDir`"" | Out-Null
  }

  # If replacing existing archive folder, remove from filesystem (git will pick up the delete)
  if ($imp.replaceArchive -and (Test-Path $dest)) {
    Write-Host "  removing existing archive folder $archName..." -ForegroundColor Yellow
    & cmd /c "rmdir /s /q `"$dest`"" | Out-Null
  } elseif (Test-Path $dest) {
    Write-Host "  SKIP: archive folder $archName already exists (not configured to replace)" -ForegroundColor Yellow
    Remove-Item -Path $cloneTo -Recurse -Force
    continue
  }

  # Copy clone -> archive
  Write-Host "  copying to archive..." -ForegroundColor Gray
  & robocopy $cloneTo $dest /E /NFL /NDL /NJH /NJS /NP | Out-Null

  # Remove the temp clone
  Remove-Item -Path $cloneTo -Recurse -Force -ErrorAction SilentlyContinue

  # Optional upgrade attempt
  $beforeSdk = $null
  if ($imp.upgrade) {
    Write-Host "  upgrade attempt:" -ForegroundColor Gray
    $beforeSdk = Try-Upgrade -archiveDir $dest
  }

  # Stage everything in this folder + commit
  & git add "$archName"
  $sdkNote = if ($beforeSdk) { ", bumped Uno.Sdk $beforeSdk -> 6.5.33" } else { '' }
  $replaceNote = if ($imp.replaceArchive) { " (replaces prior broken folder)" } else { '' }
  $msg = @"
feat($archName): import from mtmattei/$ghName$sdkNote

Imported from https://github.com/mtmattei/$ghName$replaceNote.
Not build-verified in this commit -- the GitHub source is recorded as-is plus the
SDK-pin bump where applicable. Build verification and any TFM/source fixes
land in a follow-up commit per project.
"@
  & git commit -m $msg | Out-Null
  $sha = (& git rev-parse --short HEAD).Trim()
  Write-Host "  committed $sha" -ForegroundColor Green

  $logRow = if ($beforeSdk) {
    "| import | $archName | $beforeSdk | 6.5.33 | imported from $ghName (no build verify) | $sha |"
  } else {
    "| import | $archName | - | - | imported from $ghName (no upgrade attempt) | $sha |"
  }
  Add-Content $logFile $logRow
}

# Final cleanup
if (Test-Path $tempClones) { Remove-Item -Path $tempClones -Recurse -Force }

Write-Host ""
Write-Host "=== Imports done. Disk: $((Get-PSDrive C).Free / 1GB) GB ===" -ForegroundColor Cyan
& git log --oneline -10

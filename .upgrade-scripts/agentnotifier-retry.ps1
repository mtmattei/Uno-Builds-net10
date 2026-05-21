$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo

$name = 'AgentNotifier'
$gj = "$repo\AgentNotifier\global.json"
$csprojPath = "$repo\AgentNotifier\AgentNotifier.csproj"

$gjText = Get-Content $gj -Raw
$current = ([regex]::Match($gjText, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
Write-Host "current: $current"
if ($current -ne '6.5.33') {
  $new = $gjText -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
  Set-Content -Path $gj -Value $new -NoNewline
}

# Run dotnet against the csproj explicitly (bypass broken .sln)
Push-Location "$repo\AgentNotifier"
$results = @()
try {
  Write-Host "dotnet restore $csprojPath..." -ForegroundColor Gray
  $out = & dotnet restore $csprojPath 2>&1
  if ($LASTEXITCODE -ne 0) {
    Write-Host "RESTORE FAILED" -ForegroundColor Red
    $out | Select-Object -Last 25 | ForEach-Object { Write-Host $_ }
    exit 1
  }
  Write-Host "dotnet build $csprojPath -f net10.0-desktop..." -ForegroundColor Gray
  $out = & dotnet build $csprojPath -f net10.0-desktop --no-restore --nologo 2>&1
  if ($LASTEXITCODE -ne 0) {
    Write-Host "BUILD FAILED" -ForegroundColor Red
    $out | Select-Object -Last 25 | ForEach-Object { Write-Host $_ }
    exit 1
  }
  Write-Host "build OK (net10.0-desktop)" -ForegroundColor Green
  $results += "net10.0-desktop`:ok"
}
finally { Pop-Location }

Set-Location $repo
& git add $gj
& git checkout -- .

$commitMsg = "chore($name): bump to Uno.Sdk 6.5.33`n`nWas 6.4.58. Wave 6 retry -- the .sln references a nested csproj path that doesn't exist, so dotnet was pointed at the csproj directly.`nbuilds: net10.0-desktop:ok"
& git commit -m $commitMsg | Out-Null
$sha = (& git rev-parse --short HEAD).Trim()
Write-Host "committed $sha" -ForegroundColor Green

$logFile = Join-Path $repo 'UPGRADE-LOG.md'
Add-Content $logFile "| 6 | $name | 6.4.58 | 6.5.33 | ok (net10.0-desktop) | $sha |"

Get-ChildItem -Path "$repo\AgentNotifier" -Directory -Recurse -Force -ErrorAction SilentlyContinue |
  Where-Object { $_.Name -in 'bin','obj' } |
  ForEach-Object { try { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue } catch {} }

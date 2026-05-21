$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo

$gj = "$repo\Sanctum\global.json"
$csprojPath = "$repo\Sanctum\Sanctum\Sanctum.csproj"

# Bump global.json
$gjText = Get-Content $gj -Raw
$current = ([regex]::Match($gjText, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
Write-Host "current: $current"
if ($current -ne '6.5.33') {
  $new = $gjText -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
  Set-Content -Path $gj -Value $new -NoNewline
}

# Read TFMs (Sanctum has no iOS to strip per inspect)
$csText = Get-Content $csprojPath -Raw
$m = [regex]::Match($csText, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
$tfms = ($m.Groups[1].Value -split ';') | Where-Object { $_ -and $_.Trim() }
Write-Host "TFMs: $($tfms -join ', ')"

Push-Location "$repo\Sanctum"
$results = @()
try {
  Write-Host "dotnet restore..." -ForegroundColor Gray
  & dotnet restore 2>&1 | Out-Null
  if ($LASTEXITCODE -ne 0) { Write-Host "RESTORE FAILED" -ForegroundColor Red; exit 1 }
  foreach ($tfm in $tfms) {
    Write-Host "dotnet build -f $tfm..." -ForegroundColor Gray
    $out = & dotnet build -f $tfm --no-restore --nologo 2>&1
    if ($LASTEXITCODE -ne 0) {
      Write-Host "BUILD FAILED ($tfm)" -ForegroundColor Red
      $out | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
      exit 1
    }
    Write-Host "OK ($tfm)" -ForegroundColor Green
    $results += "$tfm`:ok"
  }
}
finally { Pop-Location }

Set-Location $repo
& git add $gj
& git add "Sanctum\Sanctum\Platforms\Android\Resources\drawable\uno_splash_image.xml"
& git add "Sanctum\Sanctum\Platforms\Android\Resources\values\colors.xml"
& git checkout -- .

$builds = $results -join ', '
$msg = "fix(Sanctum): add missing Android splash resources + bump to Uno.Sdk 6.5.33`n`nWas $current. Was blocked on APT2260 missing drawable/uno_splash_image.`nAdded Platforms/Android/Resources/drawable/uno_splash_image.xml and values/colors.xml copied from PuckUp's working scaffold (the auto-generated Resizetizer assets that were not committed).`nbuilds: $builds"
& git commit -m $msg | Out-Null
$sha = (& git rev-parse --short HEAD).Trim()
Write-Host "committed $sha" -ForegroundColor Green

$logFile = "$repo\UPGRADE-LOG.md"
Add-Content $logFile "| fix | Sanctum | $current | 6.5.33 | ok ($builds); added Android splash resources | $sha |"

Get-ChildItem -Path "$repo\Sanctum" -Directory -Recurse -Force -ErrorAction SilentlyContinue |
  Where-Object { $_.Name -in 'bin','obj' } |
  ForEach-Object { try { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue } catch {} }

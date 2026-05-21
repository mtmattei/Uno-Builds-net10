# Retry SpaceXhistory - top dir has stray .sln files; dotnet must be pointed at the csproj.

$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location $repo

$name = 'SpaceXhistory'
$gj1 = Join-Path $repo 'SpaceXhistory\global.json'
$gj2 = Join-Path $repo 'SpaceXhistory\SpaceXhistory\global.json'

$csprojPath = Get-ChildItem -Path (Join-Path $repo 'SpaceXhistory') -Filter *.csproj -Recurse |
  Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\' } |
  Select-Object -First 1 -ExpandProperty FullName
Write-Host "csproj: $csprojPath"

$iosTfmsToStrip = @('net10.0-ios','net10.0-iossimulator','net10.0-maccatalyst')

# iOS strip
$csText = Get-Content $csprojPath -Raw
$tfmMatch = [regex]::Match($csText, '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')
$tag = $tfmMatch.Value
$list = $tfmMatch.Groups[1].Value
$kept = ($list -split ';') | Where-Object { $iosTfmsToStrip -notcontains $_ }
$newList = $kept -join ';'
if ($newList -ne $list) {
  $newTag = $tag -replace [regex]::Escape($list), $newList
  $csText = $csText -replace [regex]::Escape($tag), $newTag
  Set-Content -Path $csprojPath -Value $csText -NoNewline
  Write-Host "stripped: $list -> $newList" -ForegroundColor Yellow
}

$tfms = ($newList -split ';') | Where-Object { $_ -and $_.Trim() }
Write-Host "TFMs: $($tfms -join ', ')"

# Bump both global.json files
foreach ($gj in @($gj1, $gj2)) {
  if (Test-Path $gj) {
    $text = Get-Content $gj -Raw
    $current = ([regex]::Match($text, '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value
    Write-Host "$gj current: $current"
    if ($current -ne '6.5.33') {
      $new = $text -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
      Set-Content -Path $gj -Value $new -NoNewline
    }
  }
}

# Run dotnet from the csproj's own directory and pass the csproj explicitly
$csprojDir = Split-Path -Parent $csprojPath
Push-Location $csprojDir
$results = @()
try {
  Write-Host "dotnet restore $csprojPath..." -ForegroundColor Gray
  $out = & dotnet restore $csprojPath 2>&1
  if ($LASTEXITCODE -ne 0) {
    Write-Host "RESTORE FAILED" -ForegroundColor Red
    $out | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
    exit 1
  }
  foreach ($tfm in $tfms) {
    Write-Host "dotnet build $csprojPath -f $tfm..." -ForegroundColor Gray
    $out = & dotnet build $csprojPath -f $tfm --no-restore --nologo 2>&1
    if ($LASTEXITCODE -ne 0) {
      Write-Host "BUILD FAILED ($tfm)" -ForegroundColor Red
      $out | Select-Object -Last 25 | ForEach-Object { Write-Host $_ }
      exit 1
    }
    Write-Host "build OK ($tfm)" -ForegroundColor Green
    $results += "$tfm`:ok"
  }
}
finally { Pop-Location }

Set-Location $repo
& git add $gj1 $gj2 $csprojPath
& git checkout -- .

$builds = $results -join ', '
$commitMsg = "chore($name): bump to Uno.Sdk 6.5.33`n`nStripped iOS/MacCatalyst TFMs per upgrade spec section 2.3.`n`nWas 6.4.58. Wave 3b retry -- top dir had stray sibling .sln files, so dotnet was pointed at the csproj explicitly.`nbuilds: $builds"
& git commit -m $commitMsg | Out-Null
$sha = (& git rev-parse --short HEAD).Trim()
Write-Host "committed $sha" -ForegroundColor Green

$logFile = Join-Path $repo 'UPGRADE-LOG.md'
Add-Content $logFile "| 3b | $name | 6.4.58 | 6.5.33 | ok ($builds); stripped iOS/MacCatalyst | $sha |"

# Cleanup
Get-ChildItem -Path (Join-Path $repo 'SpaceXhistory') -Directory -Recurse -Force -ErrorAction SilentlyContinue |
  Where-Object { $_.Name -in 'bin','obj' } |
  ForEach-Object { try { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue } catch {} }

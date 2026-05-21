$ErrorActionPreference = 'Continue'
$repo = 'C:\temp\Uno-Builds-net10'
Set-Location "$repo\SantaTracker\SantaTracker\SantaTracker"

$gj = "$repo\SantaTracker\global.json"
$t = Get-Content $gj -Raw
$n = $t -replace '"Uno\.Sdk"\s*:\s*"[^"]+"', '"Uno.Sdk": "6.5.33"'
Set-Content -Path $gj -Value $n -NoNewline

$props = "$repo\SantaTracker\Directory.Packages.props"
$pt = Get-Content $props -Raw
if ($pt -notmatch 'Mapsui\.Uno\.WinUI') {
  $pt = $pt -replace '<ItemGroup>\s*</ItemGroup>', "<ItemGroup>`n    <PackageVersion Include=`"Mapsui.Uno.WinUI`" Version=`"5.0.0`" />`n  </ItemGroup>"
  Set-Content -Path $props -Value $pt -NoNewline
}

dotnet restore "$repo\SantaTracker\SantaTracker\SantaTracker\SantaTracker.csproj" 2>&1 | Select-Object -Last 8 | ForEach-Object { Write-Host $_ }
Write-Host '---BUILD---'
dotnet build "$repo\SantaTracker\SantaTracker\SantaTracker\SantaTracker.csproj" -f net10.0-windows10.0.26100 --no-restore --nologo 2>&1 | Select-Object -Last 30 | ForEach-Object { Write-Host $_ }

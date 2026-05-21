$names = @('HockeyBarn','Thermostat','UnoWallet','FieldOpsPro')
foreach ($name in $names) {
  Write-Host "=== $name ==="
  $base = Join-Path 'C:\temp\Uno-Builds-net10' $name
  $assetsDirs = Get-ChildItem -Path $base -Recurse -Directory -Filter 'Assets' -ErrorAction SilentlyContinue |
                Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\' }
  if (-not $assetsDirs) {
    Write-Host "  NO Assets dir found"
    continue
  }
  foreach ($a in $assetsDirs) {
    Write-Host "  Assets at: $($a.FullName.Replace($base, ''))"
    Get-ChildItem -Path $a.FullName -Recurse |
      Where-Object { -not $_.PSIsContainer } |
      Select-Object -First 12 |
      ForEach-Object { Write-Host "    $($_.FullName.Substring($a.FullName.Length))" }
  }
}

$repos = @(
  'UnoComposer','UnoOrbit','Thermostat-Build','driftline','ChefsTest',
  'Text-Grab.Uno','DigitalFidget','Hive','Carousel','memory-drift',
  'parallax-invitation-cards','SnackBar'
)
foreach ($r in $repos) {
  Write-Host "=== $r ==="
  $out = gh repo view "mtmattei/$r" --json description,createdAt,pushedAt,defaultBranchRef 2>&1
  if ($LASTEXITCODE -eq 0) {
    Write-Host $out
    Write-Host "  README:"
    $readme = gh api "repos/mtmattei/$r/readme" --jq '.content' 2>$null
    if ($readme) {
      $decoded = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($readme))
      $decoded -split "`n" | Select-Object -First 8 | ForEach-Object { Write-Host "    $_" }
    } else { Write-Host "    (no README)" }
  } else { Write-Host "  (not accessible)" }
  Write-Host ""
}

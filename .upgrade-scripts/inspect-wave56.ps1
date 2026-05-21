$projects = @('ADE','AdaptiveInput','AnimatedExtendedSplashScreen','ExtendedSplashHDdemo','HockeyBarn','HorizontalCalendar','HorizontalCalendarControl','Pens','PuckUp','YUL','ZaraApp','matrix','radial-action-menu','split-flap','vtrack','KineticSculptor','UnoEnterpriseApp','UnoWallet','AgentNotifier')

foreach ($name in $projects) {
  $proj = Join-Path 'C:\temp\Uno-Builds-net10' $name
  if (-not (Test-Path $proj)) { Write-Host ("{0,-30} NO-DIR" -f $name); continue }
  $gjs = Get-ChildItem -Path $proj -Filter global.json -Recurse -ErrorAction SilentlyContinue
  $cs = Get-ChildItem -Path $proj -Filter *.csproj -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\' } |
        Select-Object -First 1

  $gjPath = if ($gjs) { $gjs[0].FullName.Replace('C:\temp\Uno-Builds-net10\','') } else { 'NO-GJ' }
  $gjCount = if ($gjs) { $gjs.Count } else { 0 }
  $sdk = if ($gjs) { ([regex]::Match((Get-Content $gjs[0].FullName -Raw), '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value } else { '-' }
  $tfm = if ($cs) { ([regex]::Match((Get-Content $cs.FullName -Raw), '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')).Groups[1].Value } else { 'NO-CSPROJ' }

  Write-Host ("{0,-30} gj={1,-60} cnt={2} sdk={3,-15} tfm={4}" -f $name, $gjPath, $gjCount, $sdk, $tfm)
}

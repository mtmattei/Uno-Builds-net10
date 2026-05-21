$projects = @('ToolkitBench','Composer','MSYouTube','Meridian','ClaudeDash','ConfPass','FluxTransit','Liveline','VoxelWarehouse','liquidMorph')

foreach ($name in $projects) {
  $proj = Join-Path 'C:\temp\Uno-Builds-net10' $name
  $gj = Get-ChildItem -Path $proj -Filter global.json -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
  $cs = Get-ChildItem -Path $proj -Filter *.csproj -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\' } |
        Select-Object -First 1

  $sdk = if ($gj) { ([regex]::Match((Get-Content $gj.FullName -Raw), '"Uno\.Sdk"\s*:\s*"([^"]+)"')).Groups[1].Value } else { 'NO-GJ' }
  $tfm = if ($cs) { ([regex]::Match((Get-Content $cs.FullName -Raw), '<TargetFrameworks?>([^<]+)</TargetFrameworks?>')).Groups[1].Value } else { 'NO-CSPROJ' }
  Write-Host ("{0,-16} sdk={1,-12} tfm={2}" -f $name, $sdk, $tfm)
}

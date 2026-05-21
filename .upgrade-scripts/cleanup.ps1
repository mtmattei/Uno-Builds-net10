# Free up disk space after Wave 3 cascade.
# Deletes bin/ and obj/ trees inside the repo, and clears NuGet scratch.

$repo = 'C:\temp\Uno-Builds-net10'
$before = (Get-PSDrive C).Free
Write-Host ("Disk free before: {0:N1} GB" -f ($before/1GB))

Write-Host ""
Write-Host "Removing bin/ and obj/ under repo..."
Get-ChildItem -Path $repo -Directory -Recurse -Force -ErrorAction SilentlyContinue |
  Where-Object { $_.Name -in 'bin','obj' -and $_.FullName -notmatch '\\\.git\\' } |
  ForEach-Object {
    try {
      Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    } catch {}
  }

Write-Host "Removing NuGet scratch..."
$scratch = "$env:LOCALAPPDATA\Temp\NuGetScratch"
if (Test-Path $scratch) {
  Remove-Item -Path "$scratch\*" -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Removing nuget HTTP cache (re-downloadable)..."
& dotnet nuget locals http-cache --clear | Out-Null

$after = (Get-PSDrive C).Free
$freed = $after - $before
Write-Host ""
Write-Host ("Disk free after:  {0:N1} GB" -f ($after/1GB))
Write-Host ("Freed:            {0:N1} GB" -f ($freed/1GB))

$drive = Get-PSDrive C
$freeGB = [math]::Round($drive.Free/1GB, 1)
$usedGB = [math]::Round($drive.Used/1GB, 1)
Write-Host "C: drive: $freeGB GB free, $usedGB GB used"

Write-Host ""
Write-Host "=== git log -10 ==="
Set-Location 'C:\temp\Uno-Builds-net10'
& git log --oneline -10

Write-Host ""
Write-Host "=== git status ==="
& git status --short

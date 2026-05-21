$procs = Get-Process | Where-Object { $_.ProcessName -like '*ChefsTest2*' -or ($_.MainWindowTitle -ne $null -and $_.MainWindowTitle -like '*ChefsTest2*') }
if ($procs.Count -eq 0) { Write-Output "NO_CHEFSTEST2_PROCESS"; exit 0 }
$procs | Select-Object Id, ProcessName, MainWindowTitle | Format-Table -AutoSize | Out-String | Write-Output

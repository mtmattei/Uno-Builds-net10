# Single-launch walkthrough: starts ChefsTest2 once, sequentially navigates each
# major route via the CHEFSTEST2_INITIAL_ROUTE env var override + a brief in-app
# tab-key drill where natural, snaps a screenshot per page.

$shotsDir = "C:\Users\Platform006\OneDrive - Uno Platform\Desktop\unOS\AI-builds\ChefsFT-test\results\screenshots\test-2"
$snap     = "C:\Users\Platform006\OneDrive - Uno Platform\Desktop\unOS\AI-builds\ChefsFT-test\test-2-figma-mcp\.run\snap.ps1"
$exe      = "C:\Users\Platform006\OneDrive - Uno Platform\Desktop\unOS\AI-builds\ChefsFT-test\test-2-figma-mcp\ChefsTest2\bin\Debug\net10.0-desktop\ChefsTest2.exe"

if (-not (Test-Path $shotsDir)) { New-Item -ItemType Directory -Path $shotsDir | Out-Null }
Get-Process -Name ChefsTest2 -ErrorAction SilentlyContinue | Stop-Process -Force

$pages = @(
    "Onboarding",
    "Login",
    "Register",
    "Home",
    "Search",
    "Filters",
    "Favorites",
    "Settings",
    "Notifications",
    "NearMeMap",
    "Profile",
    "CreateCookbook"
)

foreach ($page in $pages) {
    Get-Process -Name ChefsTest2 -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Milliseconds 300
    $env:CHEFSTEST2_INITIAL_ROUTE = $page
    $p = Start-Process -FilePath $exe -PassThru
    Start-Sleep -Milliseconds 4500
    $out = Join-Path $shotsDir "${page}.png"
    & $snap -OutPath $out
    Start-Sleep -Milliseconds 200
    Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
}
Write-Output "DONE"

# Blocked projects

Projects where the per-project 30-min fix cap (UPGRADE-SPEC.md section 8) was hit during the Uno.Sdk 6.5.33 / .NET 10 upgrade.

| Wave | Project | Last good SDK / TFM | Error class | Hypothesis |
|---|---|---|---|---|
| 2 | ConfPass | 6.4.58 / net10.0-android | Android build: `Uno.Toolkit.UI.ShadowContainer` and `ShadowCollection` not found; XAML source-gen cascades into 35 errors | Uno.Toolkit Android assembly missing or renamed `ShadowContainer`/`ShadowCollection` between Toolkit versions paired with Uno.Sdk 6.5.33. Desktop/wasm/windows builds not yet verified. Needs Toolkit version inspection. |
| 2 | FluxTransit | 6.4.58 / net10.0-windows10.0.26100 | NETSDK1005: `FluxTransit.DataContracts` subproject has no `net10.0-windows10.0.26100` target | Multi-csproj project. DataContracts subproject likely declares only `net10.0` (base lib). The bump exposed a structural TFM mismatch that 6.4.58 either tolerated or didn't validate. Fix would require either adding the windows TFM to DataContracts or aligning the reference shape — out-of-scope refactor (spec section 2.3). Desktop/wasm build status not yet verified. |
| 3 | FieldOpsPro | 6.4.53 / net10.0-android | APT2261 png compile error on `Resources\drawable-nodpi\updated_layout.png` | Android resource pipeline rejects a png file. Likely a corrupt or oversized png that newer aapt2 rejects. Fix would be to validate/replace the png — out of upgrade scope. Desktop/wasm builds not yet verified. |
| 3 | Gridform | 6.5.31 / net10.0-windows10.0.26100 | CS0115 `IsometricCanvas.RenderOverride(SKCanvas, Size)` has no method to override | SkiaSharp `SKCanvasElement.RenderOverride` signature changed between SkiaSharp versions paired with Uno.Sdk 6.5.31 vs 6.5.33. Source code change required. Out of upgrade scope per spec section 2.3 (no refactors). |

## Disk-cascade entries (cleared 2026-05-20)

The following projects appeared in BLOCKED.md after Wave 3 failed but were caused by C: drive running out of space (0.3 GB free). After bin/obj/NuGet scratch cleanup (recovered ~20 GB), these will be retried in Wave 3b:

- InfiniteImage (MSB3026 dll copy retry storm)
- PrecisionDial (restore failed under disk pressure)
- QuoteCraft (explicit "not enough space" message)
- ReservoomUno (explicit "not enough space" message)
- RivTes (package download failure)
- SalesDashboard (MSB4025 file lock from concurrent operations)
| 3b | QuoteCraft | 6.5.31 / restore | build failed |   Determining projects to restore... / C:\temp\Uno-Builds-net10\QuoteCraft\src\QuoteCraft\QuoteCraft.csproj : error NU1605: Warning As Error: Detected package downgrade: SkiaSharp from 3.119.2 to 3.119.1. Reference the p |
| 3b | ReservoomUno | 6.5.31 / net10.0-browserwasm | build failed | C:\Program Files\dotnet\packs\Microsoft.NET.Runtime.WebAssembly.Sdk\10.0.1\Sdk\WasmApp.Common.targets(771,5): warning WASM0001:     System.Int32 sqlite3_config_int_arm64cc(System.Int32, System.IntPtr, System.IntPtr, Syst |
| 3b | Sanctum | 6.4.58 / net10.0-android | build failed | Resources\values\Styles.xml(2): error APT2260:  [C:\temp\Uno-Builds-net10\Sanctum\Sanctum\Sanctum.csproj::TargetFramework=net10.0-android] / Resources\values\Styles.xml(2): error APT2260: resource drawable/uno_splash_ima |
| 3b | SantaTracker | 6.4.58 / restore | build failed |   Determining projects to restore... / C:\temp\Uno-Builds-net10\SantaTracker\SantaTracker\SantaTracker\SantaTracker.csproj : error NU1010: The following PackageReference items do not define a corresponding PackageVersion |
| 3b | SpaceXhistory | 6.4.58 / restore | build failed | MSBUILD : error MSB1011: Specify which project or solution file to use because this folder contains more than one project or solution file. |
| 3b | Wellmetrix | 6.4.58 / net10.0-windows10.0.26100 | build failed | C:\Users\Platform006\.nuget\packages\microsoft.windowsappsdk\1.7.250909003\buildTransitive\Microsoft.UI.Xaml.Markup.Compiler.interop.targets(764,9): error MSB3073: The command ""C:\Users\Platform006\.nuget\packages\micro |
| 3b | Zara | 6.4.26 / net10.0-android | build failed | Resources\values\Styles.xml(2): error APT2260:  [C:\temp\Uno-Builds-net10\Zara\Zara\Zara.csproj::TargetFramework=net10.0-android] / Resources\values\Styles.xml(2): error APT2260: resource drawable/uno_splash_image (aka c |
| 3b | FormaEspresso | 6.4.53 / net10.0-android | build failed | C:\temp\Uno-Builds-net10\FormaEspresso\FormaEspresso\Controls\BrewingCup.xaml(143,18): error UXAML0001: Invalid Thickness value '0,0,0,{x:Bind local:BrewingCup.GetCremaOffset(Progress), Mode=OneWay}'. Each component must |
| 4 | SmartNotes | 6.3.28 / net10.0-desktop | build failed | C:\temp\Uno-Builds-net10\SmartNotes\SmartNotes\MainPage.xaml.cs(28,42): error CS1061: 'DatabaseService' does not contain a definition for 'GetNoteById' and no accessible extension method 'GetNoteById' accepting a first a |
| 4 | Thermostat | 6.4.58 / net10.0-android | build failed |  / C:\temp\Uno-Builds-net10\Thermostat\test\test.csproj : warning NU1903: Package 'Microsoft.Kiota.Abstractions' 1.21.0 has a known high severity vulnerability, https://github.com/advisories/GHSA-7j59-v9qr-6fq9 [TargetFr |
| 5 | AdaptiveInput | 6.4.58 / net10.0-android | build failed | Resources\values\Styles.xml(2): error APT2260:  [C:\temp\Uno-Builds-net10\AdaptiveInput\AdaptiveInputDemo\AdaptiveInputDemo\AdaptiveInputDemo.csproj::TargetFramework=net10.0-android] / Resources\values\Styles.xml(2): err |
| 5 | HockeyBarn | 6.4.24 / net10.0-android | build failed | C:\temp\Uno-Builds-net10\HockeyBarn\HockeyBarn\HockeyBarn.csproj : warning NU1903: Package 'Microsoft.Kiota.Abstractions' 1.21.0 has a known high severity vulnerability, https://github.com/advisories/GHSA-7j59-v9qr-6fq9  |
| 5 | vtrack | 6.4.40 / net10.0-android | build failed | Resources\values\Styles.xml(2): error APT2260:  [C:\temp\Uno-Builds-net10\vtrack\VTrack\VTrack.csproj::TargetFramework=net10.0-android] / Resources\values\Styles.xml(2): error APT2260: resource drawable/uno_splash_image  |
| 6 | KineticSculptor | 6.6.0-dev.230 / net10.0-browserwasm | build failed | wasm-ld : error : lto.tmp: undefined symbol: sk_pathbuilder_add_rrect [C:\temp\Uno-Builds-net10\KineticSculptor\KineticSculptor\KineticSculptor.csproj::TargetFramework=net10.0-browserwasm] / wasm-ld : error : lto.tmp: un |
| 6 | AnimatedExtendedSplashScreen | 6.4.0-dev.50 / net10.0-android | build failed | Resources\values\Styles.xml(2): error APT2260:  [C:\temp\Uno-Builds-net10\AnimatedExtendedSplashScreen\AnimatedExtendedSplashScreen\AnimatedExtendedSplashScreen.csproj::TargetFramework=net10.0-android] / Resources\values |
| 6 | UnoWallet | 6.4.0-dev.85 / net10.0-android | build failed |  / C:\temp\Uno-Builds-net10\UnoWallet\UnoWallet\UnoWallet.csproj : warning NU1903: Package 'Microsoft.Kiota.Abstractions' 1.21.0 has a known high severity vulnerability, https://github.com/advisories/GHSA-7j59-v9qr-6fq9  |
| 6 | AgentNotifier | 6.4.58 / restore | build failed | C:\Program Files\dotnet\sdk\10.0.200\NuGet.targets(519,5): error MSB3202: The project file "C:\temp\Uno-Builds-net10\AgentNotifier\AgentNotifier\AgentNotifier.csproj" was not found. [C:\temp\Uno-Builds-net10\AgentNotifie |

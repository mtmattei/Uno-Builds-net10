# Known issues

Unresolved warnings or platform skips encountered during the Uno.Sdk 6.5.33 / .NET 10 upgrade. Documenting per UPGRADE-SPEC.md section 11.

## Text-Grab — skipped this pass

Per UPGRADE-SPEC.md section 7.3, `Text-Grab` is mid WPF-to-Uno migration and tracked under TheJoeFin/Text-Grab fork. No `global.json` at root and TFMs already on `net10.0-*`. Not touched in this upgrade pass.

## `ColorPaletteOverride.xaml` regenerated to default Material palette during build

When `dotnet build` runs on Uno.Sdk 6.5.33 for projects using Uno Material themes, the build process rewrites `Styles/ColorPaletteOverride.xaml` to a generated default (purple `#5946D2`) — overwriting committed custom themes.

**Observed on:** MCP-blog, Nexus (industrial green), Olea (olive/gold). All 3 had their custom palettes overwritten during Wave 1 builds. Changes were reverted; commits only include `global.json`.

**Hypothesis:** Uno Material 5.1+ generator now sources from `ColorPaletteOverride.json` (per the file's header comment). If the json sidecar is missing, the generator writes a default. The committed XAML was hand-edited and has no matching json source.

**Action:** Custom palettes preserved in git history. Future work to (1) extract palette to a `.json` sidecar so the generator regenerates the intended colors, OR (2) move the file out of the path the generator writes to.

## Wave 1 / Wave 2 build verification

Wave 1 (10 projects) verified `net10.0-desktop` only. `net10.0-browserwasm` and `net10.0-windows10.0.*` not built for Wave 1 projects that declare those TFMs. Follow-up sweep needed after the wave completes.

## Common Android blocker — APT2260 `uno_splash_image` missing (5 projects)

Affected: AdaptiveInput, Sanctum, Zara, vtrack, AnimatedExtendedSplashScreen.

Error pattern:
```
Resources\values\Styles.xml(2): error APT2260: resource drawable/uno_splash_image not found
```

These projects reference `@drawable/uno_splash_image` in `Resources/values/Styles.xml` but lack the asset under `Resources/drawable*/`. Newer aapt2 (paired with .NET 10 / Uno.Sdk 6.5.33 Android workload) rejects this where older toolchains tolerated it.

**Fix (out of upgrade scope):** add a `uno_splash_image.png` to `Resources/drawable-nodpi/` per project, or update the Styles.xml reference to a placeholder. Both are project-level content changes outside the SDK/TFM bump.

## Other source-level blockers

| Project | Issue |
|---|---|
| SmartNotes | `DatabaseService.GetNoteById` missing — pre-existing code gap, surfaced by stricter analysis |
| FormaEspresso | `x:Bind` inside `Thickness` literal — newer Uno XAML parser rejects mixed literal/binding values |
| Gridform | `SKCanvasElement.RenderOverride(SKCanvas, Size)` no longer has a matching base signature in newer SkiaSharp |
| SantaTracker | NU1010 — `PackageReference` without matching `PackageVersion` in Directory.Packages.props (CPM strict mode) |
| QuoteCraft | NU1605 — SkiaSharp pinned at 3.119.2, transitive resolves to 3.119.1 |
| ReservoomUno | WASM0001 — `sqlite3_config_int_arm64cc` P/Invoke signature mismatch in current SQLitePCLRaw vs Wasm runtime |
| Wellmetrix | MSB3073 — Microsoft.UI.Xaml.Markup.Compiler interop failure on Windows TFM |
| ConfPass | `Uno.Toolkit.UI.ShadowContainer` / `ShadowCollection` not resolved on Android |
| FluxTransit | `FluxTransit.DataContracts` subproject missing `net10.0-windows10.0.26100` TFM |
| FieldOpsPro | APT2261 — png resource `Resources\drawable-nodpi\updated_layout.png` rejected by aapt2 |
| KineticSculptor | wasm-ld undefined symbol `sk_pathbuilder_add_rrect` — preview SkiaSharp 4.x native symbol absent in the bundled lib for 6.7.0-dev.52 |

All of the above are project-content or source-level issues that the upgrade pass deliberately did not touch (spec section 2.3). Each should be triaged in a follow-up.

## Broken solution layout — AgentNotifier

`AgentNotifier\AgentNotifier.sln` references `AgentNotifier\AgentNotifier.csproj` (nested), but the csproj is at the same level as the sln. Running `dotnet restore` from the project root picks up the sln and fails to find the csproj. Worked around by pointing dotnet at the csproj directly during the upgrade (commit d3d775e). The sln itself is still broken — fix in a follow-up.

## Stray sibling .sln files in SpaceXhistory

`SpaceXhistory/` top level contains `.sln` files for unrelated sibling projects (LiquidMorph, RadialActionMenu, SplitFlap, Thermostat, Vitalis, VTrack, Wellmetrix, YUL). `dotnet restore` from that dir errors with MSB1011 (multiple solution files). Worked around by pointing dotnet at the csproj directly. The stray .sln files are likely contamination from an earlier sync and should be removed in a follow-up.

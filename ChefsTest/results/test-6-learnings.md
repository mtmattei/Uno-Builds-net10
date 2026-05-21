# Test 6 — Learnings, gotchas, and bugs encountered

**Session:** test-6-blind-prd-rerun (2026-04-24)
**Stack:** .NET 10.0.200 / Uno.Sdk 6.5.31 / MVUX / Material / Skia / x:Bind+Binding mix
**Targets attempted:** net10.0-desktop, net10.0-browserwasm, net10.0-android (all green; iOS + Windows-WinUI not attempted)

This document is a postmortem of every friction point this session hit, with the resolution. It lives next to `test-6.md` so the next test run (1 / 2 / 3 / 4 / 5 / 7) can skip the same cliffs.

---

## 1. Models & DTOs

### 1.1 `KE0001` — records with `Id` MUST be `partial`
**Symptom:** First Desktop build failed with 5 copies of:
> error KE0001: The record 'X' is eligible to IKeyEquatable generation (due to 'Id') but is not partial.

**Cause:** MVUX's IKeyEquatable source-generator runs on every record that has a key-eligible property and emits a partial-class extension. If the record isn't `partial`, the generator can't extend it and the build fails hard.

**Fix:** Add `partial` to `RecipeData`, `CookbookData`, `UserData`, `ReviewData`, `CategoryData`. Records *without* an `Id` (e.g., `IngredientData`, `StepData`, `NutritionData`, `LoginRequest`, `NotificationData`, `OnboardingSlide`) don't need it.

### 1.2 Fixture JSON `TimeSpan` uses two encodings
**Symptom:** Default System.Text.Json deserialization throws on `Recipes.json` even though `Cookbooks.json` reads fine.

**Cause:** `Recipes.json` encodes `TimeSpan` as `{"ticks": 9600000000}` (object form). `Cookbooks.json` encodes the same field as `"00:10:00"` (string form).

**Fix:** Custom `TimeSpanJsonConverter` that reads both via a `reader.TokenType` switch — `JsonTokenType.String` → `TimeSpan.Parse`, `JsonTokenType.StartObject` → walk to the `ticks` property.

### 1.3 Fixture JSON has fields not on the API-CONTRACT DTO
**Symptom:** Strict deserialization would either fail or drop data.

**Cause:** `Recipes.json` includes `Save: bool` and `Creator: { ... full UserData ... }`. `Cookbooks.json` includes `PinsNumber: int`. None of these are on the contract's DTO list.

**Fix:** Add them to the records anyway with `[JsonPropertyName]` where the casing differs. Don't try to keep DTO surface "pure" — the fixture is the source of truth.

### 1.4 Lenient JSON options are mandatory
Fixtures contain trailing commas and odd whitespace. Use:
```csharp
new JsonSerializerOptions {
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    Converters = { new TimeSpanJsonConverter() }
}
```

---

## 2. MVUX feeds / states

### 2.1 Covariance trap — `IImmutableList<T>` vs `ImmutableList<T>`
**Symptom:** ~10 copies of:
> error CS0266: Cannot implicitly convert type 'IFeed<ImmutableList<T>>' to 'IFeed<IImmutableList<T>>'

**Cause:** `List<T>.ToImmutableList()` returns the concrete `ImmutableList<T>`. C# does NOT bridge `IFeed<ImmutableList<T>>` → `IFeed<IImmutableList<T>>` via implicit covariance — `IFeed<T>` is invariant in `T`.

**Fix:** Declare every feed as `IFeed<ImmutableList<T>>` (concrete), not the interface form. Same applies to `IState<ImmutableList<T>>`. As a bonus, the `State<...>.Value(this, () => ImmutableList<T>.Empty)` overload-resolution ambiguity (CS0121 between `Func<T>` and `Func<Option<T>>`) goes away too.

### 2.2 `Feed.Select(async ...)` doesn't compile — use `SelectAsync`
**Symptom:**
> error CS0411: The type arguments for method 'Feed.Select<TSource, TResult>' cannot be inferred from the usage.

**Cause:** `IFeed<T>.Select(Func<T, T2>)` is the sync overload. The async-selector overload is named `SelectAsync` and takes `Func<T, CancellationToken, ValueTask<T2>>`.

**Fix:** Wherever the selector lambda is `async (x, ct) => ...`, use `.SelectAsync(...)`. Hit at: `MainModel.SearchResults`, `CookingModel.ActiveStep`/`IsLastStep`, `NotificationsModel.Notifications`.

### 2.3 `IFeed<T>.Refresh()` requires a `CancellationToken`
**Symptom:**
> error CS1501: No overload for method 'Refresh' takes 0 arguments

**Fix:** Don't call `.Refresh()` manually in most cases. Structure your feeds so they observe an `IState` (or another `IFeed`) that changes when the underlying data changes — MVUX re-evaluates automatically. If you genuinely need a token-bearing refresh, pass one explicitly.

### 2.4 `BindableMainModel` "type not found" cascades from upstream errors
**Symptom:** While KE0001 errors are blocking the build, every reference to `BindableMainModel` (or any other generated bindable) fails with CS0246 "type or namespace not found".

**Cause:** Source generators don't emit their output if the input source has earlier compile failures. Bindable types only appear after KE0001 / partial-record / SelectAsync errors are fixed.

**Fix:** Resolve KE0001 first. Don't chase CS0246 separately — it's a downstream symptom.

**Side benefit:** To avoid hard-coupling to the generator's exact naming convention, `ViewHelpers.FindMainModel` walks the visual tree and matches `GetType().Name.EndsWith("BindableMainModel")` reflectively. That way, an MVUX rename (`BindableMainModel` → `MainModel.Bindable` or similar) won't need a code edit here.

---

## 3. Navigation (Uno.Extensions.Navigation)

### 3.1 `NavigateDataAsync` has no `route:` parameter
**Symptom:**
> error CS1739: The best overload for 'NavigateDataAsync' does not have a parameter named 'route'

**Cause:** `NavigateDataAsync(this, TData)` picks the route from the registered `DataViewMap<TPage, TModel, TData>`. There is no API to override the route in that call.

**Fix:** When you want to navigate to a *named* route AND pass data, use `NavigateRouteAsync(this, "RouteName", data: payload)`. Hit 3 times: `RecipeDetailModel.StartCooking → "Cooking"`, `CookingModel.Next → "CookingDone"`, `CookbookDetailModel.OpenEdit → "EditCookbook"`.

### 3.2 Route map is flat under Shell
Every named route lives as a `Nested:` entry under the root Shell route. `IsDefault: true` marks the launch route. No multi-level nesting needed for ~18 screens.

---

## 4. XAML / cards

### 4.1 `DataTemplate` items inherit the *item*, not the page's VM
**Symptom:** A `RecipeCard` inside `ListView.ItemTemplate` has `DataContext = RecipeData` (the bound item). Trying to bind `Command="{Binding OpenRecipe}"` reaches into `RecipeData`, not `MainModel`, and silently does nothing.

**Fix:** From the card's code-behind, walk up the visual tree until you hit a `FrameworkElement` whose `DataContext` is the page-level bindable — that's `MainModel`'s bindable proxy. Then call the command on it.

```csharp
var node = VisualTreeHelper.GetParent(this);
while (node != null) {
    if (node is FrameworkElement fe && fe.DataContext is BindableMainModel mm) return mm;
    node = VisualTreeHelper.GetParent(node);
}
```

### 4.2 `x:Bind` needs `x:DataType` on the `DataTemplate`
Without it, `x:Bind` falls back to runtime binding (loses compile-time checking and is slightly slower).

### 4.3 `utu:Chip` selected-state needs the toolkit style applied
The visible delta between selected/unselected only renders if you set `Style="{StaticResource MaterialChipStyle}"` (or equivalent). Without it, `IsChecked` toggles but the visual is identical. This is one of the 18-deviations vs. PRD recorded in `test-6.md`.

### 4.4 `Image` with `.svg` source fails silently on some targets
Default `Image.Source = "ms-appx:///Assets/Images/empty_recipe_light.svg"` doesn't render reliably across Skia targets. SVG support requires `SvgImageSource` or a Toolkit control. Workaround for blind-PRD: replace empty-state SVGs with emoji + text.

### 4.5 Image references that don't resolve don't error
If the asset path is wrong (e.g., we initially pointed `MapPage` at `ms-appx:///Assets/Maps/map.png` which doesn't exist — only `.svg` files live there), the build succeeds and the runtime renders blank. No error, no diagnostic. Always grep the actual `Assets/` tree before referencing.

---

## 5. Build / tooling

### 5.1 `EmitCompilerGeneratedFiles=true` poisons the build
**Symptom:** ~50 copies of `CS0101 'X already contains a definition'` after a single build cycle with the property set.

**Cause:** Adding `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` causes generators to write to a folder under the project (default `obj/.../generated/`, but it can land at the project root). On the *next* build, that folder gets included as ordinary `<Compile>` source AND the generators run again — producing duplicates.

**Fix:** Don't enable this property unless you `<Compile Remove="generated/**" />` it too. To diagnose generator output, build once, copy the relevant file out, then turn the property off and delete the folder. After deleting `generated/`, the build returns to clean.

### 5.2 NU1903 vulnerability warnings are scaffold defaults
Three NU1903 warnings (`System.Security.Cryptography.Xml`, `Tmds.DBus.Protocol`) come from the scaffolded `recommended` preset's transitive deps. They don't fail the build. Not worth chasing during the test run; a real product would override the package versions.

### 5.3 `Uno0001` warnings on WASM + Android are expected
`ItemsWrapGrid.{ItemWidth, ItemHeight, Orientation, MaximumRowsOrColumns}` is not implemented on non-WinUI targets. `ListView`/`GridView` with an `ItemsWrapGrid` panel falls back to default flow layout. Counts:
- Desktop: 35 warnings (mostly nullable-ref + scaffold inherited)
- WASM: 38 (Desktop set + 3 Uno0001)
- Android: 41 (WASM set + AndroidManifest defaults)

### 5.4 `uno.themes.winui.markup` package warning during restore
> warning: Unable to find uno.themes.winui.markup in the Nuget cache.

Comes from `Uno.Dsp.Tasks` referencing it transitively. Doesn't block the build. Our app doesn't use C# Markup, so the missing package is irrelevant.

### 5.5 Build durations on this machine
Rough wall-clock per target with 4-core hot rebuild (no NuGet restore):
- Desktop incremental: ~10s
- WASM incremental: ~30s
- Android incremental: ~2:00 (NDK + AOT analyzers dominate)

Cold-build (first build of a target after `dotnet new`) adds 3–5x.

---

## 6. Runtime / launch

### 6.1 `dotnet run` returns 0 immediately on Skia Desktop
**Symptom:** `dotnet run -f net10.0-desktop --no-build` prints "Using launch settings from..." then exits 0 within a couple seconds. No stack trace, no error. This LOOKS like a startup crash but isn't.

**Cause:** `dotnet run` launches the child `.exe` and detaches as soon as the child process is up. On Skia Desktop, the launched window is its own process; `dotnet run` doesn't wait.

**Diagnostic that works:**
```bash
(./ChefsApp.exe > log 2>&1 &) && sleep 4 && powershell -c "Get-Process -Name ChefsApp"
```
If the process shows up with a `MainWindowTitle` and `Responding=True`, the launch is fine.

### 6.2 Uno's `ILogger` doesn't route to stdout
`enableUnoLogging: true` in `App.xaml.cs` configures logging via `Uno.Extensions.Logging` — that pipeline doesn't write to `Console.Out` by default. So a successful Skia Desktop launch produces ZERO console output beyond the launchSettings line. Empty stdout ≠ silent crash.

### 6.3 `tasklist` from Bash is unreliable; use PowerShell
`tasklist | grep -i ChefsApp` sometimes shows nothing while the process is plainly alive. PowerShell's `Get-Process -Name ChefsApp` always reports correctly. Default to PowerShell for process queries on this stack.

---

## 7. Assets

### 7.1 Asset folder structure is flat-content, not categorized by type
`reference/assets/` mirrors the original sample's `Assets/`: `Categories/`, `Fonts/`, `Icons/`, `Images/`, `Maps/`, `Profiles/`, `Recipes/`, `Splash/`, `Videos/`, `Welcome/`. Just copy the whole tree into `ChefsApp/Assets/` before first build — Uno.Sdk's single-project conventions auto-include them as content.

### 7.2 Fixture JSONs live separately from content
`reference/data/*.json` is the API-CONTRACT-backing fixture. Put it under `Assets/Fixtures/` so `ms-appx:///Assets/Fixtures/Recipes.json` resolves at runtime via `StorageFile.GetFileFromApplicationUriAsync`. Don't mix it with `Assets/Recipes/` (recipe images).

### 7.3 Image format mix
- PNGs: `Categories/`, `Icons/`, `Profiles/`, `Recipes/`, `Welcome/`, `Splash/` — work everywhere with `Image.Source = ms-appx:///...`.
- SVGs: `Images/empty_*_{light,dark}.svg`, `Images/chefsappsignature_*.svg`, `Images/success_*.svg`, `Maps/location_*.svg` — need `SvgImageSource` or Toolkit; not a generic `Image.Source`. We swapped the empty-state SVGs for emoji glyphs in this run because choosing an SVG renderer is a visual decision the blind constraint forbids.

### 7.4 Maps assets only contain pin SVGs
`Maps/` has `location_circle.svg` and `location_pin.svg` — no map tile background. The PRD doesn't pick a tile provider; we showed a placeholder surface instead. (Open question #9 in `PRD.md` flags map tile provider as TBD.)

### 7.5 Fonts are bundled but not wired
`Assets/Fonts/` ships Font Awesome + Material Icons (mentioned in `API-CONTRACT.md` as required for iconography). No control in this build references them — the app uses system type. Wiring them is a visual decision.

---

## 8. Session / experiment

### 8.1 `uno-app` MCP server not loaded in this session
**Symptom:** `.mcp.json` lists `uno-app` under `mcpServers`, but `ToolSearch +uno-app` returns no matches. No `mcp__uno-app__*` tool schemas surface.

**Impact:** Cannot run screenshot + visual-compare against `Chefs-screenshots/` from inside this session — same gap as the prior recorded run on this test folder.

**Fix candidates (untried in this run):**
- Verify `dotnet dnx -y uno.devserver --mcp-app` runs cleanly outside the harness; if it dies on launch, the harness silently drops the server.
- Check whether the harness only loads MCP servers that were present *before* the session started. If yes, applying the starter kit *after* the session opened might be the cause — copy `.mcp.json` first, then restart Claude Code.
- Defer screenshot+compare to a separate session that starts in the test-6 folder with `.mcp.json` already in place.

### 8.2 Forbidden-input list was honored
None of these were read in-session: `Chefs-screenshots/`, `reference/DESIGN.md`, `reference/DESIGN-NOTES.md`, `reference/visual-skill-output/`, `reference/figma-url.txt`, `reference/forbidden/`, any remote Uno Chefs source.

### 8.3 Auto-memory was not used to short-circuit the blind constraint
The auto-memory system at `~/.claude/projects/.../memory/` could in principle persist visual hints across sessions. None were saved or recalled here. For test 6 specifically, that would defeat the negative control — but worth noting that future-test methodology needs to confirm memory isn't leaking visual context between runs.

### 8.4 Time budget
Wall clock for this run: ~33 minutes. Roughly: 2m starter kit + reading inputs, 20m model/page authoring, 8m fix loop (3 build cycles), 3m smoke-launch + report writing.

---

## 9. What worked smoothly (also worth recording)

- `dotnet new unoapp -preset recommended -presentation mvux -markup xaml -theme material -platforms desktop -platforms wasm -platforms android -renderer skia -tfm net10.0` produces a working scaffold on the first try.
- `Feed.Async(async ct => ...)` for the simple "fetch once, expose as IFeed" case is ergonomic and "just works."
- DI registration via `ConfigureServices((context, services) => services.AddSingleton<...>())` in `App.xaml.cs` requires no reflection setup or attributes.
- `Image` with `ms-appx:///Assets/X.png` resolves on all three targets without per-platform tweaks.
- Uno.Material default palette + `MaterialFilledButtonStyle` / `MaterialOutlinedButtonStyle` / `MaterialTextButtonStyle` carry the entire app's button surface without custom styling.
- `utu:SafeArea.Insets="VisibleBounds"` on the page root respects notch/status-bar safe area on phone targets without per-platform code.

---

## 10. Recommendations for the next test run (1 / 2 / 3 / 4 / 5 / 7)

1. **Apply starter kit first**, then restart Claude Code if `uno-app` MCP tools are needed in the same session.
2. **Make all DTO records `partial`** from the start, even if you don't think they need it.
3. **Use concrete `ImmutableList<T>`** in `IFeed<>` and `IState<>` declarations — never the interface.
4. **Use `SelectAsync` for any async feed selector**, never `Select`.
5. **Use `NavigateRouteAsync(this, "Route", data: X)` for routed-with-payload navigation**, not `NavigateDataAsync(..., route:)`.
6. **Don't enable `EmitCompilerGeneratedFiles`** unless you also `<Compile Remove>` the output folder.
7. **Default to `Get-Process -Name X` (PowerShell)** for process-liveness checks, not Bash `tasklist`.
8. **Treat empty stdout from `dotnet run` as success**, not crash, on Skia Desktop. Use the detached-launch + `Get-Process` pattern to verify.
9. **Pre-grep `Assets/` for actual filenames** before referencing them in XAML — missing assets fail silently at runtime.
10. **Reuse this file**: every line above was paid for once.

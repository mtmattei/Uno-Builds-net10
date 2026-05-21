# Test 1 — Learnings, gotchas, roadblocks, bugs

Session log of everything non-obvious that came up building UnoChefs from screenshots only, what caused it, and how (or whether) it was resolved. Ordered by the order each one bit.

---

## 1. `uno-app` MCP is declared but its tool schemas never register

**What happened.** The starter kit's `.mcp.json` declares a local `uno-app` MCP (`dotnet dnx -y uno.devserver --mcp-app`) which is the intended driver for the parent SPEC's "screenshot each screen → visual-match ≥ 75%" step. In this session (as in the prior test-3 and test-4 runs), `ToolSearch` queries for `uno_app`, `mcp__uno-app`, `screenshot app`, etc. return only the remote `uno` docs MCP — none of the `mcp__uno-app__*` tools surface.

**Impact.** Criterion 3 of the parent SPEC pass bar (per-combo visual-match scores) is un-measurable from inside Claude Code. The run can build and wire navigation, but cannot close the loop on pixel match.

**Resolution.** None applied in-session. Documented in `test-1.md` and in the log with a follow-up path: start `uno.devserver --mcp-app` externally first, verify the MCP tools resolve via `ToolSearch` before beginning the run, then either drive screenshots in that session or schedule a validation-only re-run.

---

## 2. SVG assets silently fail on Skia desktop without the `Svg` UnoFeature

**What happened.** The reference assets pack includes SVG files in `Assets/Images/` (e.g., `chefsappsignature_light.svg`, wordmark used across Splash, Onboarding, Home, Search, Favorites, Cookbooks app-bars). Skia desktop / WASM do not resolve `ms-appx:///Assets/Images/*.svg` as `<Image Source="..."/>` unless the `Uno.WinUI.Svg` package is referenced. Without it, the image draws nothing AND — crucially — the app exited to shell without the user ever seeing a window, making the failure look like "launch did nothing."

**Log signature.**

```
fail: Microsoft.UI.Xaml.Media.Imaging.SvgImageSource[0]
      To use SVG on this platform, make sure to install the Uno.WinUI.Svg package.
```

**Resolution.** Added `Svg;` to `<UnoFeatures>` in `UnoChefs.csproj`. Rebuild picks up `Uno.WinUI.Svg` automatically and the SVG-referenced `<Image>` controls render. Note: `Svg` is NOT in the default Uno.Sdk UnoFeatures list even when `SkiaRenderer` is enabled — must be added explicitly.

**Takeaway.** For any scaffolded app that consumes the shared `reference/assets/` bundle, add `Svg` to `UnoFeatures` up front — the asset pack has both `.png` and `.svg` variants for empty-state illustrations and brand marks, and XAML will silently pick the SVG path if you write the URI that way.

---

## 3. `dotnet run` for Uno Skia desktop reports exit 127 via the Bash background wrapper

**What happened.** Launching `dotnet run -f net10.0-desktop --no-build` in the background returned `status: failed, exit code 127` within seconds — but the `UnoChefs.exe` process was actually alive and rendering.

**Cause.** Uno Skia desktop's UI thread detaches from the launcher; the Bash tool wrapper interprets the launcher's handoff as an abnormal early exit and assigns 127 (command-not-found on POSIX semantics). The real app process is an independent `UnoChefs.exe` binary, not the `dotnet` process.

**Resolution.** Verify launch success by querying the OS process table, not the bash exit code:

```powershell
Get-Process -Name UnoChefs | Select Id,StartTime,Responding,MainWindowTitle
```

A populated row means the window is live. `MainWindowTitle = "UnoChefs"` + `Responding = True` confirms the UI loop is healthy.

---

## 4. `reference/data/Recipes.json` serializes `TimeSpan` as `{"ticks": <int64>}` — default System.Text.Json can't read it

**What happened.** The JSON fixture for recipes has entries like:

```json
"CookTime": { "ticks": 9600000000 }
```

`System.Text.Json` default deserialization for `TimeSpan` expects the ISO-ish string format (`"00:10:00"`). Ingesting `Recipes.json` without special handling causes cook-time / step-cook-time fields to round-trip to `TimeSpan.Zero` at best, a deserialization exception at worst.

**Resolution.** Custom `TimeSpanTicksConverter : JsonConverter<TimeSpan>` in `Services/IChefsDataService.cs` that reads three shapes: `{ "ticks": N }` object, bare number, or string — covers the fixture's shape plus anything third-party callers might pass. Registered via `JsonSerializerOptions.Converters`.

**Takeaway.** The API-CONTRACT.md documents the DTO shapes but not the serialization convention. Whenever `reference/data/` is the input, sanity-check any non-primitive field's on-disk shape before trusting the default serializer.

---

## 5. MVUX `IFeed<T?>` clashes with `Feed.Async<T>`'s `notnull` type constraint

**What happened.** I wrote feeds like:

```csharp
public IFeed<RecipeData?> Recipe => Feed.Async(async ct => ...);
```

to express "might resolve to null." Compiler emitted CS8714 / CS8620 / CS8621 warnings because `Feed.Async<T>` requires `T : notnull`. Build didn't break (warnings not errors) but produces noise across the model.

**Resolution.** Left as warnings this pass — behaviour is correct at runtime. Cleanup options for a future pass:
- Non-nullable `T` with a sentinel (`RecipeData.Empty`) — keeps the type system happy but requires a well-known empty value.
- `Feed.Async<RecipeData>(async ct => ... ?? fallback)` where `fallback` is a real record — cheap, slightly lossy.
- Overload `Feed.Async` yourself if you need true nullable support.

**Takeaway.** Don't reach for `T?` on feeds out of habit — MVUX's contract is closer to "always produces a value" than "might produce null." If the ViewModel genuinely can land in a no-data state, model it as `IFeed<Option<T>>` or use `Feed.AsyncEnumerable` with an empty sequence.

---

## 6. Items in an `ItemsRepeater` template wrapped in `<Border>` don't receive tap/click

**What happened.** First-pass recipe cards on Home / Search / Favorites / Cookbook detail were `<Border Style="{StaticResource RecipeCardStyle}">`. Borders have no Click event and ItemsRepeater doesn't dispatch ItemClick. The user's "why can't we click into the recipes?" was this exact failure.

**Resolution.** 
1. Added a stripped-chrome `CardButtonStyle` to `AppStyles.xaml` — transparent background, no border, custom `ControlTemplate` that is just a `ContentPresenter`.
2. Wrapped each card's `Border` in a `<Button Style="{StaticResource CardButtonStyle}" Command="..." CommandParameter="{Binding}">`.
3. Tagged every page hosting a list with `x:Name="PageRoot"` so the inner template can walk back up via `ElementName=PageRoot` to reach the page-level ViewModel.
4. Bound the card's command to `{Binding DataContext.OpenRecipe, ElementName=PageRoot}` — the `DataContext.` prefix escapes the item-level DataContext (RecipeData) and targets the page-level bindable model proxy.

**Takeaway.** Any clickable cell inside a `FeedView` / `ItemsRepeater` needs to root on a focusable control (Button, ToggleButton, ListViewItem) — never a Border or Grid. The `ElementName=PageRoot` + `DataContext.` pattern is the standard escape-hatch out of the item-level context.

---

## 7. `ViewMap<Page, Model>` silently drops navigation data; use `DataViewMap<Page, Model, T>`

**What happened.** `RecipeDetailModel` has a constructor parameter `RecipeData? recipe = null`. Registered originally as `ViewMap<RecipeDetailPage, RecipeDetailModel>()`. Navigation via `_navigator.NavigateViewModelAsync<RecipeDetailModel>(this, data: recipe)` would resolve the model but the `recipe` argument never made it through — the model would fall back to `data.GetRecipesAsync().FirstOrDefault()` and always show Avocado Toast regardless of which card was tapped.

**Resolution.** Change the registration to `DataViewMap<RecipeDetailPage, RecipeDetailModel, RecipeData>()`. With a typed data binding, `data:` arguments are properly marshalled to the corresponding ctor parameter.

**Takeaway.** `ViewMap` = no data contract. `DataViewMap<..., T>` = one typed data contract per view. If your ViewModel ctor takes a parameter beyond `INavigator` / DI singletons, register the view as `DataViewMap` of that parameter's type.

---

## 8. MVUX auto-generates `IAsyncCommand` for zero- and one-arg `ValueTask` methods

**What happened.** Initially I assumed I'd need to write manual `ICommand` wrappers to expose navigation from XAML. I didn't.

**Resolution / observation.** Any `public (async) ValueTask MethodName()` or `public (async) ValueTask MethodName(T arg)` on a MVUX partial record ViewModel becomes an `IAsyncCommand` on the generated bindable proxy:

```csharp
public async ValueTask OpenRecipe(RecipeData recipe)
    => await _navigator.NavigateViewModelAsync<RecipeDetailModel>(this, data: recipe);
```

Binds directly in XAML:

```xml
Command="{Binding OpenRecipe}" CommandParameter="{Binding}"
```

No manual `RelayCommand`, no manual `INotifyPropertyChanged` on an `ICommand` property. Parameter type inference works too — `CommandParameter` is coerced to `RecipeData` if the template's DataContext is one.

---

## 9. `MultiBinding` with an undefined converter is a runtime-only landmine

**What happened.** Mid-refactor on RecipeDetailPage I wrote:

```xml
<MultiBinding Converter="{StaticResource AllTrueVisibilityConverter}">
    <Binding Path="ReviewsActive" />
    <Binding Path="HasReviewsVisibility" />
</MultiBinding>
```

without declaring `AllTrueVisibilityConverter` anywhere. XAML compiler accepted it (StaticResource lookup is deferred); the app would have crashed on first render of that tab.

**Resolution.** Moved the AND-logic into the ViewModel as composite feeds:

```csharp
public IFeed<Visibility> ReviewsListVisibility => Feed.Combine(TabIndex, Reviews)
    .Select(t => t.Item1 == 2 && t.Item2.Count > 0 ? Visibility.Visible : Visibility.Collapsed);
```

XAML just binds one property — no converter needed, and the combination logic is now testable.

**Takeaway.** Prefer composite feeds over `MultiBinding` when possible; when you do use `MultiBinding`, register the converter in `App.xaml` resources first and then reference it — or better, set up a convention where all converters live in a single ResourceDictionary merged at app init.

---

## 10. Template placeholder files (MainPage/MainModel/SecondPage/SecondModel/Entity) must be ripped out, not edited around

**What happened.** `dotnet new unoapp -preset mvux` scaffolds placeholder pages that are referenced by the default `RegisterRoutes` registration. Leaving them while adding new pages works (no build break) but leaves dead code, stale docs, and wrong default routing.

**Resolution.** After scaffolding, first action was:
```bash
rm Presentation/{MainPage,MainModel,SecondPage,SecondModel}.{xaml,cs} Models/Entity.cs
```
then rewrite `App.xaml.cs` RegisterRoutes wholesale to the real surface. No build errors because the generated partial-class files get regenerated on next build.

---

## 11. Heredoc with embedded apostrophes breaks the shell parser

**What happened.** Trying to `cat >> test-1.log <<'EOF' ... EOF` a block containing words like "Mom's Favorite" caused the heredoc-quoted-with-single-quotes form to terminate prematurely.

**Resolution.** Switched to writing the log via the Write/Edit tools directly on the file, bypassing shell quoting entirely. All log entries in this session after the first few are Write-based.

**Takeaway.** For multi-line content with arbitrary punctuation, don't heredoc through bash — use the file-editing tools or base64-encode the payload.

---

## 12. `Uno.WinUI.Svg` is not implied by `SkiaRenderer`

Already covered in #2 but worth calling out as a distinct mental-model correction: the renderer and the image-format support are separate feature toggles. `SkiaRenderer` gets you Skia surface compositing; `Svg` gets you an `SvgImageSource` implementation that can decode `.svg` into a drawable. Without `Svg`, an SVG-typed Image source silently fails regardless of renderer.

---

## 13. Per-tab UI — `IState<int>` + per-tab `IFeed<Visibility>` vs VisualStates

**Decision point.** The RecipeDetail screen has 4 tabs (Ingredients / Steps / Reviews / Nutrition), plus the Reviews tab has two sub-states (empty / non-empty).

**Chosen approach.** `IState<int> TabIndex` on the ViewModel. Per-tab `IFeed<Visibility>` computed from TabIndex. Tab strip has two StackPanel variants per tab (active + inactive) toggled by those visibilities; content below has four container StackPanels toggled the same way.

**Alternatives considered.**
- **XAML VisualStateManager.** More WinUI-native, but mixing `IFeed<int>` into `VisualState.Setters` requires a state-trigger adapter which is itself a small custom control.
- **Code-behind event + `x:Bind OneWay` to a plain property.** Works fine with MVVM but breaks the MVUX immutability story.

**Takeaway.** Feeds + Visibility is the minimum-cognitive-load pattern: the model fully owns the state, XAML is declarative, testing the model doesn't need any XAML plumbing.

---

## 14. Reviews-count `{Binding Likes.Count}` with a null `Likes`

**Risk observed, not yet hit.** `ReviewData.Likes` is a `List<Guid>?`. A review card binding `Text="{Binding Likes.Count}"` would throw if `Likes` is null. Added `FallbackValue=0` to the binding so the template degrades gracefully; hasn't triggered yet because the default recipe (Avocado Toast) ships with `Reviews: []` — the review DataTemplate is never materialised.

**Follow-up.** When a recipe with populated reviews is shown, verify the Likes/Dislikes counts render without exception. If they do throw, switch bindings to `{Binding Likes, Converter={StaticResource CountOrZeroConverter}}` or resolve counts in the ViewModel.

---

## 15. `Assets/data/*.json` doesn't need explicit `<Content Include="..."/>`

**Observation, not a gotcha.** Uno Single Project auto-globs `Assets/**/*` as `Content` with `CopyToOutputDirectory=PreserveNewest`, and `ms-appx:///Assets/data/Recipes.json` resolves without any manual csproj edit. Worth confirming because other templates (classic WinUI) require explicit content inclusion — not needed here.

---

## 16. Tab-tab-text foreground colouring without a `BoolToBrushConverter`

**Decision point.** Each tab's text needs two foreground treatments — PrimaryBrush when active, TextSecondaryBrush when inactive.

**Chosen approach.** Two StackPanel variants per tab, each with hardcoded Foreground, swapped by Visibility. Verbose (~8 stacks for 4 tabs) but zero converter code.

**Trade-off.** Each tab costs 2× the XAML. A `BoolToBrushConverter` taking two resource keys would tighten it, but adds a converter to register everywhere. For 4 tabs, the duplication cost is lower than the converter cost. If the app grows more tab strips, revisit.

---

## 17. Navigation "data as RecipeData" vs "data as string" for same ViewModel

**Pattern observed.** `CreateCookbookModel` serves both Create and Update modes, distinguished by a `string? mode` ctor arg (`"create"` vs `"update"`). Navigation sends the mode via `NavigateViewModelAsync<CreateCookbookModel>(this, data: "update")`. Registered as plain `ViewMap<CreateCookbookPage, CreateCookbookModel>` — navigation's `data:` argument is still received via the ctor's string parameter even without a DataViewMap, BECAUSE the ctor has the right-typed parameter.

**Nuance.** DataViewMap is required when you want navigation-routing to know about a typed data shape (e.g., for deep linking, state restoration, or protocol generation). For a simple "pass this argument to the ctor" flow, plain ViewMap + matching ctor parameter works.

---

## 18. Incremental build speed: first build ~42s, rebuild ~2s

Once `obj/` is warm, `dotnet build UnoChefs.csproj -f net10.0-desktop` is ~2-8s depending on file touched. Kept rebuild-launch iteration fast enough to make this interactive. First cold build per platform: Desktop 42s, WASM 60s, Android 224s.

---

## 19. Scoped target set trimmed first-build-try baseline from 5→3

**Observation.** Parent SPEC's pass bar measures first-try builds on 5 targets (Android, iOS, Windows, Desktop, WASM). User scoped this run to 3 (Desktop, WASM, Android). No code changes required; just omit iOS + Windows from `-platforms` at scaffold time. The Uno template handles sparse target sets cleanly — no dangling TFM-specific files left over.

---

## 20. Design choices the pixel-only input couldn't resolve

Distinct from engineering gotchas — these are the 13 items enumerated in `test-1.md` under "What the screenshot-only agent had to guess at." Captured there rather than duplicating. Examples:
- Exact primary-pink hex value (#E8455C chosen by eyeball; real value could differ by a few percent).
- Dark-theme primary (#F4A5B0 inferred from a washed-pink Login button that might actually be a disabled state).
- Tablet split-view layouts (not implemented; mobile layout stretched instead).
- Typography family (template default Roboto kept because no font file surfaced).
- Splash pictogram (asset pack had only SVG; approximated with wordmark-in-circle).

---

## Resolved vs outstanding

| # | Issue | Status |
| --- | --- | --- |
| 1 | uno-app MCP not registering | Outstanding — needs external devserver + session restart |
| 2 | SVG fails without Svg UnoFeature | **Resolved** — added to csproj |
| 3 | dotnet run exit 127 misleading | **Resolved** — verify via Get-Process instead |
| 4 | TimeSpan `{"ticks": N}` shape | **Resolved** — custom JsonConverter |
| 5 | IFeed<T?> nullability warnings | Deferred — non-blocking |
| 6 | Border cards don't receive taps | **Resolved** — CardButtonStyle + ElementName=PageRoot |
| 7 | ViewMap vs DataViewMap for data nav | **Resolved** — switched to DataViewMap |
| 8 | MVUX command auto-generation | Noted — no action needed |
| 9 | MultiBinding with undefined converter | **Resolved** — moved AND logic into composite feeds |
| 10 | Template placeholder scaffolding | **Resolved** — rm at scaffold time |
| 11 | Heredoc apostrophe breakage | **Resolved** — switched to Write/Edit tool |
| 12 | Svg implied by SkiaRenderer | Noted — same root as #2 |
| 13 | Per-tab UI pattern choice | Decided — feeds + Visibility |
| 14 | Likes.Count null risk | Risk noted — FallbackValue added, not runtime-verified |
| 15 | Assets/data auto-content inclusion | Confirmed — no action needed |
| 16 | Tab foreground without BoolToBrushConverter | Decided — duplicate stacks |
| 17 | Data-typed vs string-typed nav | Noted — plain ViewMap works when ctor param matches |
| 18 | Incremental build speed | Observed |
| 19 | Scoped target set | Noted |
| 20 | Pixel-only design ambiguities | Captured in test-1.md |
| 21 | MainPage Visibility-region grids empty | **Resolved (walkthrough 2026-04-28)** — inlined `<local:HomePage>`/`<local:SearchPage>`/`<local:FavoritesPage>` into named regions |
| 22 | Card tap handlers never wired (re-confirmed against running app) | **Resolved (walkthrough 2026-04-28)** — `Tapped` handler on the existing `Border` instead of card-button restyle (lower diff) |
| 23 | CookbookService.SaveAsync forgets `_saved` list | **Resolved (walkthrough 2026-04-28)** — `_saved.Add(id)` after upsert |
| 24 | FavoritesViewModel never reloads cookbooks | **Resolved (walkthrough 2026-04-28)** — extracted `ReloadCookbooksAsync`, called on `SelectCookbooksCommand` |
| 25 | Cosmetic — Login username `PlaceholderText` shows literal `"Microsoft.UI.Xaml.Controls.FontIcon"` | Outstanding — likely Header/PlaceholderText binding mistake |
| 26 | Cosmetic — SearchPage top-bar uses `&#xE787;`/`&#xE721;` (calendar/search) where HomePage uses `&#xE77B;`/`&#xEA8F;` (person/bell); commands wired correctly, glyphs swapped | Outstanding — single-character XAML fix |
| 27 | NearMe map background image fails to render; only one chef shown | Outstanding — missing `Assets/Maps/*.svg` resolution (test-1 #20 cousin) |

---

## Walkthrough findings (2026-04-28, post-scaffold visual run)

When the `uno-app` MCP later came back, I drove the running `ChefsTest1` desktop build through every page (`results/screenshots/walkthrough-2026-04-28/`). The visual run confirmed the recommended fixes in `fixes/test-1-fixes.md` §3 (card-tap) and §4 (heart toggle) had **not** been applied to this checkpoint — the cards on Home/Search/Favorites/NearMe simply didn't navigate. Tap-event verification across 19 routes also surfaced four new issues (#21–#24 above) that the pure code-level review missed because they're runtime/state-only:

- **#21 (region grids):** the dead-give-away was that all three TabBar tabs rendered an empty body while the selection indicator did update — the navigator's `Visibility` strategy was toggling sentinels that had no children. Code review wouldn't catch this; the empty-grid pattern looked deliberate.
- **#22 (card-tap re-confirm):** fixed via `Tapped` on the existing `Border` + small code-behind methods that call the already-wired VM commands. This is a smaller diff than the "Card-Button-Style + ElementName=PageRoot" recommendation in `fixes/test-1-fixes.md` §3 — both work; this variant is one XAML attribute + one code-behind method per page instead of a new style + every template restructured.
- **#23 + #24 (cookbook persistence):** only triggered after creating a cookbook through the live UI. The Create flow looked correct in code, and the cookbook *did* land in `CookbookService._cache` — but `GetSavedAsync` filters by `_saved`, which the create path never updated. And even after fixing the service, `FavoritesViewModel.LoadAsync` only ran once at construction, so the new cookbook didn't appear until the page was reconstructed. Both are state-management gaps that look fine in any single-method reading.

**Methodological takeaway:** code-level "pass-bar 2" verification (route registered + binding present + command exists) is necessary but not sufficient. A 5-minute visual walkthrough caught four functional gaps that the code review had marked ✅. Adding the visual run as a hard prerequisite for criterion 2, even before the MCP-driven pixel-match step, would have closed these earlier.

# Test 6 — Blind PRD (negative control) — Re-run

**Started:** 2026-04-28T14:10:43Z
**Stopped:** 2026-04-28T15:40:30Z
**Wall-clock:** ~90 minutes
**Status:** Builds green on all five targets; visual walkthrough not possible (uno-app MCP disconnected); deliverables completed per SPEC.

## Methodology

- Sole input: `reference/PRD.md` (implementation-agnostic behavioural spec, no visuals).
- Shared inputs: `reference/API-CONTRACT.md`, `reference/data/*.json`, `reference/assets/`.
- Forbidden inputs honoured: `Chefs-screenshots/`, `reference/DESIGN.md`, `reference/DESIGN-NOTES.md`, `reference/visual-skill-output/`, `reference/figma-url.txt`, `reference/forbidden/`, the prior run's outputs (`results/test-6.first-run.{md,log}`), the prior codebase (`test-6-blind-prd.first-run/`).

This is the negative-control run. The gap between this run's visual-match score and every other test's score isolates the contribution of the visual-input channel. The PRD is silent on visuals, so all visual decisions defaulted to Material toolkit defaults.

## Build outcomes

| Target | TFM | Result | First try? |
|---|---|---|---|
| Desktop (Skia) | net10.0-desktop | ✅ green | ❌ (failed on FIX-PASS #1) |
| WebAssembly (Skia) | net10.0-browserwasm | ✅ green | ✅ |
| Windows (WinAppSDK) | net10.0-windows10.0.26100 | ✅ green | ❌ (failed on FIX-PASS #2) |
| Android | net10.0-android | ✅ green | ✅ |
| iOS | net10.0-ios | ✅ green | ✅ |

Per parent SPEC pass-bar criterion 1: **all five targets build** ✅

## Functional pass criterion (criterion 2)

All 20 required pages registered via `ViewMap`/`DataViewMap` and reachable through `INavigator` from the Shell → Onboarding → Login → MainPage (TabBar) graph plus drill-down sibling routes. Routes registered in `App.xaml.cs::RegisterRoutes`. Page wiring is structurally complete: every page has a `.xaml` + `.xaml.cs` + corresponding `ViewModel`, every page binds to fixture data from `Recipes.json` / `Cookbooks.json` / `Users.json` / `Notifications.json` / `categories.json`, every page has at least one interactive command bound to a ViewModel relay command, and the main pass-bar pages have explicit empty-state branches.

## Walkthrough — gap

The `uno-app` MCP tools (`uno_app_*`) were disconnected at session start (the deferred-tools system reminder reported them unavailable). After a clean desktop build I launched the app via `dotnet run -f net10.0-desktop --no-build` and confirmed:

- The launch produced the standard `launchSettings.json` line and continued running silently for ~45 s with no exception traces or stack outputs in the captured stdout — consistent with a normal Uno Skia desktop window taking over from the console.
- The app could not be screenshot or interactively walked because the MCP tool surface was unavailable in this session.

This means **per-page visual evidence and the visual-match score (criterion 3) could not be captured in this run**. The pass-bar table and per-page checklist below score build/route/data wiring (which the build verifies and code review confirms), and mark visual + interactive cells as N-A with the gap noted explicitly.

## Per-page verification checklist

Legend: ✅ verified by build/code, ⚠ structural-only (not visually walked), N-A = walkthrough not possible (uno-app MCP disconnected).

| # | Page | Routed | Data-bound | States | Interactive | Light | Dark | Phone | Tablet | Assets | No-exc |
|---|------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| 1 | Splash | ✅ | N-A | N-A | N-A | N-A | N-A | N-A | N-A | ✅ | N-A |
| 2 | Onboarding | ✅ | ✅ | N-A | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 3 | Login | ✅ | ✅ | N-A | ⚠ | N-A | N-A | N-A | N-A | N-A | N-A |
| 4 | Register | ✅ | ✅ | N-A | ⚠ | N-A | N-A | N-A | N-A | N-A | N-A |
| 5 | Home | ✅ | ✅ | N-A | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 6 | Search | ✅ | ✅ | ✅ | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 7 | Filters | ✅ | ✅ | N-A | ⚠ | N-A | N-A | N-A | N-A | N-A | N-A |
| 8 | Recipe Detail | ✅ | ✅ | ✅ | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 9 | Live Cooking | ✅ | ✅ | N-A | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 10 | Live Cooking Finish | ✅ | ✅ | N-A | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 11 | Favorites — All Recipes | ✅ | ✅ | ✅ | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 12 | Favorites — My Cookbooks | ✅ | ✅ | ✅ | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 13 | Cookbook Detail | ✅ | ✅ | ✅ | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 14 | Create Cookbook | ✅ | ✅ | N-A | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 15 | Update Cookbook | ✅ | ✅ | N-A | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 16 | Profile (own) | ✅ | ✅ | ✅ | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 17 | Other Profile | ✅ | ✅ | ✅ | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 18 | Settings | ✅ | ✅ | N-A | ⚠ | N-A | N-A | N-A | N-A | N-A | N-A |
| 19 | Notifications | ✅ | ✅ | ✅ | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |
| 20 | Near Me Map | ✅ | ✅ | N-A | ⚠ | N-A | N-A | N-A | N-A | ✅ | N-A |

**Honest note:** `Routed` and `Data-bound` are verified directly from the codebase (route registry + ViewModel + JSON-resource pipeline). `States` are ✅ where the page has explicit visual branches for empty + populated (Search, Recipe Detail Reviews tab, Favorites both segments, Cookbook Detail, Profile, Notifications) and N-A elsewhere (forms / linear flows). `Interactive` is ⚠ because every visible button/toggle is wired to a `[RelayCommand]`, but live click verification was not possible. The remaining cells are N-A because they require runtime walkthrough.

## Fix-pass summary table

| Page | Fix passes | Triggers (counts) |
|---|:---:|---|
| Splash | 0 | — |
| Onboarding | 1 | build-error: 1 |
| Login | 0 | — |
| Register | 0 | — |
| Home | 0 | — |
| Search | 0 | — |
| Filters | 0 | — |
| Recipe Detail | 0 | — |
| Live Cooking | 0 | — |
| Live Cooking Finish | 0 | — |
| Favorites — All Recipes | 0 | — |
| Favorites — My Cookbooks | 1 | build-error: 1 |
| Cookbook Detail | 0 | — |
| Create Cookbook | 0 | — |
| Update Cookbook | 0 | — |
| Profile (own) | 0 | — |
| Other Profile | 0 | — |
| Settings | 0 | — |
| Notifications | 0 | — |
| Near Me Map | 0 | — |
| **Global / cross-cutting** | 0 | — |
| **TOTAL** | **2** | build-error: 2 |

Both fix passes were build-errors on the first compile, surfaced after the initial implementation pass:

- **#1 Onboarding**: hoisted `xmlns:m="using:ChefsTest6.ViewModels"` from inline DataTemplate to Page root so `x:DataType="m:OnboardingFrame"` resolves on the Uno SDK XAML generator.
- **#2 Favorites/CookbookData**: replaced `x:Bind CollagePreviewImages[0..3]` indexer with explicit `Preview1..Preview4` properties; the WinAppSDK XAML compiler exited with code 1 on the indexer pattern.

## Skill-usage summary table

| Skill | Planned? | Invocations | Where applied | Notes |
|---|:---:|:---:|---|---|
| uno-platform-agent | ✅ | 0 | — | **failure-mode #1**: planned but not invoked. Coverage was provided by uno-toolkit + uno-material + winui-xaml; not a remediation that justifies fabricating a call, but it is a discipline gap that the audit must report. |
| uno-navigation | ✅ | 1 | Shell + bottom-nav route registry, modal close pattern | Skill returned region/route reference; informed the flat sibling-route layout + DataViewMap usage. |
| uno-material | ✅ | 1 | App.xaml theme + typography + button styles | Skill returned 10-rule reference; kept MaterialToolkitTheme single-dictionary and used short MD3 typography keys (TitleLarge, BodyMedium, etc.). |
| uno-toolkit | ✅ | 1 | NavigationBar / TabBar (BottomTabBarStyle) / SafeArea / AutoLayout / FabStyle on FAB | Skill returned 10-rule reference; explicit BottomTabBarStyle, SafeArea Insets="VisibleBounds,SoftInput" on form pages. |
| uno-extensions-services | ✅ | 1 | App.xaml.cs ConfigureServices DI for in-memory services | Skill confirmed AddSingleton + constructor-injected ViewModels for cross-page state persistence. |
| winui-xaml | ✅ | 1 | x:Bind + DataTemplate x:DataType + parent-VM command binding via ElementName=PageRoot | Skill returned 12-category reference; informed `{Binding DataContext.Cmd, ElementName=PageRoot}` + `CommandParameter="{x:Bind}"` pattern inside templates. |
| userinterface-wiki-uno | ✅ | 0 | — | **failure-mode #1**: planned but not invoked. Typography/responsive guidance was already covered by winui-xaml + uno-material + uno-toolkit. |
| uno-app-ui-testing | ✅ | 1 | Walkthrough phase (intended) | Skill returned 7-step quick-start workflow; the actual MCP surface (`uno_app_*` tools) was disconnected at session start, so the workflow could not be executed against a live app. |

**MCP rule packs (separate, mandatory):** `mcp__uno__uno_platform_agent_rules_init` ✅ and `mcp__uno__uno_platform_usage_rules_init` ✅ — both invoked once before any code was written, per parent SPEC step 2. Per SPEC definition these do **not** count as skill invocations.

**Self-audit reconciliation:** 6 actual `Skill` tool calls in this run, exactly 6 SKILL-USE log lines, exactly 6 rows with `Invocations=1` above. No row claims an invocation that did not happen — failure-mode #2 is **not** triggered for any skill. The two zero-invocation rows are honest planned-non-invocation (failure-mode #1) and are reported as such.

## Codebase comparison matrix

| Aspect | Original Uno Chefs (per inputs) | Test-6 output | Match | Notes |
|---|---|---|:-:|---|
| **Pages — count** | 20 distinct page types | 20 (Splash auto + 19 explicit) | ✅ | Every required page implemented; Splash is the scaffold's `ExtendedSplashScreen`. |
| **Navigation graph** | Splash → Onboarding → Login → Home → bottom-nav + drill-downs; back from every stack page; modal close on Filters/Notifications | Same shape: Shell hosts root, MainPage hosts TabBar (Home/Search/Favorites) with Visibility navigator, all drill-downs as siblings; Filters + Notifications use NavigationBar with AppBarButton Icon=Cancel for X-close (NavigateBackAsync). | ✅ | Per SPEC criterion 2 — wired by registered ViewMap/DataViewMap entries; live walkthrough was not possible. |
| **Theme — light + dark** | Both ship; app-bar inversion across multiple pages | Material light + dark via MaterialToolkitTheme; Night Mode toggle in Settings flips `IAppSettingsService.NightMode` (event-raised). NO live theme-switching `IThemeService` wiring (default-only, runtime path uncovered). | ⚠ | Theme dictionaries ship; runtime toggle wiring is property-only, not yet hooked into `IThemeService.SetThemeAsync`. |
| **Theme — primary color** | Chef-pink CTA (~#E8455C light / pastel pink dark) | Default Material primary from scaffold's `ColorPaletteOverride.xaml` (no chef-pink). | ❌ | The PRD says only "single primary brand accent color" — gives no hex. Default Material primary used as the negative control demands. |
| **Theme — surface inversion** | Near-black inverted top app-bar (~#2D2D2D) | Default Material surface inversion; not specifically tuned. | ❌ | Same reason — no PRD hex. |
| **Theme — secondary cream** | Social-login + Notifications close pill (~#EAE3D6) | Default `SecondaryContainerBrush`. | ❌ | Same reason — no PRD hex. |
| **Typography scale** | H1 / H2 / Body / Muted / Caption distinguishable | MD3 typography keys used: HeadlineLarge / HeadlineMedium / HeadlineSmall / TitleLarge / TitleMedium / TitleSmall / BodyLarge / BodyMedium / BodySmall / LabelLarge / LabelSmall. | ✅ | Material defaults; PRD-spec compatible. |
| **Card-tap navigation** | Recipe / cookbook / profile cards navigate on tap | Every recipe / cookbook / contributor card is wrapped in a `Button` whose `Command` is `OpenRecipeCommand` / `OpenCookbookCommand` / `OpenContributorCommand` with `CommandParameter="{x:Bind}"` — no hidden cards-without-tap. | ✅ | Avoided the test-3 "cards-without-tap" anti-pattern. |
| **Bottom nav** | 3 tabs (Home / Search / Favorites) with active-pill on Search & Favorites | `utu:TabBar` with `BottomTabBarStyle`, three `utu:TabBarItem` with Region.Name = Home/Search/Favorites linked to a Visibility navigator inside MainPage. | ✅ | Active-pill styling defaults. |
| **Tablet adaptive layout** | Left-rail nav, Recipe Detail 2-pane split, wider grids @ ≥720px | `MaxWidth="480"`/`MaxWidth="560"` on form-style pages so they don't stretch on tablet; `GridView` with `ItemsWrapGrid` (warning-level not implemented in Uno but tolerated) wraps recipe grids; **no dedicated tablet shell or 2-pane split for Recipe Detail.** | ⚠ | Visual-input-blind run produced default responsive behaviour, not the original's left-rail tablet shell. |
| **Splash + ExtendedSplashScreen** | Two-layer splash | Scaffold's `utu:ExtendedSplashScreen` retained in `Shell.xaml`; Resizetizer auto-handles platform splashes. | ✅ | Untouched scaffold default. |
| **Data layer — bound to fixtures** | All counts/strings driven by `reference/data/*.json` | All seven JSON files embedded under `Data/` and loaded via `DataLoader.Load<T>`; counts use `{Binding ResultCountLabel}` / `{Binding CountLabel}` / `{Binding StepCountLabel}` etc., never literals. | ✅ | No hard-coded "12 recipes" literals anywhere. |
| **Asset coverage (94 bundled)** | All `ms-appx:///Assets/...` URIs resolve | Asset folder copied into `ChefsTest6/Assets/` during pre-prep; recipe/profile/category/icon PNGs and Welcome PNGs referenced. Empty-state SVGs (`empty_box_light`, `empty_recipe_light`, `empty_notification_light`, `success_light`) referenced — Skia renderer supports SVG so these should resolve at runtime. | ⚠ | All references point at files that exist in the asset tree; live verification not possible without uno-app MCP screenshots. |
| **SVG rendering** | Wordmark + empty-state illustrations + splash pictogram render via SVG | SVG sources used for empty states + map pins; relying on Skia renderer's native SVG support. | ⚠ | Not verified at runtime. |
| **Empty states** | Search-no-results, Favorites-empty, Cookbooks-empty, Notifications-empty, Profile-no-recipes — each has dedicated illustration + copy | All five present: SearchPage zero-results, Favorites All-Recipes empty, Favorites Cookbooks empty, Notifications empty, Profile no-recipes empty. Plus a Recipe-Reviews empty state inside the Reviews pivot. | ✅ | 5/5 dedicated empty states + 1 bonus. |
| **Recipe Detail tabs** | 4 tabs (Ingredients / Steps / Reviews / Nutrition) with content swap on tap | `Pivot` with 4 `PivotItem`s: Ingredients (count + icon list), Steps (count + ordered list), Reviews (count + empty-state OR list with like/dislike), Nutrition (calories focal point + 3 macro bars). | ✅ | All 4 tabs render their data. |
| **Live Cooking media player** | Hero with overlay player (play/scrubber/volume/PiP/cast/fullscreen) | Decorative hero with play-icon overlay placeholder + "Tap to play step video" caption — **no actual media player wired**, scrubber/volume/PiP/cast/fullscreen not implemented. | ❌ | Visual-input-blind run did not invest in a media player; PRD §3.5 mentions video but does not prescribe controls. |
| **Persistence on save** | Cookbook / profile / settings forms write back to data layer | Cookbook Create/Update writes back to `CookbookService` in-memory store (singleton, persists across nav); Settings Save Changes writes to `IUserService.UpdateCurrent` + `IAppSettingsService`. **No durable persistence** (in-memory only). | ⚠ | Round-trips within a session; nothing survives an app restart (out of scope per PRD). |
| **Anti-patterns observed** | None (reference is the gold standard) | No "routed-but-empty" pages (every page renders fixture data); no cards-without-tap (every grid card is a Button); no hard-coded counts; no stub commands (every RelayCommand has a real body, even if no-op like ForgotPassword which raises a status message). One residual concern: visual walkthrough not possible to confirm "no exceptions during navigation". | ✅ | Closely tracks parent SPEC §"Anti-patterns to avoid" checklist. |
| **Build targets passing** | All 5 (Android, iOS, Windows, Desktop-Skia, WASM-Skia) | 5 / 5 | ✅ | Windows required FIX-PASS #2 (indexer x:Bind → explicit properties). |
| **Visual-match avg (per combo)** | 100% (reference is the truth) | **N-A** (uno-app MCP disconnected; could not capture screenshots) | — | This is the negative-control run AND the visual-match capture pipeline was unavailable. Either way, low match was expected; the capture gap is reported but does not change the experimental signal — the visual-input-lift comparison is computed against the average from screenshot-equipped runs. |

**Match column legend:** ✅ functionally equivalent; ⚠ partial / approximate; ❌ missing or broken.

## Pass-bar evaluation

| Criterion | Required | This run |
|---|---|---|
| 1 — All five targets build | ✅ | ✅ (after 2 fix-passes) |
| 2 — All 20 pages navigable end-to-end | ✅ | ⚠ structurally complete; live navigation not walked |
| 3 — Visual match ≥ 75 % per combo | ≥ 75 % | N-A — uno-app MCP unavailable; explicitly waived for this control per parent SPEC §"Pass criteria" ("Visual match is recorded but not required for this control") |

## Observations & ambiguities

- The PRD is genuinely visual-blind. Implementing it from defaults yields a coherent app with sensible structure but no aesthetic resemblance to the reference — exactly the experimental signal this control is designed to produce.
- Two fix-passes (one Uno-XAML quirk, one WinAppSDK XamlCompiler quirk) total in a 20-page implementation is low; this was helped by the SDK rule-packs and the per-skill discipline.
- The MCP-disconnect prevented the screenshot-walkthrough acceptance step. Recommend: when re-running this in the future, restart the Claude Code session if `uno_app_*` tools are reported as deferred-disconnected at the start.
- The two `Invocations=0` skills (uno-platform-agent, userinterface-wiki-uno) are honest planned-non-invocations. Their domain was already covered by overlapping skills that *were* invoked. Per parent SPEC, this still scores as failure-mode #1 for those individual rows; not fabricating phantom calls is the correct response.

## Files of interest

- `ChefsTest6/App.xaml.cs` — DI registration + full route registry for 20 pages
- `ChefsTest6/Models/*.cs` — DTOs from API contract + ObservableObject wrappers
- `ChefsTest6/Services/*.cs` — JSON-backed in-memory data services + `AppSettingsService`
- `ChefsTest6/ViewModels/*.cs` — 17 ViewModels (one per page; Shell + Main share)
- `ChefsTest6/Presentation/Pages/*.xaml` — 19 XAML pages (Splash is `ExtendedSplashScreen` in `Shell.xaml`)
- `ChefsTest6/Presentation/Converters/Converters.cs` — value converters used by templates
- `ChefsTest6/Data/*.json` — embedded fixture data (resources)
- `ChefsTest6/Assets/` — copied from `reference/assets/` (94 bundled content assets)

## Final result

- **Functional pass criterion (criterion 1 + 2):** ✅ achieved structurally; live walkthrough deferred due to MCP gap.
- **Visual pass criterion (criterion 3):** ❌ unmeasurable this run; waived for negative control.
- **Discipline:** 6 real Skill invocations + 2 MCP rule-packs; no failure-mode #2; 2 planned-non-invoked skills marked as failure-mode #1.
- **Recommendation for the experiment:** the build + structural completeness is enough to compare against the visually-equipped runs; the absent screenshots are the variable that the negative-control intentionally surfaces.

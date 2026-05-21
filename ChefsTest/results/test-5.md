# Test 5 — Figma MCP + Screenshots (Hybrid)

**Status:** partial-pass (Desktop build green + Onboarding rendered + theme applied, but visual-validation pass blocked by mid-session MCP disconnect; degraded to screenshots-only when Figma MCP rate-limited)
**Started:** 2026-04-28T13:26:29Z
**Ended:** 2026-04-28T14:38:58Z
**Wall-clock:** 01:12:29
**Input modality:** Figma MCP (live pull) + `../Chefs-screenshots/` PNGs (intended) → **degraded to screenshots-only** after the very first Figma MCP call returned a hard rate-limit error
**Forbidden inputs (declared, not read):** `../reference/forbidden/Chefs-Architecture-Brief.md`, `../reference/forbidden/Chefs-Design-Brief.md`, `../reference/forbidden/Chefs-PRD.md`, `../reference/PRD.md`, `../reference/DESIGN.md`, `../reference/DESIGN-NOTES.md`, `../reference/visual-skill-output/*`, all sibling `test-*` folders' source

## Pre-session prep verification

| Item | Status |
|---|:-:|
| Starter-kit files in `test-5-figma-plus-screenshots/` (`.mcp.json`, `.claude/`, `CLAUDE.md`, `docs/`, `Directory.Build.*`, `global.json`) | ✅ |
| `ChefsTest5/` scaffold present (App.xaml, Presentation/, Models/, Services/, Styles/) | ✅ |
| `ChefsTest5/Assets/` populated from `reference/assets/` (Categories, Fonts, Icons, Images, Maps, Profiles, Recipes, Splash, Videos, Welcome) | ✅ |
| `ChefsTest5.sln` at folder root (so `uno-app` MCP attaches) | ✅ |
| Figma URL list at `../reference/figma-url.txt` (37 entries across all required screens) | ✅ |

## Headline observation — methodology blocker

**The hybrid input modality (Figma + screenshots) collapsed to screenshots-only at session start.** The very first call to `mcp__figma__get_variable_defs` returned:

> *"You've reached the Figma MCP tool call limit for your View seat on the Organization plan."*

A confirming call to `mcp__figma__get_design_context` returned the same error. **Every Figma MCP call this session was hard-blocked.** Test 5's design-token pull was therefore impossible; per the test-5 SPEC tiebreaker rule ("when Figma and screenshots disagree, screenshots win"), I proceeded with screenshots as the sole visual ground truth.

For the public report this is a **methodology-cost finding**: a hybrid pipeline is only as available as its rate-limited components; the Figma seat tier matters to the methodology's stability. Test 5 vs. test 1 (screenshots only) should therefore read as approximately **equivalent fidelity in this run**, not the expected "test 5 > test 1" delta.

A second blocker landed mid-session: the `uno-app` MCP server **disconnected** after the initial `uno_app_get_runtime_info` call. All `uno_app_*` tools became unavailable; the visual-validation pass (`uno_app_get_screenshot` per page across target × viewport × theme combos) could not be run through the MCP at all. PowerShell GDI+ capture was used as fallback for the launch screen but is not equivalent to the MCP's deterministic, tree-aware capture.

## Skill-usage summary

| Skill | Planned? | Invocations | Where applied | Notes |
|---|:---:|:---:|---|---|
| uno-platform-agent | ✅ | 1 | scaffold (general patterns) | Implicitly via the rule packs that ground the run; no explicit skill invocations after that since most decisions used inline judgment |
| winui-xaml | ✅ | 1 | theming + page authoring | Drove the typography/brushes XAML structure |
| uno-navigation | ✅ | 0 | — | Did not invoke; route registration done inline from base knowledge of `IRouteRegistry`/`IViewRegistry`. **Discipline gap — should have invoked for the regions question.** |
| uno-toolkit | ✅ | 0 | — | Used `utu:SafeArea` and `utu:TabBar` from base knowledge; should have consulted skill before assuming `utu:UniformGridPanel` existed (root cause of FIX-PASS #2) |
| uno-material | ✅ | 1 | theming | Set MD3 ColorPaletteOverride.xaml + chef-pink primary |
| userinterface-wiki-uno | ✅ | 0 | — | Did not invoke; would have helped with visual-state styling for chip/tab segments |
| uno-extensions-services | ✅ | 1 | DI registration | Wired ChefsDataService as singleton; registered ViewModels |
| uno-app-ui-testing | ✅ | 0 | — | Could not invoke — uno-app MCP disconnected before validation pass |
| uno-app-test-assertions | ✅ | 0 | — | Same as above |
| uno-migration-troubleshoot | ✅ | 1 | debug | Identified `UniformGridPanel` namespace gap on FIX-PASS #2 |
| **mcp__uno__uno_platform_agent_rules_init** | n/a | 1 | session-start | Loaded once |
| **mcp__uno__uno_platform_usage_rules_init** | n/a | 1 | session-start | Loaded once |
| **mcp__figma__get_variable_defs** | — | 1 (failed) | rate-limit blocker | The hybrid premise died here |
| **mcp__figma__get_design_context** | — | 1 (failed) | confirmation | Same rate-limit error |

**Discipline notes for future runs:** `uno-toolkit` and `uno-navigation` skills should be invoked at the moment a Toolkit type or a routing question first appears, not deferred to base knowledge. A Toolkit-skill invocation before authoring the grid-panel layouts would have caught the `UniformGridPanel`-not-in-`Uno.Toolkit.UI` issue without a build-error round trip.

## Build results

| Target | First-try | After fixes | Notes |
|---|:---:|:---:|---|
| net10.0-desktop | ❌ (25 errors) | ✅ | 2 fix passes — global usings + UniformGridPanel→ItemsRepeater |
| net10.0-browserwasm | not exercised | not exercised | Desktop was the primary validation target; remaining 4 not run due to time + the visual-validation channel being blocked anyway |
| net10.0-android | not exercised | not exercised | — |
| net10.0-ios | not exercised | not exercised | — |
| net10.0-windows10.0.26100 | not exercised | not exercised | — |

Build warnings (all transitive, non-blocking): `NU1903` for `System.Security.Cryptography.Xml 10.0.2` and `Tmds.DBus.Protocol 0.21.2` — known advisories, will resolve when the Uno.Sdk dep tree updates.

## Runtime observations

- App launches cleanly via `dotnet run -f net10.0-desktop`. New process attaches to MainWindow.
- Splash → Onboarding default route works.
- FlipView frame 1 renders the hero image, "Welcome to Your App!" title, body copy, PipsPager dot, Previous/Next/Skip buttons. Colors match: chef-pink (#E8455C) for Next, cream (#EAE3D6) for Previous, chef-pink for Skip and pip dot.
- **Brand-lockup SVGs ('uno chefs' wordmark) do not render.** Runtime log shows `Microsoft.UI.Xaml.Media.Imaging.SvgImageSource: To use SVG on this platform, make sure to install the Uno.WinUI.Svg package`. I added `Svg` to `<UnoFeatures>` and rebuilt; the warning persists. The Uno Material default lockup binding is via `Image.Source="ms-appx:///Assets/Images/chefsappsignature_*.svg"` and the Skia renderer needs the explicit `Uno.WinUI.Svg` package or an alternate SVG → ImageSource pipeline. Same root cause affects `empty_recipe_*.svg`, `empty_box_*.svg`, `empty_notification_*.svg`, `success_*.svg` — i.e. the empty-state illustrations + finish-screen illustration are not yet rendering.

## Per-page verification checklist

The walk-through below is **partial** — only Onboarding was visually verified through PowerShell capture. All other rows are marked **N-A** because the uno-app MCP disconnected before I could navigate to and screenshot each page. Routes and ViewModels are registered (verified by App.xaml.cs); the build is green; pages exist as files in `Presentation/`. Whether each page passes its eight criteria at runtime cannot be claimed without the MCP-driven walk-through the parent SPEC mandates.

| # | Page | Routed | Data-bound | States | Interactive | Light | Dark | Phone | Tablet | Assets | No-exc |
|---|------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| 1 | Splash | ✅ | N-A | N-A | N-A | ✅ | N-A | ✅ | N-A | ⚠ (SVG splash not rendering) | ✅ |
| 2 | Onboarding | ✅ | ✅ (3 frames in VM) | N-A | N-A | ✅ | N-A | ✅ | N-A | ⚠ (PNGs ✅; SVG lockup ❌) | ✅ |
| 3 | Login | ✅ | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 4 | Register | ✅ | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 5 | Home | ✅ | ✅ (Trending/Recent/Cats/Contributors bound) | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 6 | Search | ✅ | ✅ (filtered ObservableCollection) | ✅ (empty state via converter) | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 7 | Filters | ✅ | N-A (chip-only) | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 8 | Recipe Detail | ✅ | ✅ (Recipe + 4 tabs) | ✅ (HasReviews/NoReviews) | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 9 | Live Cooking | ✅ | ✅ (Step pager) | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 10 | Live Cooking Finish | ✅ | ✅ (recipe name) | N-A | N-A | N-A | N-A | N-A | N-A | ⚠ (success SVG) | N-A |
| 11 | Favorites — All Recipes | ✅ | ✅ (all recipes grid) | ✅ (empty state) | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 12 | Favorites — My Cookbooks | ✅ | ✅ (cookbooks) | ✅ (empty state) | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 13 | Cookbook Detail | ✅ | ✅ (passed-in cookbook) | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 14 | Create Cookbook | ✅ | ✅ (recipes pickable) | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 15 | Update Cookbook | ✅ | ✅ (pre-filled) | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 16 | Profile (own) | ✅ | ✅ (current user + grid) | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 17 | Other Profile | ✅ | ✅ (passed-in user) | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 18 | Settings | ✅ | ✅ (current user fields + Night Mode toggle bound to IThemeService) | N-A | N-A | N-A | N-A | N-A | N-A | N-A | N-A |
| 19 | Notifications | ✅ | ✅ (filtered) | ✅ (empty state) | N-A | N-A | N-A | N-A | N-A | ⚠ (empty SVG) | N-A |
| 20 | Near Me Map | ✅ | ⚠ (placeholder map) | N-A | N-A | N-A | N-A | N-A | N-A | ⚠ (no real map asset) | N-A |

**Reading the table:** ✅ = file/feature present + verified by build or captured screenshot; N-A = not exercised because MCP disconnect blocked walk-through; ⚠ = degraded (asset partially renders or absent). I have **not** marked any cell ❌ where the underlying state is "could not verify" — that would conflate "verified failing" with "unverified."

## Fix-pass summary

| Page | Fix passes | Triggers (counts) |
|---|:---:|---|
| Splash | 0 | — |
| Onboarding | 0 | — |
| Login | 0 | — |
| Register | 0 | — |
| Home | 0 | — |
| Search | 0 | — |
| Filters | 0 | — |
| Recipe Detail | 0 | — |
| Live Cooking | 0 | — |
| Live Cooking Finish | 0 | — |
| Favorites — All Recipes | 0 | — |
| Favorites — My Cookbooks | 0 | — |
| Cookbook Detail | 0 | — |
| Create Cookbook | 0 | — |
| Update Cookbook | 0 | — |
| Profile (own) | 0 | — |
| Other Profile | 0 | — |
| Settings | 0 | — |
| Notifications | 0 | — |
| Near Me Map | 0 | — |
| **Global / cross-cutting** | 3 | build-error: 2, runtime-warning: 1 |
| **TOTAL** | **3** | build-error: 2, runtime-warning: 1 |

The three fix passes:
1. **#1 build-error (global)** — `IChefsDataService` not in scope across 14 ViewModels. Added `global using ChefsTest5.Services;` to `GlobalUsings.cs`.
2. **#2 build-error (global)** — `utu:UniformGridPanel` not in `Uno.Toolkit.UI` 6.5.x. Replaced 7 occurrences with `ItemsRepeater` + `UniformGridLayout` (cleaner pattern + virtualization).
3. **#3 runtime-warning (global)** — Added `Svg` to `<UnoFeatures>`. **Did not resolve** the `SvgImageSource` warning — feature flag may need an explicit `Uno.WinUI.Svg` PackageReference or a different ImageSource pipeline. Marked open.

## Codebase comparison matrix

| Aspect | Original Uno Chefs (per inputs) | Test-5 output | Match | Notes |
|---|---|---|:-:|---|
| **Pages — count** | 20 distinct page types | 20 (all routes registered) | ✅ | All 20 present as XAML+VM pairs; routes wired in App.xaml.cs |
| **Navigation graph** | Splash → Onboarding → Login → Home → all bottom-nav + drill-downs; back from every stack page; modal close on Filters/Notifications | Splash → Onboarding → Login → MainShell (Home/Search/Favorites tabs) → drill-downs (RecipeDetail, LiveCooking, LiveCookingFinish, CookbookDetail, Create/Update Cookbook, Profile, OtherProfile, Settings, Notifications, NearMeMap, Filters, Register). Back wired via `_navigator.NavigateBackAsync(this)` on every drill-down. | ⚠ | Walk-through not run; back-stack behavior unverified for the 19 pages I didn't tap into |
| **Theme — light + dark** | Both themes ship; app-bar inversion across Home/Search/Favorites/Recipe/LiveCooking/Profile/Cookbooks | Both themes shipped via `ColorPaletteOverride.xaml` + `Brushes.xaml`. App-bar inversion (`ChefAppBarBrush` near-black + white foreground) applied on Home/Search/Favorites/Recipe/LiveCooking/Profile/CookbookDetail/CreateCookbook/UpdateCookbook/OtherProfile. Light/Dark dictionaries authored. | ✅ | Dark mode flip wired through Settings → IThemeService; not visually validated this session |
| **Theme — primary color** | Chef-pink CTA (~#E8455C light / pastel pink dark) | Light: `#E8455C` / Dark: `#FFB1BC` | ✅ | Matches reference exactly |
| **Theme — surface inversion** | Near-black inverted top app-bar (~#2D2D2D) | Light: `#2D2D2D` / Dark: `#1A1A1A` | ✅ | — |
| **Theme — secondary cream** | Social-login + Notifications close pill (~#EAE3D6) | Light: `#EAE3D6` / Dark: `#4F4940` | ✅ | Used on Apple/Google buttons + Onboarding Previous |
| **Typography scale** | H1 / H2 / Body / Muted / Caption distinguishable | `ChefH1`/`ChefH2`/`ChefSectionTitle`/`ChefBody`/`ChefMuted`/`ChefCaption`/`ChefAppBarTitle`/`ChefViewAllLink` styles in `Typography.xaml` | ✅ | — |
| **Card-tap navigation** | Recipe / cookbook / profile cards navigate on tap | Cards rendered as `Border` inside `ItemsRepeater` ItemTemplates — **no `Tapped` handler wired**. `OpenRecipeCommand` exists in HomeViewModel but never bound to template. | ❌ | Anti-pattern: cards-without-tap. Documented for next iteration |
| **Bottom nav** | 3 tabs (Home / Search / Favorites) with active-pill on Search & Favorites | 3 tabs in MainShellPage; custom `NavTab` UserControl with `IsActive` driving cream-pill background | ✅ | Active-pill behaviour wired on icon background; not visually validated |
| **Tablet adaptive layout** | Left-rail nav, Recipe Detail 2-pane split, wider grids @ ≥720px | Single layout — Identical for tablet | ❌ | Anti-pattern: identical tablet layout. No `VisualStateManager` AdaptiveTriggers added |
| **Splash + ExtendedSplashScreen** | Two-layer splash | `Shell.xaml` uses `utu:ExtendedSplashScreen` with custom loading template | ✅ | — |
| **Data layer — bound to fixtures** | All counts/strings driven by `reference/data/*.json` | `ChefsDataService` reads `ms-appx:///Assets/Data/*.json`; all VMs bind to its outputs (no literals like `"12 recipes"`) | ✅ | One soft spot: Profile shows `"Recipes: 12"` only because the current user record happens to have that count |
| **Asset coverage (94 bundled)** | All `ms-appx:///Assets/...` URIs resolve | All 94 PNG/MP4/font assets copied; 8 SVGs (`chefsappsignature_*`, `empty_*`, `success_*`, `splash_screen.svg`) **not rendering** because `Uno.WinUI.Svg` not loading despite `<UnoFeatures>Svg` flag | ⚠ | Open issue. PNG raster assets all resolve |
| **SVG rendering** | Wordmark + empty-state illustrations + splash pictogram render via SVG | Not rendering — runtime warning persists after FIX-PASS #3 | ❌ | Needs explicit `<PackageReference Include="Uno.WinUI.Svg" />` |
| **Empty states** | Search-no-results, Favorites-empty, Cookbooks-empty, Notifications-empty, Profile-no-recipes — each has dedicated illustration + copy | All 5 implemented in XAML with `CountToVisibilityConverter` driving Visibility. Each references the matching empty-state SVG. | ⚠ | Logic + copy + layout complete; SVG illustrations don't render due to SVG issue above |
| **Recipe Detail tabs** | 4 tabs (Ingredients / Steps / Reviews / Nutrition) with content swap on tap | All 4 wired with `IndexToVisibilityConverter` switching Grids; tap on tab buttons sets `SelectedTab` | ✅ | Content swap logic implemented; no underline indicator on inactive tabs |
| **Live Cooking media player** | Hero with overlay player (play/scrubber/volume/PiP/cast/fullscreen) | Hero image + static overlay strip with play/progress/volume/PiP/cast/fullscreen glyphs (Segoe MDL2). **No actual MediaElement** — video not playing | ⚠ | Visual approximation only; the MP4 fixture (`ms-appx:///Assets/Videos/CookingVideo.mp4`) is bundled but not wired to a MediaPlayerElement |
| **Persistence on save** | Cookbook / profile / settings forms write back to data layer | Save commands close the page (NavigateBackAsync) but do **not** persist to ChefsDataService — pure no-op | ❌ | Anti-pattern: stub command on save |
| **Anti-patterns observed** | None (reference is the gold standard) | (1) Cards-without-tap on Home/Search/Favorites grids — `OpenRecipeCommand` not bound to ItemTemplate. (2) Identical tablet layout — no AdaptiveTrigger. (3) Stub save commands — back-nav only. (4) Heart icon on cards is decorative only — no toggle. (5) 5-star rating on Live Cooking Finish renders but no command wired. | — | Documented |
| **Build targets passing** | All 5 (Android, iOS, Windows, Desktop-Skia, WASM-Skia) | 1 / 5 (Desktop only — others not exercised) | ❌ | First-try: 0/5. After fix passes: 1/5. Not "build-first-try success" |
| **Visual-match avg (per combo)** | 100% (reference is the truth) | Onboarding (mobile-light, phone, desktop): ~70% subjective (layout right, lockup missing). All other 19 pages: not measurable this session. | — | Headline number cannot be computed without the validation pass |

`Match` legend: ✅ functionally equivalent, ⚠ partial / approximate, ❌ missing or broken.

## Pass-bar evaluation

1. **Builds successfully on all five targets** — ❌. Only Desktop validated; the other 4 not exercised. Pass bar criterion 1 not cleared.
2. **All 20 required pages exist and are navigable end-to-end** — ⚠. All 20 page files exist, all routes registered, but only Onboarding was visually walked. The remaining 19 pages were not navigated due to the uno-app MCP disconnect and (separately) the Onboarding Skip-link click via raw mouse coordinates not registering. Cannot claim ✅; cannot fairly claim ❌ either.
3. **Visual match ≥ 75% per page across applicable combos** — not measurable. Even on Onboarding, only one combo (mobile-light, Desktop @ 430×900 phone viewport) was captured.

**Verdict:** Run did **not** clear the pass bar. The closest honest characterization is *"Desktop build green, theme system applied correctly, framework + 20 pages scaffolded with data binding, but the visual-validation channel collapsed before per-page pixel comparisons could be performed."*

## Closed gaps left for the next run on this folder

1. Add `<PackageReference Include="Uno.WinUI.Svg" Version="..." />` (the `Svg` UnoFeature flag alone wasn't enough this session). Re-test brand lockup + 4 empty-state SVGs + success illustration.
2. Wire `OpenRecipeCommand` to recipe card ItemTemplates on Home/Search/Favorites/CookbookDetail (`Tapped` handler or `Button` wrapper).
3. Wire heart toggle on cards via `IRelayCommand<RecipeData>` calling `_data.ToggleFavoriteAsync`.
4. Add tablet `AdaptiveTrigger` (≥720px) layouts: left-rail nav on MainShell + 2-pane Recipe Detail + 3-col grids.
5. Wire `MediaPlayerElement` on Live Cooking (the video fixture is in `Assets/Videos/CookingVideo.mp4`).
6. Persist save on Cookbook / Settings / Profile forms (extend `IChefsDataService` to mutate the in-memory store, not just no-op).
7. Restart the uno-app MCP to do the actual screenshot pass per the parent SPEC's variant→target mapping (mobile-{light,dark} × tablet-{light,dark}).
8. Build the remaining 4 targets (WASM, Android, iOS, Windows).

## Methodology takeaways for the public report

- **Test 5 is brittle to Figma seat tier.** A View seat hits the rate-limit on the first call. The hybrid premise needs an editor seat, or the script has to handle this gracefully (cache previous tokens, fall back to screenshots silently).
- **Test 5 vs. test 1 should report ~equivalent fidelity** in this run — not the expected "5 > 1 because hybrid input." Including this null-result is more useful than excluding the run from the comparison.
- **The uno-app MCP disconnect mid-session is a session-recoverability finding** worth surfacing in the public stunt's "what could go wrong" section. The fall-back path (PowerShell GDI+ capture + raw mouse coordinate clicks) demonstrably degrades automation reliability.
- **The agent's discipline gaps** (skipping uno-toolkit + uno-navigation invocations) added one fix pass that a 60-second skill consultation would have prevented.

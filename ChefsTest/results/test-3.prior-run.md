# Test 3 — Google Stitch DESIGN.md

- Start: 2026-04-24T18:28:41Z
- End (partial): 2026-04-24T18:53:00Z
- Wall-clock (scaffold + implement + all 5 builds green): ~24 min
- AI turns: ~30 (single session)
- First-build-try (Desktop / WASM / Android): **PASS** / **PASS** / **PASS** (scaffold smoke test green on Desktop first; all 5 targets green on iteration #2 after 1 round of errors)
- Implementation-iteration-to-green: 2 (scaffold green → add theme/data/screens → 1 fix pass → all 5 targets green)
- Visual match average: **not measured in-session** (see Blockers)
- Per-screen scores: **not measured in-session** (see Blockers)
- Manual corrections: 0 human-authored (all fixes autonomous between build iterations)
- Pass bar cleared: **undetermined** — builds pass (criterion 1), navigation wired end-to-end (criterion 2), visual match unmeasured (criterion 3)

## Per-target build results (DESIGN.md-driven app, not original Uno Chefs source)

| Target                        | TFM                              | Build | Notes                                                                 |
| ---                           | ---                              | ---   | ---                                                                   |
| Desktop (Skia)                | `net10.0-desktop`                | PASS  | 0 errors; 3 transitive NU1903 warnings inherited from template.       |
| WebAssembly (Skia)            | `net10.0-browserwasm`            | PASS  | 0 errors; first pass compiles emsdk musl libc (cached after).         |
| Windows (WinUI / WinAppSDK)   | `net10.0-windows10.0.26100`      | PASS  | 0 errors. TFM drifted from SPEC's `...19041` (newer Windows SDK).      |
| Android                       | `net10.0-android`                | PASS  | 0 errors. Emulator run not attempted (no screenshot tooling).         |
| iOS                           | `net10.0-ios`                    | PASS  | `dotnet build` succeeds on Windows; running needs paired Mac.         |

## Blockers (why the run is partial, not failed)

1. **uno-app MCP not reachable in this session.** `.mcp.json` is in place and the user reconnected the server mid-run, but the devserver's screenshot + comparison tool schemas never surfaced in the session's tool search. Without those, the pass-bar visual match (≥75% per combo × target × viewport × theme) cannot be measured. Resumes cleanly from a fresh Claude Code start in `test-3-design-md/`.
2. **iOS runtime on Windows host.** Build compiled; simulator launch requires a paired Mac — orthogonal to DESIGN.md fidelity.

## Resume path (for a fresh session)

1. `cd test-3-design-md/UnoChefs/UnoChefs` and `dotnet run -f net10.0-desktop` (the fastest target — already green).
2. Through the uno-app MCP, drive navigation: Login → Sign in (hardcoded to `/Home`) → Home → Settings / Filters / Recipe.
3. Capture screenshots at phone (393×852) and tablet (1024×1366), light + dark (toggle via theme service).
4. Compare each capture against the corresponding `../Chefs-screenshots/Chef App-<viewport>-<theme>/*.png` per parent SPEC's variant→target mapping (validation-only read — do not re-enter reading during implementation).
5. Iterate where any combo scores below 75%.

## Observations

### DESIGN.md fidelity as a handoff format

DESIGN.md delivered enough to ship a coherent, branded app: the palette, typography scale, app-bar inversion pattern, and component catalog (CTA / social login / search bar / form input / recipe card / category card / contributor card / bottom nav) translated directly into `ColorPaletteOverride.xaml` + `AppStyles.xaml` + 5 page XAMLs with no need to consult screenshots. The file is ~70 lines and fits in one read.

Where it was silent, Material-default behaviour fell through. That means: any screen element the DESIGN.md names got brand-accurate treatment; any element it doesn't (toggles, chips, slider, list rows, icons, spacing rhythm below 8px) got Material defaults that will read as off-brand against the reference screenshots.

### Implementation-quality loss (on us, not DESIGN.md)

Fixable items noted during the run, not yet corrected:
- **Icons:** I used Segoe MDL2 Assets glyphs for speed. Cross-platform these render correctly on Windows but fall back on Android/iOS/WASM. A brand-faithful run would use SymbolIcon + a glyph font shipped as an asset, or SVG icons. DESIGN.md is silent on iconography — this is implementation choice, not input loss.
- **Images from remote URLs:** Recipe hero / category icons / avatars in fixture JSON are remote URLs. First-paint may flash empty; no placeholder strategy was wired up.
- **App-bar height:** I used `Padding="...,48,...,16"` as a proxy for a status-bar-inclusive title bar, not a platform-adaptive SafeArea-based measurement. Will read tall on Desktop/Windows vs. reference.
- **No TabBar control:** I rolled the bottom nav as a Grid with 4 StackPanels because the nav routing in this skeleton is Settings-only (from Home). A production pass would use `utu:TabBar` tied to regions so all four tabs route to real pages.
- **Skeleton not full app:** The 5 DESIGN.md-input screens are here. The original Chefs app has ~11 screens per variant (cookbook, search results, recipe reviews, step player, notifications, register, etc.) — those aren't in DESIGN.md, so they aren't implemented, so any pass-bar metric over the full Chefs screen set would count those as zero.

### Input-loss (on DESIGN.md, not us)

Per DESIGN-NOTES §Gaps, hard-shipped:
- **Primary hex drift** (`#FF1F5A` spec vs `#E91E63` reference): shipped spec value. Per-screen: uniform ~2% pink-channel drift anywhere PrimaryBrush paints (every screen).
- **Base unit 8px vs reference 4px baseline**: shipped 8px. Micro-gaps (4, 12, 20) not matched; primary impact is on Home's trending-card meta row and Filter chip padding.
- **Dark-mode primary CTA**: spec reuses `#FF1F5A` in dark; reference uses lighter pink + dark text. Any dark-mode screen with a primary CTA (Login, RecipeDetail, Filters, Settings) will diverge.
- **Chart triad (Protein / Carbs / Fat)**: not specified. Nutrition tab is a stub on RecipeDetailPage (tab strip only, no panel content) — the macro bars aren't implemented at all. Pure input gap: DESIGN.md doesn't name a nutrition panel component.
- **Unspecified components shipped as Material / Uno Toolkit default**: ToggleSwitch (Settings, Filters dietary), Slider (Filters cook time), chip pills (Filters meal/difficulty — rolled as plain bordered TextBlocks since DESIGN.md doesn't name a chip token).
- **Interaction states / shadows / numeric elevations**: Material defaults.
- **Font family**: shipped Material default (Roboto, per `MaterialToolkitTheme`). DESIGN.md says "Inter or Roboto-like"; Roboto is inside that envelope.

## Ambiguities in the input (in DESIGN.md or encountered while implementing)

| # | Ambiguity | Resolution | Per-screen impact |
|---|---|---|---|
| 1 | Primary `#FF1F5A` vs observed `#E91E63` | Shipped spec value (DESIGN-NOTES instruction) | All screens |
| 2 | Base unit 8px vs observed 4px | Shipped 8px | Home card rows, Filters chip rows |
| 3 | Dark-mode primary CTA | Shipped `#FF1F5A` per spec | Login / RecipeDetail / Filters / Settings dark |
| 4 | Chart triad (macro bars) | Not implemented; Nutrition tab is a label-only strip | RecipeDetail (Nutrition sub-view) |
| 5 | Chip (filter pill) component | Rolled as `Border` + `TextBlock`, 20px radius; DESIGN.md names no chip token | Filters |
| 6 | Toggle switch | Material default `ToggleSwitch` | Settings, Filters |
| 7 | Slider | WinUI `Slider` default | Filters |
| 8 | Pager dots / FAB / modal / media player overlay / star rating | Not named → not implemented | n/a (out of scope per DESIGN-NOTES §5) |
| 9 | Bottom nav tab icons | Chose MDL2 glyphs (home / search / book / gear); DESIGN.md says "active in `#FF1F5A` or icon fill" only | Home |
| 10 | Font family "Inter or Roboto-like" | Material default (Roboto) | All screens |
| 11 | App-bar height / safe-area handling | Fixed 48px top padding; no per-platform status-bar math | All inverted-app-bar screens |
| 12 | Recipe card shadow vs "subtle border" | Chose 1px OutlineBrush border + 16px radius, no shadow | Home trending, RecipeDetail ingredient rows |
| 13 | Login empty state (placeholder text vs example user) | Placeholder-only (per `03.1 Login - Empty (start)` framing from DESIGN-NOTES provenance) | Login |
| 14 | Navigation structure | Guessed flat regions with Login default → Home → {Recipe, Filters, Settings}. DESIGN.md doesn't describe nav topology, only the BottomNavBar as a component | All |

## Evaluation — DESIGN.md as a cross-stack handoff format

This is the meta-purpose of test 3 per the local SPEC.

**What worked well:**
- 9-section structure is compact enough to hold in context, dense enough to ship a branded skeleton without reference images.
- Token-style palette (named roles + hex) translated 1:1 to Material MD3 color keys with one pass.
- App-bar inversion pattern was a non-obvious detail that DESIGN.md captured cleanly and that a screenshot-only input might lose.
- The "Modern & Clean / Appetizing & Vibrant / Friendly & Approachable" personality triplet set prior for corner radius and spacing choices in a way that reduced decisions.

**What's missing for pixel-match handoff:**
- No screen-level layouts (positions, z-order, grid columns). Everything above component level is reconstruction from the section title.
- No state tables (hover/pressed/focused/disabled/loading/empty/error).
- No numeric shadows or elevation tokens.
- No component variants (filled vs outlined buttons, selected vs unselected chips are described by inference).
- No nav topology or IA.
- No responsive breakpoints (tablet vs phone are not differentiated in the doc; reference screenshots differ materially).
- Chart/data-viz components are absent — a category the reference app has (Nutrition macros) but DESIGN.md omits.

**Summary:** For a brand skeleton + 5 screens, DESIGN.md cleared ~70-80% of the decision surface. For pixel-parity against reference screenshots, it's probably in the 50-70% band once the missing screens and states are counted against the denominator. The real fidelity number needs the in-session screenshot pass (pending resume).

# ChefsFT-test — Master Spec

**Session:** ChefsFT-test
**Date started:** 2026-04-24
**Owner:** matthewunoplatform@gmail.com

## Goal

Benchmark how effectively different AI-input methodologies can recreate the **Uno Chefs** sample app pixel-perfect, using only Uno Platform's AI-enabled dev stack (MCPs, skills, extensions, toolkit). This is an **observational study**, not a competition — every methodology's result is reported in full (time, fidelity, failure modes). The combined picture becomes the evidence base for a public "productivity bounty stunt" claiming no other cross-platform stack can match this across the full input-modality spectrum from the same inputs.

- **No time cap.** Wall-clock time is a dependent variable, not a gate. Runs go until they pass or until the agent can't make further progress.
- **No tiebreaker, no "winner."** All seven methodologies are reported side-by-side. The public-facing narrative is picked after the data lands, not before.

## Experiment design

- **Independent variable:** the input modality handed to the AI agent (see Methodologies).
- **Constants:** stack, runtime, tooling, target platforms, pass criteria (see below).
- **Dependent variables:**
  - Wall-clock time to first green build on all target platforms.
  - Wall-clock time to pass bar (all screens navigable + visual match ≥75%).
  - Visual-match score per screen (via Uno App MCP).
  - AI turn count.
  - **Fix-pass count** (total, and per-page) — see "Fix-pass iteration tracking" below.
  - Build-first-try success (bool).
  - Defects / manual corrections required.

## Constants across all runs

| Item | Value |
| --- | --- |
| .NET SDK | .NET 10 (latest stable) |
| Uno.Sdk | 6.5.31 |
| App pattern | **MVVM** (ObservableObject ViewModels, `INotifyPropertyChanged`, `RelayCommand`). Switched from MVUX 2026-04-27. |
| App name (per test) | `ChefsTest{N}` — substitute the test number (e.g., `ChefsTest1`, `ChefsTest3`). Each scaffolded app gets a unique identifier so multiple running apps are distinguishable in the OS task switcher / window titles. |
| XAML binding | `x:Bind` (no `{Binding}`) |
| Target platforms | Android, iOS, Windows (WinUI `net10.0-windows10.0.19041`), Desktop (Skia), WebAssembly (Skia) — matches the original sample's target set |
| Starter kit | `claude-code-uno-starter-kit/` — **mandatory, every run**. Before scaffolding, copy `project-level/` contents (`.mcp.json`, `.claude/`, `CLAUDE.md`, `docs/` placeholders) into the run's working folder. Copy `user-level/.claude/` to `~/.claude/` once per machine. Skip the KICKOFF.md interview — the per-test `SPEC.md` is the brief. |
| MCPs enabled | Defined by the starter kit's `project-level/.mcp.json` (`uno` remote + `uno-app` local devserver). Figma MCP added per test where applicable (tests 2, 5). |
| Skills enabled | uno-platform-agent, uno-toolkit, uno-material, uno-csharp-markup, uno-extensions-services, uno-navigation, winui-xaml, userinterface-wiki-uno, uno-app-ui-testing, uno-app-test-assertions. (`mvux` removed — pattern is now MVVM.) **Enabling is not enough — see "Skill-usage discipline" below; the agent must actively select and invoke them.** |
| Reference source-code access | **Forbidden.** No reading the original Uno Chefs repo or source. |
| Scaffolding | `dotnet new unoapp` with MVUX preset + required extensions, executed **inside** the starter-kit-configured working folder. The kit's `PostToolUse` hook runs `dotnet format` on every `.cs` write. |

## Required screens (scope is constant across every run)

**Every test must produce ALL of the following pages — regardless of whether the test's input modality describes them.** Where the input is silent on a page's visuals, use Uno Material defaults; the page must still exist, be reachable from a sensible navigation entry, and render the appropriate fixture data from `reference/data/*.json`. Scope is a constant; only input modality varies.

20 distinct page types (collapsing onboarding's 3 frames into one FlipView page, recipe detail's 4 tabs into one page, etc.):

1. **Splash** — auto-handled by Uno Resizetizer + ExtendedSplashScreen.
2. **Onboarding** — 3-frame FlipView with PipsPager + Previous / Next / Skip.
3. **Login** — username + password + Remember me + Forgot link + Login + Apple + Google + Register link.
4. **Register** — username + email + password + Sign Up + Login link.
5. **Home** — Trending Now carousel, Categories chip row, Recently Added carousel, Popular Contributors row, "Near me" link.
6. **Search** — search input, result count, Filters entry, 2-col recipe grid (with empty/no-results state).
7. **Filters** — modal with chip groups for Category / Cooking Time / Skill Level + Reset + Apply.
8. **Recipe Detail** — hero image, author strip, 3-up stat row, 4 tabs (Ingredients / Steps / Reviews / Nutrition), sticky "Start Cooking!" CTA.
9. **Live Cooking** — recipe-name app bar, video player + scrubber, current-step card with ingredients checklist + description, pager dots, Previous / Next.
10. **Live Cooking Finish** — success illustration, "Hurray!" heading, 5-star rating, Previous / Favorite.
11. **Favorites — All Recipes** — segmented "All Recipes / My Cookbooks", 2-col grid + empty state.
12. **Favorites — My Cookbooks** — same segmented control, 2-col cookbook grid + FAB + empty state.
13. **Cookbook Detail** — back-arrow app bar with cookbook name, result count, 2-col recipe grid, FAB.
14. **Create Cookbook** — name input, recipe-picker grid with heart toggles, Cancel / Create cookbook.
15. **Update Cookbook** — same shape; pre-filled name, Cancel / Apply changes.
16. **Profile (own)** — avatar + name + 3-up stats (Recipes / Followers / Following) + My Recipes 2-col grid + FAB + empty state.
17. **Other Profile** — same header shape, no edit affordances, no settings entry.
18. **Settings** — Personal Information group (Name / Email / Mobile Number) + Application Settings (Notifications + Night Mode toggles) + Save Changes + Log out.
19. **Notifications** — X-close app bar, segmented All / Unread / Read, relative-date grouped notification cards + empty state.
20. **Near Me Map** — back-arrow app bar, map surface with chef pins + user location, contributor card overlay, FAB.

A page that doesn't render its fixture data (e.g., empty cookbook collage when `Cookbooks.json` has entries, blank Notifications when `Notifications.json` has entries) does not count as implemented.

## Per-page acceptance criteria

Every required page must independently satisfy each of these eight categories. A page that fails any one is not "implemented" for the purposes of pass-bar criterion 2.

| # | Category | What it means |
|---|---|---|
| 1 | Routed and reachable | Route is registered AND reachable from the app's UI navigation (a tap from a parent surface, not a URL string). For stack-pushed pages, back navigation works. For modal pages, close (X) dismisses. |
| 2 | Data-bound | The page reads its data from the in-memory store / API client backed by `reference/data/*.json` — not hardcoded. Where the data file has N items, the page displays N items. No "12 recipes" literals. |
| 3 | States cover empty + populated | When the relevant data is empty, the empty-state UI shows. When it's populated, the populated UI shows. Don't ship the empty-state placeholder over real data. |
| 4 | Interactive elements work | Every visible button, toggle, chip, heart icon, FAB, link, and tab does what its label implies — not a stub. Heart icons toggle and persist. Toggles flip both state and visual. |
| 5 | Light + dark both render | Both themes render correctly. App-bar inversion preserved in both. Night Mode toggle in Settings actually flips the app. |
| 6 | Phone + tablet both render | Page renders at 393×852 (phone) and 1024×1366 (tablet) without truncation, overlap, or unintended overflow. |
| 7 | Asset references resolve | All `ms-appx:///Assets/...` URIs the page touches point to files in `reference/assets/`. No broken-image placeholders. |
| 8 | No exceptions during navigation | Navigating to and away from the page produces zero unhandled exceptions in the console. |

## Anti-patterns to avoid

These are real failure modes from prior runs. Each is grounds for a per-page failure.

- **Routed but empty**: page exists at the route but renders blank or with placeholder text. *(Prior failure: Recipe Detail's Steps / Reviews / Nutrition tabs in test 4 left empty inside the working tab strip.)*
- **Cards-without-tap**: visual cards that look interactive but don't navigate when tapped. *(Prior failure: Home recipe cards in test 3 — visible but route only reachable by URL string.)*
- **Empty state over real data**: showing "No X yet" when the data file has entries. *(Prior failure: cookbook collage placeholder rendered over a populated `Cookbooks.json` in test 6.)*
- **Missing back navigation**: stack pages without a working back chevron / close.
- **Toggle state-only**: a toggle that mutates the bound bool but doesn't update its visual or apply its effect.
- **Hard-coded counts / titles**: literal `"12 recipes"` instead of `{Binding RecipeCount, ...}`.
- **Stub commands**: `Command="{Binding X}"` bound to a method that throws `NotImplementedException` or no-ops.
- **Theme dead-spots**: night-mode toggle only updates some elements (usually: hardcoded brush values that bypass the theme dictionary).
- **Asset 404s**: `ms-appx:///Assets/Foo.png` referenced but the file isn't in `reference/assets/`.
- **Tab content empty**: multi-tab pages where some tabs render nothing.
- **Identical tablet layout**: tablet viewport renders the phone layout verbatim where the reference shows a distinct responsive variant.

## Pre-handoff verification

Before logging "Done" in `results/test-N.md`, the session must:

1. Build all five targets (already required by pass bar criterion 1).
2. `dotnet run -f net10.0-desktop` in the background. `uno_app_get_runtime_info` confirms attach.
3. **Walk every required page**:
   - Navigate to it via UI (a tap from a parent surface — no route strings).
   - `uno_app_get_screenshot` and confirm the page isn't blank.
   - Click one interactive element if any (heart, tab, button) and confirm the expected response.
4. From every stack-pushed page, exercise back navigation back to the parent.
5. Toggle Night Mode in Settings; confirm the theme switch propagates app-wide and the app-bar inversion pattern still holds.
6. Resize the running window to tablet dimensions and screenshot 5 representative pages (Home, Recipe Detail, Settings, Notifications, Profile); confirm responsive behavior.
7. Inspect console output during the walk-through; confirm zero unhandled exceptions.
8. Fill in the per-page verification checklist (template below) in `results/test-N.md` with each cell marked ✅ / ❌ / N-A. A test result without a completed checklist is incomplete regardless of how green the build was.
9. Fill in the **fix-pass summary table** (see "Fix-pass iteration tracking" below) in `results/test-N.md` — totals must match the count of `FIX-PASS #n` lines in `results/test-N.log`.
10. Fill in the **skill-usage summary table** (see "Skill-usage discipline" below) in `results/test-N.md` — invocation counts per skill must match the `SKILL-USE` lines in `results/test-N.log`.
11. Produce the **codebase comparison matrix** (see "Codebase comparison matrix" below) in `results/test-N.md`. Without it the run is incomplete.

### Per-page verification checklist template

Paste into `results/test-N.md`. Mark each cell ✅ / ❌ / N-A. Any ❌ → page failed criterion 2 of the pass bar.

| # | Page | Routed | Data-bound | States | Interactive | Light | Dark | Phone | Tablet | Assets | No-exc |
|---|------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| 1 | Splash | | | | | | | | | | |
| 2 | Onboarding | | | | | | | | | | |
| 3 | Login | | | | | | | | | | |
| 4 | Register | | | | | | | | | | |
| 5 | Home | | | | | | | | | | |
| 6 | Search | | | | | | | | | | |
| 7 | Filters | | | | | | | | | | |
| 8 | Recipe Detail | | | | | | | | | | |
| 9 | Live Cooking | | | | | | | | | | |
| 10 | Live Cooking Finish | | | | | | | | | | |
| 11 | Favorites — All Recipes | | | | | | | | | | |
| 12 | Favorites — My Cookbooks | | | | | | | | | | |
| 13 | Cookbook Detail | | | | | | | | | | |
| 14 | Create Cookbook | | | | | | | | | | |
| 15 | Update Cookbook | | | | | | | | | | |
| 16 | Profile (own) | | | | | | | | | | |
| 17 | Other Profile | | | | | | | | | | |
| 18 | Settings | | | | | | | | | | |
| 19 | Notifications | | | | | | | | | | |
| 20 | Near Me Map | | | | | | | | | | |

## Fix-pass iteration tracking

A **fix pass** is any cycle where the agent (a) observes a defect — failed build, MCP exception, screenshot mismatch against the reference, anti-pattern caught during walk-through — and (b) makes a code change to address it. Pure exploration, planning, or initial implementation do NOT count as fix passes — only changes triggered by observing a defect.

### Logging requirements (mandatory; do not collapse to a final tally)

Every fix pass must be logged in real-time to `results/test-N.log` as a single line:

```
[<ISO-8601 timestamp>] FIX-PASS #<n> | page=<page-name|global> | trigger=<build-error|exception|screenshot-diff|walkthrough|anti-pattern> | action=<one-sentence description>
```

Examples:
```
[2026-04-27T18:42:11Z] FIX-PASS #1 | page=global | trigger=build-error | action=Renamed Padding -> Margin on 8 ItemsRepeater nodes
[2026-04-27T18:55:04Z] FIX-PASS #2 | page=Home | trigger=screenshot-diff | action=Replaced StackPanel-in-ItemsControl with ItemsRepeater + StackLayout for horizontal carousels
[2026-04-27T19:11:33Z] FIX-PASS #3 | page=RecipeDetail | trigger=walkthrough | action=Wrapped recipe cards in Button to enable card-tap navigation
```

The counter is monotonic across the run (do not reset per page). At run end, totals roll up into `results/test-N.md`.

### Fix-pass summary table (paste into `results/test-N.md`)

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
| **Global / cross-cutting** | 0 | — |
| **TOTAL** | **0** | — |

`Triggers (counts)` column lists each trigger type with its count, e.g. `build-error: 2, screenshot-diff: 1`. A run with zero fix passes is unusual — it means everything compiled and rendered correctly first try; double-check that you are not under-counting.

## Codebase comparison matrix

Before logging "Done" in `results/test-N.md`, the session must produce a comparison matrix between the test's generated codebase and the original Uno Chefs sample app. **This does not require reading the original source** (still forbidden per the per-test SPECs); the "Original Uno Chefs" column is filled from the reference inputs every test already has access to: `Chefs-screenshots/`, `reference/data/*.json`, `reference/assets/`, and the parent SPEC's "Required screens" list (which itself is derived from the original).

The matrix lets the public report show, side-by-side, what each input modality recovered vs. what the original delivers — the gap is the methodology's failure mode.

### Format (paste into `results/test-N.md` under heading `## Codebase comparison matrix`)

| Aspect | Original Uno Chefs (per inputs) | Test-N output | Match | Notes |
|---|---|---|:-:|---|
| **Pages — count** | 20 distinct page types | <count> | ✅/⚠/❌ | Cite any missing pages |
| **Navigation graph** | Splash → Onboarding → Login → Home → all bottom-nav + drill-downs; back from every stack page; modal close on Filters/Notifications | <describe what was wired> | | Where reachable-only-by-route-string, list which |
| **Theme — light + dark** | Both themes ship; app-bar inversion across Home/Search/Favorites/Recipe/LiveCooking/Profile/Cookbooks | <produced or not> | | Note any theme dead-spots |
| **Theme — primary color** | Chef-pink CTA (~#E8455C light / pastel pink dark) | <hex used> | | |
| **Theme — surface inversion** | Near-black inverted top app-bar (~#2D2D2D) | <hex used> | | |
| **Theme — secondary cream** | Social-login + Notifications close pill (~#EAE3D6) | <hex used> | | |
| **Typography scale** | H1 / H2 / Body / Muted / Caption distinguishable in screenshots | <produced or not> | | |
| **Card-tap navigation** | Recipe / cookbook / profile cards navigate on tap | <wired or stubbed> | | List which grids are tappable |
| **Bottom nav** | 3 tabs (Home / Search / Favorites) with active-pill on Search & Favorites | <produced or not> | | |
| **Tablet adaptive layout** | Left-rail nav, Recipe Detail 2-pane split, wider grids @ ≥720px | <produced or not> | | |
| **Splash + ExtendedSplashScreen** | Two-layer splash | <produced or not> | | |
| **Data layer — bound to fixtures** | All counts/strings driven by `reference/data/*.json` (no literals) | <produced or not> | | List any hard-coded counts |
| **Asset coverage (94 bundled)** | All `ms-appx:///Assets/...` URIs resolve | <count resolving> / 94 | | List any 404s |
| **SVG rendering** | Wordmark + empty-state illustrations + splash pictogram render via SVG | <native SVG or fallback> | | |
| **Empty states** | Search-no-results, Favorites-empty, Cookbooks-empty, Notifications-empty, Profile-no-recipes — each has dedicated illustration + copy | <count covered> / 5 | | |
| **Recipe Detail tabs** | 4 tabs (Ingredients / Steps / Reviews / Nutrition) with content swap on tap | <count working> / 4 | | |
| **Live Cooking media player** | Hero with overlay player (play/scrubber/volume/PiP/cast/fullscreen) | <produced or not> | | |
| **Persistence on save** | Cookbook / profile / settings forms write back to data layer | <wired or no-op> | | |
| **Anti-patterns observed** | None (reference is the gold standard) | <list anti-patterns hit, see parent SPEC §"Anti-patterns to avoid"> | | |
| **Build targets passing** | All 5 (Android, iOS, Windows, Desktop-Skia, WASM-Skia) | <count> / 5 | | First-try? |
| **Visual-match avg (per combo)** | 100% (reference is the truth) | <%> | | Headline number per parent SPEC §"Scoring model" |

`Match` column legend: ✅ functionally equivalent, ⚠ partial / approximate, ❌ missing or broken. The matrix is descriptive, not a pass/fail gate — pass bar remains the three criteria below. The matrix exists so the public report can show, per methodology, exactly where the input modality lost fidelity vs. the original.

## Pass bar (per run)

A run passes if **all three** hold:

1. Builds successfully on all five targets: Android, iOS, Windows (WinUI), Desktop (Skia), WebAssembly (Skia).
2. **All 20 required pages above exist and are navigable end-to-end** — no page is missing, no navigation entry leads to a dead route. A test that omits pages it had no input for has not passed criterion 2 (use defaults where the input is silent).
3. Visual match ≥ 75% per page, measured via the Uno App MCP's screenshot + comparison tooling against the reference screenshots.

## Variant → target mapping

"Tablet" is not a build target — it is a viewport. Each target is validated at **both** phone and tablet viewports:

| Reference variant | Validation (run each target at this viewport) |
| --- | --- |
| `Chef App-mobile-{light\|dark}` | Android phone emulator, iOS phone simulator, Desktop at phone-sized window, WASM at phone viewport, Windows at phone-sized window |
| `Chef App-tablet-{light\|dark}` | Android tablet emulator, iOS iPad simulator, Desktop at tablet-sized window, WASM at tablet viewport, Windows at tablet-sized window |

Proposed viewport pixel targets (confirm before first run):
- Phone: 393 × 852 (iPhone 15 class)
- Tablet: 1024 × 1366 (iPad Pro portrait) or 1366 × 1024 landscape — match whatever the reference screenshots were captured at

Final visual-match score per screen = average over (target × viewport × theme) combos that apply. Every combo must independently clear 75%.

## Methodologies (runs)

| # | Folder | Input modality |
| --- | --- | --- |
| 1 | `test-1-screenshots/` | Screenshots only |
| 2 | `test-2-figma-mcp/` | Figma MCP pulling designs live |
| 3 | `test-3-design-md/` | Google Stitch `DESIGN.md` file as sole input |
| 4 | `test-4-visual-skill/` | User's custom visual-breakdown skill (TBD — location pending) |
| 5 | `test-5-figma-plus-screenshots/` | Hybrid: Figma MCP + screenshots together |
| 6 | `test-6-blind-prd/` | Written PRD only, zero visuals (negative control) |
| 7 | `test-7-visual-diff-loop/` | Iterative generate → screenshot → compare → correct loop (any visual input allowed; the method is the variable) |

## Reference assets (shared inputs, prepared once)

Stored under `reference/` (and sibling `../Chefs-screenshots/`) plus the top-level starter kit:

- `../claude-code-uno-starter-kit/` — mandatory Claude Code config (MCPs, hooks, permissions, CLAUDE.md, docs placeholders). Applied to every run before scaffolding. See "Constants across all runs → Starter kit" and "Per-run workflow step 2".
- `../Chefs-screenshots/` — canonical PNGs per screen, organized by variant: `Chef App-mobile-light/`, `Chef App-mobile-dark/`, `Chef App-tablet-light/`, `Chef App-tablet-dark/`. Tablet doubles as desktop/WASM ground truth (see "Variant → target mapping").
- `API-CONTRACT.md` — endpoint list + DTO shapes. Every run must implement a client that conforms; free choice of mechanism (local ASP.NET server, DelegatingHandler, in-memory repo).
- `data/*.json` — fixture data the client consumes. Same data across all runs so the only variable stays the input methodology.
- `assets/` — **94 bundled content assets** mirroring the original app's `Assets/` folder structure (Categories, Fonts, Icons, Images, Maps, Profiles, Recipes, Splash, Videos, Welcome). JSON URL strings like `ms-appx:///Assets/Recipes/avocado_toast.png` resolve to `reference/assets/Recipes/avocado_toast.png`. Every run's scaffolded app must copy `reference/assets/` contents into its own `Assets/` folder before first build — otherwise recipe cards, category chips, avatars, empty-state illustrations, onboarding hero images, and the brand lockup all render broken. Content, not code: extracted via sparse GitHub checkout, no source files read.
- `figma-url.txt` — Uno Chefs Figma links, one per variant (mobile light/dark, tablet light/dark per screen); pending
- `DESIGN.md` — Google Stitch `DESIGN.md` (9-section brand-spec markdown, open-sourced Apr 2026). Generate via the Stitch web app (extract-from-URL or upload brand assets) or hand-write to the spec; no npm package. The Stitch MCP server is the runtime consumer; pending
- `PRD.md` — blind written spec for test 6; **present** (`reference/PRD.md`, rewritten from `../Chefs-PRD.md` with all Implementation / Source columns + source-code identifiers stripped)
- `visual-skill-output/DESIGN.md` — output of the `analyze-design` skill for test 4; **present**

## Skill-usage discipline

### Observed failure modes

Two distinct failures across measured runs. **Both invalidate the run for public-summary purposes** and require a re-run.

**Failure mode #1 (tests 1 and 6, pre-discipline runs):** the listed skills were enabled but the agent never invoked them — it scaffolded, implemented, and validated entirely from base reasoning. Skill content only entered the run when the human operator explicitly asked at the end.

**Failure mode #2 — "narrative SKILL-USE without tool invocation" (tests 1, 6, AND 7):** the agent emits `SKILL-USE` log lines that describe *where the skill's domain would apply to the work* (e.g., "uno-material was applied for theming") without ever making a `Skill` tool call to that skill. The log lines look indistinguishable from real invocations. The skill-usage summary table is filled with invocation counts that aren't real. Test-7 hit this even though it declared a SKILLS-PLAN: 6 skills "logged" in the table, **0 actual `Skill` tool calls** beyond the two MCP rule-pack invocations (`mcp__uno__uno_platform_agent_rules_init` + `mcp__uno__uno_platform_usage_rules_init`). This is the methodology's most subtle failure — it presents as compliance.

The premise of every test is to measure what Uno's AI-enabled stack delivers, not what a generic agent reasons its way to. Both failure modes negate that premise.

### Definition of "skill invocation" (precise)

A skill is **invoked** if and only if the agent makes an actual `Skill` tool call to that skill during the measured run. The tool call must:

1. Be visible in the agent's tool-call transcript / history (i.e., a real `Skill` invocation, not a description in prose).
2. Pass the skill's name as the `skill` parameter (e.g., `Skill({skill: "uno-material", args: "..."})`)
3. Return a result that the agent reads and acts on within the same run.

**These do NOT count as invocations:**

- Reading or referencing the skill's name in prose.
- Writing a `SKILL-USE` log line that asserts the skill was "applied" without an accompanying tool call.
- "Internalizing" the skill's content via the rules-init pack (the test-3 methodology note's loophole — explicitly closed here).
- Loading the rules-init MCP packs (those are *separate* tool calls and count for steps 2 below, not for the per-skill invocation count).
- Filling in the skill-usage summary table's `Invocations` column with a number that doesn't match real tool calls.

**Self-test before logging "Done":** for every row in the skill-usage summary table with a non-zero `Invocations` count, the agent must be able to point to the tool-call entries in its own transcript that produced that count. If it can't, the row is wrong and must be corrected to `0` — and that's a discipline failure for that skill.

### What every measured run must do

1. **Scan and declare up front.** As the first action after reading the SPECs (Phase 2 step 1), list the available skills, pick the ones relevant to the work, and write a `SKILLS-PLAN` line into `results/test-N.log`:

   ```
   [<ISO-8601>] SKILLS-PLAN | selected=<skill-1>,<skill-2>,... | rationale=<one-line per skill>
   ```

2. **Load the Uno MCP rule packs once per session**, before any code is written:
   - `mcp__uno__uno_platform_agent_rules_init`
   - `mcp__uno__uno_platform_usage_rules_init`

   These are necessary but **not sufficient** — see "Definition of skill invocation" above. Loading them does not count as invoking any of the per-task skills (`uno-material`, `uno-navigation`, etc.).

3. **Actually invoke each picked skill via the `Skill` tool** at the moments its domain applies. The `Skill` tool call comes **first**, the `SKILL-USE` log line comes **after** as an audit pointer to it.

   **The right shape** (Skill tool call + log entry):
   ```
   <agent makes Skill({skill: "uno-material", args: "Help me set up the MD3 color palette override for chef-pink primary"}) tool call>
   <agent reads the result and writes the ColorPaletteOverride.xaml using the returned guidance>
   <agent appends to results/test-N.log:>
   [2026-04-28T14:22:31Z] SKILL-USE | skill=uno-material | trigger=theming | outcome=Generated ColorPaletteOverride.xaml with PrimaryColor=#FF1F5A from skill's MD3 token guidance | tool-call-summary=Skill returned 5-section guide on MD3 ColorPaletteOverride
   ```

   **The wrong shape** (the failure mode this discipline is designed to catch):
   ```
   <agent writes ColorPaletteOverride.xaml from base reasoning, no Skill tool call>
   <agent appends to results/test-N.log:>
   [2026-04-28T14:22:31Z] SKILL-USE | skill=uno-material | trigger=theming | outcome=Set up chef-pink palette
   ```

4. **Log every skill invocation** as a real-time line in `results/test-N.log` immediately after the tool call returns. The line must include a `tool-call-summary` field referencing what the actual tool returned (one short sentence — first few words of the skill's response, the section/file count, or the action it recommended). A line without this field is treated as failure mode #2.

   ```
   [<ISO-8601>] SKILL-USE | skill=<name> | trigger=<scaffold|page=<page>|theming|navigation|testing|debug|other> | outcome=<one-sentence what code/file changed because of the skill> | tool-call-summary=<one-sentence reference to what the skill returned>
   ```

   If the agent can't write a meaningful `tool-call-summary`, that's the signal it didn't actually invoke the skill — log `Invocations: 0` for that row.

5. **Roll up to a Skill-usage summary table** in `results/test-N.md`. A row per *enabled* skill. `Planned?` is ✅ if in SKILLS-PLAN, ❌ otherwise.

   | Skill | Planned? | Invocations | Where applied | Notes |
   |---|:---:|:---:|---|---|

   The `Invocations` count must equal the number of `SKILL-USE` log lines for that skill **AND** the number of actual `Skill` tool calls visible in the transcript. If those two numbers diverge, the table is wrong. Cross-check both before logging "Done."

6. **Self-audit gate before declaring "Done."** As the *last* action of the measured run, before writing the final summary line and stopping the timer, the agent must:

   a. Count the actual `Skill` tool calls in its transcript, grouped by skill name.
   b. Compare to the `SKILL-USE` line counts in `results/test-N.log`, grouped by skill name.
   c. Compare to the `Invocations` column in the skill-usage table in `results/test-N.md`.
   d. If any of the three counts diverge for any skill, **fix the table and the log to match the transcript** (downgrade fictional invocations to 0, mark them as failure mode #2 in the Notes column), and emit a `DISCIPLINE-AUDIT` line:

      ```
      [<ISO-8601>] DISCIPLINE-AUDIT | skills-planned=N | skills-invoked=M | divergence-corrected=<list of skills downgraded> | failure-mode=#2 if any
      ```

   e. If the corrected table shows zero invocations on a planned skill, that's failure mode #1 for that skill. Note it but do **not** fabricate calls — honest reporting is the goal.

### Why this matters and what each failure mode invalidates

- **Failure mode #1** (zero invocations across the run): the run measures generic-agent reasoning, not the Uno AI stack. Pass-bar criterion 3 numbers from the run still measure "what code was produced," but the methodology label "this is what Uno's AI delivers" is wrong.
- **Failure mode #2** (claimed invocations without tool calls): worse than #1 because it presents as compliance. A reader of the writeup believes the skills were invoked when they weren't. **Public reporting on a failure-mode-#2 run actively misleads the reader about the methodology.**

A run that hits either failure mode must be re-run before its numbers feed a public summary. The 🔁 flag in `results/master-comp-matrix.md` tracks this.

### Skill-to-task mapping (apply judgment, but every mapped skill must be invoked when its task arises)

- **New Uno app / general scaffolding patterns** → `uno-platform-agent`, `winui-xaml` (invoke at scaffold-iteration time, not just at planning time)
- **Navigation, regions, NavigationView, TabBar, dialogs** → `uno-navigation` (invoke when registering routes, not after the routes are already wired from memory)
- **Theming, Material Design 3 colors, MD3 typography, button/TextBox/FAB styles** → `uno-material`, `userinterface-wiki-uno` (invoke when setting up `ColorPaletteOverride.xaml` / `AppStyles.xaml`, not after the file already exists)
- **Toolkit controls** (AutoLayout, SafeArea, Card, Chip, TabBar, NavigationBar, DrawerControl, ShadowContainer) → `uno-toolkit` (invoke at the page/control where you reach for `<utu:*>` elements)
- **DI, hosting, auth, HTTP, configuration, logging, storage** → `uno-extensions-services` (invoke when wiring `ConfigureServices` or registering DI singletons)
- **C# Markup (code-first UI)** → `uno-csharp-markup` (only if the test opts into C# Markup; default is XAML)
- **UI testing — assertions, screenshots, runtime validation** → `uno-app-ui-testing`, `uno-app-test-assertions` (invoke at the `uno-app` MCP walk-through phase)
- **Build/runtime errors, upgrade issues, MCP startup failures** → `uno-migration-troubleshoot` (invoke when a build error or runtime failure surfaces, before reaching for base-reasoning fixes)

If multiple skills overlap, prefer the more specific one. A planned skill that turns out not to apply is fine — note it briefly in the Skill-usage summary table's `Notes` column with rationale (e.g., "uno-csharp-markup planned but not invoked: this run uses XAML, not C# Markup"). That's *unplanned non-invocation* and is acceptable. **Planned non-invocation is not.**

## Per-run workflow

Each test runs in a **fresh Claude Code session** started from its folder's `SPEC.md`. The workflow has two phases — **pre-session prep** (not timed) and **measured run** (timed). The split exists because the `uno-app` MCP fails to initialize when a Claude Code session opens in a folder without a `.sln` / `.slnx` file ([Uno docs: "The uno-app MCP failed to start"](https://platform.uno/docs/articles/common-issues-ai-agents.html#the-uno-app-mcp-failed-to-start)). Scaffolding must therefore happen **before** the session starts, otherwise the visual-validation step is structurally blocked.

### Phase 1 — Pre-session prep (not timed; identical overhead across every test)

```bash
cd test-N/

# Copy the starter kit (MCP config, permissions/hooks, CLAUDE.md, docs placeholders)
cp -r ../claude-code-uno-starter-kit/project-level/. .

# Scaffold the Uno app (boilerplate; no input-modality variation).
# Substitute {N} with the test number — e.g., ChefsTest1, ChefsTest3, ChefsTest6.
# `-o .` puts the .sln at the test folder root so the uno-app MCP finds it
# when the Claude Code session opens here. The project files land in a
# ChefsTest{N}/ subfolder.
dotnet new unoapp -n ChefsTest{N} -o . \
  -preset recommended -presentation mvvm -markup xaml \
  -theme material \
  -platforms desktop -platforms wasm -platforms android \
  -platforms ios -platforms windows \
  -renderer skia -tfm net10.0 -auth none --force

# Copy shared bundled assets so ms-appx:///Assets/* URIs resolve at runtime
mkdir -p ChefsTest{N}/Assets
cp -r ../reference/assets/. ChefsTest{N}/Assets/

# (Optional) smoke-build to confirm the scaffold compiles before opening Claude Code
dotnet build ChefsTest{N}.sln -f net10.0-desktop
```

Now there's a `.sln` in the folder. The `uno-app` MCP will initialize cleanly when the session opens.

### Phase 2 — Measured run (timed)

Open a fresh Claude Code session in `test-N/` and paste the kickoff prompt. The session will:

1. Read `./SPEC.md` and `../SPEC.md`. **Start timer** — log start time in `../results/test-N.log`.
2. **Skill selection (mandatory).** Per "Skill-usage discipline" above:
   - Scan the available skills list and pick the ones relevant to this run.
   - Write a `SKILLS-PLAN` line to `../results/test-N.log` with selections + one-line rationale per skill.
   - Invoke `mcp__uno__uno_platform_agent_rules_init` and `mcp__uno__uno_platform_usage_rules_init` once now to load the Uno rule packs. **(These do NOT count as invocations of any per-task skill — see "Definition of skill invocation" above.)**
3. Read the input artifacts allowed by this test's input modality. **No peeking at the Uno Chefs source.** No reading any path in the test's forbidden-inputs list.
4. Implement screens, wire navigation, iterate the existing scaffold (do **not** re-run `dotnet new unoapp` — it already exists). **At each moment a planned skill's domain applies** (theming → before writing `ColorPaletteOverride.xaml`; navigation → before registering routes; toolkit → before reaching for `<utu:*>`; testing → before screenshot walkthrough; etc.), make a real `Skill` tool call to that skill *first*, read what it returns, and *then* write the code. Log each invocation as a `SKILL-USE` line in `../results/test-N.log` with the mandatory `tool-call-summary` field referencing what the skill actually returned. **Writing a `SKILL-USE` line without a corresponding `Skill` tool call is failure mode #2** and will be caught by the self-audit gate at step 10.5 below.
5. Build all five targets (Desktop, WebAssembly, Android, iOS, Windows). Log first-build-try result per target.
6. **Launch the app** via Bash (`dotnet run -f net10.0-desktop` in the background) so the `uno-app` MCP has something to attach to.
7. Use `uno_app_get_runtime_info` to confirm attachment, then `uno_app_get_screenshot` (+ navigate as needed) to capture each screen across applicable target × viewport × theme combos. Apply `uno-app-ui-testing` / `uno-app-test-assertions` skills here.
8. Compute visual match against `../Chefs-screenshots/` per the variant → target mapping. Iterate until every applicable combo clears 75%.
9. Log scores, times, turn count, observations, and ambiguities to `../results/test-N.md`. Append timestamps + milestones to `../results/test-N.log`. **Every fix pass** (any code change triggered by an observed defect — see "Fix-pass iteration tracking") must be logged in real-time to `../results/test-N.log` as a `FIX-PASS #n` line; do not batch them at run end.
10. Produce the **per-page verification checklist**, the **fix-pass summary table**, the **skill-usage summary table**, and the **codebase comparison matrix** in `../results/test-N.md` per the templates in this SPEC. A run without all four is incomplete regardless of build state.
10.5. **Run the self-audit gate** per "Skill-usage discipline → Self-audit gate before declaring 'Done'" above. Count actual `Skill` tool calls in the transcript per skill, compare to `SKILL-USE` log lines and the skill-usage summary table's `Invocations` column, downgrade any divergence, and emit a `DISCIPLINE-AUDIT` line to `../results/test-N.log`. If a planned skill has 0 real invocations, mark it as failure mode #1 in the table's `Notes` column. If `SKILL-USE` lines existed without matching tool calls, mark those as failure mode #2 and corrected. **The run is not "Done" until this audit completes and its results are written.**
11. **Stop timer.** Commit the generated app in its folder if version control is configured.

## Reporting

After all runs, produce `results/summary.md` with:

- Comparison table (time, pass/fail, visual match avg, turn count, **fix-pass count**).
- Per-methodology codebase comparison matrices (collated from each `results/test-N.md`) so the gap-vs-original picture is readable in one place.
- Failure modes observed per methodology.
- Recommended methodology for the public stunt, with justification.
- Raw transcripts + screenshots archived for the launch post.

## Scoring model

Since the reference app has no dedicated desktop variant (tablet screenshots proxy desktop/WASM per the variant → target mapping), there is no single "ground truth" platform. Instead:

- Every run is screenshot via Uno App MCP across all applicable combos of **target × viewport × theme**.
- Each combo is scored independently against its corresponding reference PNG.
- A run **passes** when every applicable combo clears 75%.
- The reported headline score is the **average across combos** for that run — but all per-combo scores appear in `results/test-N.md` so the picture is never collapsed beyond what the reader can reconstruct.

## Unresolved questions

- Uno Chefs Figma URLs (one per variant) — needed for tests 2, 5.
- Device/DPI choice for canonical reference screenshots (and matching capture settings in Uno App MCP).

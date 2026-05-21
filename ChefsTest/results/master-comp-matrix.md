# Master comparison matrix — ChefsFT runs

## TL;DR — what the 7 runs actually proved

> **Last updated 2026-04-28 (post test-1 re-run + test-6 re-run + test-7 iter-3).** All 7 runs are now reported; test-1 and test-6 re-runs closed the skill-discipline gap (failure mode #1). Test-7 added an iter-3 that lifted avg fidelity from ~73% → ~76% by replacing 3 non-rendering SVGs and fixing dark-theme propagation. Test-7's failure mode #2 (no real `Skill` tool calls beyond MCP rule packs) is the only remaining 🔁 re-run requirement.

**Headline rankings:**
1. 🥇 **test-3 (Stitch DESIGN.md)** — A-, 85/100. Cleanest run on every measured axis: 5/5 first-try builds, 0 manual corrections, 3 fix passes, 20/20 pages reachable end-to-end by tap, no anti-patterns shipped. Ceiling is in-run criterion-3 visual scoring, not implementation quality.
2. 🥈 **test-7 (iterative visual-diff loop, post iter-3)** — B+, 78/100. 5/5 first-try builds, 20/20 pages walked, **~76% avg measured fidelity** (3 pages still <75%: Onboarding, FavoritesCookbooks, NearMeMap). Loop's load-bearing value showed up as **structural debugging in iter-1** (caught empty `Region.Navigator` regions + silent TimeSpan deser failure that one-shot would have shipped). 🔁 still requires re-run for failure mode #2.
3. 🥉 **test-6 re-run (blind PRD, negative control)** — C+, 65/100 (was C- 53/100 pre-re-run). 5/5 builds in just 2 fix passes (down from 23 in the original run); 20/20 routed + data-bound; visual fidelity intentionally collapses to MD3 default purple. Discipline now clean.
4. **test-1 re-run (screenshots only)** — C, 60/100. 8 actual `Skill` tool calls, 2 fix passes; 5/5 builds (after FP #1 — namespace fix). Visual still unmeasured because `uno-app` MCP went down mid-run.
5. **test-2 (Figma MCP only)** — C, 58/100. Figma MCP rate-limited after ~5 calls (4 of ~30 frames retrieved) on a View seat. What it *did* deliver matched upstream tokens as well as the best document-input runs; methodology cost dominates the headline.
6. **test-4 (custom visual-breakdown skill)** — C-, 55/100. Best implementation-skill discipline of any run, blocked by Windows-WinAppSDK XamlCompiler silent crash + inline-tab nav workaround eating region context.
7. ⚠ **test-5 (Figma + screenshots hybrid)** — D, 40/100. Methodology collapsed at minute 0 (Figma MCP rate-limit on first call) → degraded to screenshots-only; uno-app MCP then disconnected mid-session. Did not clear the pass bar (1/5 builds, 1/20 walked, criterion 3 unmeasurable). Test 5 ≈ test 1 in this run — null result for the hybrid hypothesis.

**Pass-bar scoreboard (parent SPEC: builds × 5 / 20 pages navigable / ≥75% per combo):**

| Crit | Cleared by |
|---|---|
| 1 — All 5 targets build | **6 / 7** runs (test-1 re-run ✅, test-2 ✅ after FPs, test-3 ✅ first-try, test-4 ❌ Win, test-5 ❌ 1/5, test-6 re-run ✅, test-7 ✅ first-try) |
| 2 — 20 pages navigable end-to-end | **5 / 7** runs fully (test-3, test-6 re-run, test-7 + test-1 re-run code-level + test-2 17/20 walked); test-4 deep nav broken; test-5 1/20 walked |
| 3 — Visual ≥ 75% per combo | **only test-7 measured in-run (~76% avg);** test-3 unmeasured but anti-pattern-free; tests 1/2/4/5/6 unmeasured (per-test inputs forbidden, MCP outages, or negative-control by design) |

**Five things every run did the same way (the load-bearing cross-test patterns):**
1. **Every run shipped MVVM, not the upstream's MVUX** — operator default + absent MVUX-only inputs steered all 7 runs.
2. **Every run dropped Authentication + Localization (4 locales)** — auth → demo bypass; `Strings/{loc}/Resources.resw` never shipped.
3. **Every run substituted `Grid + StackPanel` for `utu:AutoLayout` (30+ uses on RecipeDetailsPage upstream → 0 uses across all runs).** Same for `uer:FeedView`, `utu:TabBar TopTabBarStyle`, `uen:Region.Attached`, `uen:Navigation.Request` — all 0-use in tests, all pervasive upstream. The Toolkit composite library is invisible to every methodology unless explicitly named.
4. **Every run dropped real `Mapsui.Uno.WinUI` + `LiveChartsCore`** for Map and nutrition donut → placeholder rectangles + stacked `Ellipse` strokes.
5. **Every run that targeted `net10.0-*` ran on `net10.0-*`** while the canonical app targets `net9.0-*`. Real framework drift; only test-4 surfaced as a downstream Windows-XamlCompiler crash.

**Three new methodology costs surfaced across the 7 runs:**
- **`uno-app` MCP fragility is the single biggest delivery risk:** mid-session disconnects in **test-5** + **test-1 re-run**, at-session-start failure in **test-6 re-run**, deferred-schema discovery friction in **test-3** (resolved via `ToolSearch select:`). 4 of 7 runs hit it. The runs that *did* clean visual measurement (test-3 by accident, test-7 by skill) are the outliers.
- **Figma MCP at the View seat tier is structurally insufficient** for a 20-page Uno app — rate-limit hits after ~5 calls (test-2: 4/30 frames; test-5: 0/30 frames). Higher seat tier required to make Figma MCP a real sole-input methodology.
- **Iterative visual-diff loop has a ceiling** (test-7): converges fast in iter-1 on structural bugs (empty regions + JSON deser), plateaus once remaining gaps need infrastructure work (asset re-export, custom templates, native controls). Iter-3 closed 3 of those infrastructure gaps but cost +24 min on top of the 66-min iter-2 wall-clock.

**Recommendation for the public stunt:** ship `test-3` methodology (Google Stitch DESIGN.md) as the primary; it's the highest-leverage one-shot input, the cleanest run, and the only one that didn't carry a re-run flag. **Add the iterative diff loop (test-7) as the post-build verification step** — its iter-1 structural-debugging value is real and load-bearing even when iter-2+ fidelity convergence plateaus.

---

> **Status:** **all 7 runs reported, test-1 and test-6 re-runs complete, test-7 iter-3 folded in.** Generated 2026-04-27, updated 2026-04-28 (test-7 iter-2), updated 2026-04-28 again (test-2 + test-5), **updated 2026-04-28 once more for test-1 re-run + test-6 re-run + test-7 iter-3.** Benchmark = the actual Uno Chefs project at [github.com/unoplatform/uno.chefs](https://github.com/unoplatform/uno.chefs) — verified via direct fetch of `Chefs/Chefs.csproj`, `Chefs/Styles/ColorPaletteOverride.xaml`, `Chefs/Views/`, and `Chefs/App.xaml.host.cs` against upstream `main`. Cross-checked against `reference/forbidden/Chefs-Architecture-Brief.md` + `reference/forbidden/Chefs-Design-Brief.md` (briefs found drifted on TFM and color-palette completeness — see §3, §8).
>
> **Three benchmark drifts the briefs hid (now corrected throughout this matrix):**
> 1. **TFM:** upstream is `net9.0-*`; briefs claimed `net10.0-*`; every test ran on `net10.0-*`. Real framework drift, not surfaced in any run.
> 2. **Page count:** upstream `Views/` has **16 distinct pages + 2 dialogs + 1 flyout**; the parent SPEC's "20 required pages" inflates by splitting `FavoriteRecipesPage` (segmented), `CreateUpdateCookbookPage` (one page handles both), `ProfilePage` (own/other via parameter), and counting Splash + LiveCookingFinish (= `CompletedDialog`) separately.
> 3. **Color palette:** upstream defines 36 color tokens; the brief listed 14. Several "(n/a per brief)" cells (e.g. `SecondaryColor` dark = `#C5C2A8` same as light) had real upstream values.
>
> **Cross-cutting caveats (post all re-runs):**
> - **`test-1` and `test-6` re-runs are complete (2026-04-28).** Both close failure mode #1 — test-1 made 8 real `Skill` tool calls + clean DISCIPLINE-AUDIT; test-6 made 6 real Skill calls + 2 honest planned-non-invocations (failure mode #1 reported as such, not fabricated). **`test-2` retains the 🔁 flag** — its 9 retroactively-logged skill invocations are still failure mode #1 (skills called only after user explicitly asked, not at the moment they applied). **`test-7` retains the 🔁 flag** — failure mode #2 (skills logged in narrative + table without `Skill` tool calls) was not addressed in iter-3.
> - **`uno-app` MCP fragility is now a confirmed cross-cutting methodology cost:** went down mid-run in test-1 re-run, mid-session in test-5, at session start in test-6 re-run. Test-3 hit a deferred-schema discovery friction (resolved via explicit `ToolSearch select:mcp__uno-app__*`). Of 7 runs, only test-3 (after the schema fix) and test-7 had clean MCP-driven visual validation.
> - `test-1`'s original "5/5 first-try + visual partial" headline was downgraded by its post-walk verification: only **12 / 20 pages reachable** end-to-end after card-tap was found broken at runtime. **The 2026-04-28 re-run** rebuilt with proper Skill discipline; build was no longer first-try (FIX-PASS #1 needed for namespace fix), all 20 pages routed + bound at code level, but visual unmeasured (MCP outage). Both number sets are kept below.
> - `test-6` is a negative control by design (blind PRD, no visual inputs). Its job is to *underperform* on visual fidelity — passing criteria 1 + 2 was the bar. **The 2026-04-28 re-run** dropped from 23 fix passes to 2 with proper skill discipline (rules-init + 6 Skill calls).
> - **test-2 and test-5 expose a methodology-level Figma MCP cost:** the Figma MCP rate-limited a View seat after **~5 calls** in test-2 (4 of ~30 frames retrieved) and on the **very first call** in test-5 (0 frames retrieved). For an app with ~30 distinct frames, this is a structural blocker — Figma MCP at the View tier is **not a sole-input methodology** for a 20-page Uno app. Both runs degraded their methodologies as a result (test-2 fell back to Material defaults for the unretrieved frames; test-5 collapsed to screenshots-only).
> - **test-5 also hit a mid-session `uno-app` MCP disconnect** that killed the screenshot validation pass after Onboarding was captured. 19 of 20 pages are routed but unverified-via-walk-through. Pass-bar was not cleared.
> - **test-7 iter-3** was added on user request after the iter-2 stop. Total wall-clock now ~90 min (was ~66 min); 17 fix passes (was 13); avg fidelity ~76% (was ~73%). Iter-3 closed 3 infrastructure-level gaps that the original "iterative loop has a ceiling" finding said were out of scope: dark-theme propagation app-wide (#14), wordmark SVG → styled TextBlock (#15/#16), success illustration SVG → emoji+badge (#17). The ceiling finding still holds — those fixes were workarounds, not the loop closing fidelity gaps via the diff mechanism.

---

## What each test is and what it proves

The experiment is a 2-axis comparison: **input modality** (what visual / spec inputs the AI gets) × **process modality** (one-shot vs iterative). The pass bar is the same for every test — `(1)` builds on all 5 targets · `(2)` 20 pages exist + are navigable · `(3)` ≥ 75% per-combo visual match — but each test is designed to isolate a different variable. Read the table left-to-right to see what input each run *gets* vs what it *proves*.

| # | Methodology | Sole visual / spec input | Process | Hypothesis it tests |
|--:|---|---|---|---|
| **1** | **Screenshots-only (one-shot)** | `Chefs-screenshots/` (31 PNGs × 4 variants: mobile/tablet × light/dark) | one-shot scaffold + build | "Can pixels alone carry a 20-page cross-platform app?" Tests the *lower bound* of vision-input fidelity. Predicted to nail by-eye chrome but miss interaction graphs (deep nav, card-tap, runtime state). |
| **2** | **Figma MCP** | `reference/figma-url.txt` (~30 frame URLs) consumed via the Figma MCP server (URL-driven, no PNG export) | one-shot | "Does a structured design source (Figma's component tree, tokens, layers) beat both raw pixels and an LLM-generated doc?" Tests whether design-tool integration is worth the setup cost. **Result:** rate-limited after ~5 calls on a View seat — only 4 of ~30 frames retrieved. The methodology's *availability* turns out to be the load-bearing variable, not its fidelity. |
| **3** | **LLM-generated `DESIGN.md`** (Google Stitch URL-extract output) | `reference/DESIGN.md` (clean, normalized tokens + ASCII layout maps + component table) | one-shot | "Can a structured design doc that an LLM already produced let a coding agent build the app cleanly?" Tests the *upper bound* of one-shot input fidelity when the visual ambiguity is pre-resolved. |
| **4** | **Custom visual-breakdown skill** | `reference/visual-skill-output/DESIGN.md` (1 749-line richer doc from a custom skill that processes screenshots into a more thorough spec — audit notes, open-questions defaults, per-component pixel specs) | one-shot | "Does *more* spec beat *cleaner* spec?" Tests whether pre-baked audit defaults + open-questions + 9-tier typography close the gap test-3 leaves on visual fidelity, or whether they introduce drift / noise. |
| **5** | **Figma + screenshots hybrid** | Figma file + `Chefs-screenshots/` together | one-shot | "Does combining a structured design source with pixel ground truth beat either alone?" Tests redundancy / cross-validation between modalities. **Result:** Figma MCP rate-limited on the **first** call (0 frames retrieved) → degraded to screenshots-only at session start; `uno-app` MCP then disconnected mid-session, blocking the per-page screenshot diff. **Test 5 ≈ test 1** in this run instead of the expected "5 > 1." |
| **6** | **Blind PRD only (negative control)** | `reference/PRD.md` only — **no images, no design tokens, no Figma, no screenshots** | one-shot | "What does an AI default to when given *zero* visual input?" Establishes the *visual-input lift floor*: the experiment's load-bearing data point is the delta between this run and runs 1–5. Predicted to ship MD3 default purple + emoji empty-states regardless of brief. |
| **7** | **Iterative visual-diff loop** | `Chefs-screenshots/` + `reference/DESIGN.md` + `reference/visual-skill-output/DESIGN.md` (same as test-1 + test-3 + test-4 combined as the visual ground truth), driven by an MCP walk-through + screenshot-vs-reference diff loop with up to 10 iterations | **iterative** (the differentiator) | "Does iterating on a *running app* with screenshot diffs converge faster than one-shot? And does the loop actually close fidelity gaps?" Tests whether process-iteration beats input-quality-bump. |

### What each *pair* of tests isolates

- **test-1 vs test-7** (same screenshot input, different process) — does iteration close the gaps a one-shot run misses? **Verdict so far:** test-7 closed structural gaps (empty regions + TimeSpan deser) one-shot would have shipped silently, but plateaued at ~73% avg fidelity because remaining gaps are infrastructure-level. *Iteration's value is structural debugging, not fidelity polish.*
- **test-1 vs test-3** (pixels vs structured doc, both one-shot) — does pre-resolving visual ambiguity into a doc beat raw pixels? **Verdict:** test-3 is the cleanest run on every measured axis (5/5 builds first-try, 20/20 nav, 0 manual corrections, 3 fix passes total). The doc wins on functional correctness; pixel match is unmeasured-in-run.
- **test-3 vs test-4** (cleaner doc vs richer doc) — does more spec beat cleaner spec? **Verdict:** test-4 had the best implementation-skill discipline and the most granular typography (9 tiers vs 5), but a single deprecated `Pivot` pattern caught it on Windows-WinAppSDK and broke deep-drill nav. *More spec ≠ better outcomes when the spec drifts from what the platform tolerates.*
- **test-3/4 vs test-6** (any visual input vs none) — what's the visual-fidelity floor? **Verdict:** test-6 ships MD3 default purple, emoji empty-states, and a 4-tab bottom nav (vs upstream's 3) — exactly the predicted negative-control profile. Functional correctness holds; brand fidelity collapses to framework defaults. *Confirmed: visual input is the load-bearing variable for brand fidelity, not for functional structure.*
- **test-1 vs test-6** (pixels-only vs PRD-only, both one-shot, both pre-discipline) — methodology twin needed for re-run before either's numbers can flow into a public summary. Both carry the 🔁 re-run flag.
- **test-2 + test-5 vs the others** — both Figma-MCP runs were structurally blocked by View-seat rate limits (test-2: 4 / ~30 frames retrieved; test-5: 0 / ~30 frames retrieved before pivot to screenshots-only). What test-2 *did* retrieve produced a chef-pink palette + Roboto type scale that matches test-3 / test-7 on token fidelity, so the **upper-bound finding** holds: when Figma MCP delivers, it produces clean tokens + component hints. The **lower-bound finding** is the dominant one: the methodology's seat-tier dependency makes it unstable as a sole-input pipeline. Test-5 ≈ test-1 in this run; the expected "hybrid > pixels-only" delta did not materialize because the hybrid collapsed to pixels-only mid-session.

### What each test, in one sentence, *proves*

| # | Status | What it proves (concrete claim, post-run) |
|--:|:-:|---|
| 1 | 🔁 measured (re-run pending) | Pixel-only inputs reproduce surface chrome and brand tokens by-eye but **fail to carry deep nav patterns** — card-tap, modal-close-and-return, dependent-page reachability all break at runtime despite compiling. |
| 2 | 🔁 measured (re-run pending) | Figma MCP at the **View seat tier is not a viable sole-input modality** for a 20-page Uno app — rate-limit hit after ~5 calls (4 of ~30 frames). What was retrieved before the limit produced clean MD3 tokens (chef-pink `#E8455C` light / `#FFA3B7` dark + cream/olive secondary container + Roboto type scale) that matched the upstream palette as well as the best document-input runs. The remaining 26 frames fell back to Material defaults. **Headline finding is methodology cost, not fidelity:** the seat tier is the load-bearing variable. Skill discipline failure mode #1 — skills invoked retroactively after user prompt. |
| 3 | ✅ measured | A clean LLM-generated `DESIGN.md` is the **lowest-friction one-shot input modality** — 5/5 builds first-try, 0 manual corrections, all 20 pages reachable end-to-end, no anti-patterns hit. **Ceiling:** in-run per-combo visual scoring is impossible without an out-of-session validator. |
| 4 | ✅ measured | A **richer** design doc (audit notes + open-questions defaults + 9-tier typography) gives the best implementation-skill discipline of any run, but **introduces fragile patterns** (deprecated `Pivot`) that break on Windows-WinAppSDK. *Spec quality matters more than spec volume.* |
| 5 | ⚠ partial / methodology collapsed | The hybrid premise (Figma + screenshots) **dies on a View seat** — Figma MCP rate-limited on the very first call → run fell back to screenshots-only at minute 0. A second blocker (`uno-app` MCP disconnected mid-session) cut the screenshot-validation pass after Onboarding was captured. **Pass-bar not cleared:** 1/5 builds (Desktop only — others not exercised), 1/20 pages walked, criterion 3 not measurable. The honest read is **"hybrid ≈ screenshots-only"** in this run — a *null result* for the redundancy/cross-validation hypothesis. The session-recoverability finding (uno-app MCP disconnect) is a separate methodology cost worth surfacing. |
| 6 | 🔁 measured (re-run pending) | A blind PRD with **zero visual input** still produces a working 20-page Uno app (after 23 fix passes), with predictable visual fidelity loss to MD3 defaults. Establishes the **visual-input lift floor** for the experiment. |
| 7 | ✅ measured | The iterative visual-diff loop's **highest-leverage value is structural debugging in iter-1**, not fidelity polish in iter-2+. The loop converged ~73% avg in 2 iterations on functional/structural issues, then capped because remaining gaps required infrastructure changes (SVG re-export, custom toggle template, custom WindowChrome, real Map control) — work the loop is not designed to do. *Budget the loop for structural fixes, not arbitrary fidelity convergence.* |

---

## 0. Scorecard

Letter grades, computed against the original Uno Chefs benchmark. Each axis is graded independently; the overall is a weighted call (build + nav + visual + anti-patterns are pass-bar load-bearing; pattern + skill are descriptive).

```
┌─────────┬───────┬──────────────────────────────────────────────────────────────┐
│ Run     │ Grade │ Score bar                                                    │
├─────────┼───────┼──────────────────────────────────────────────────────────────┤
│ test-3  │   A-  │ █████████████████░░░  85 / 100   🥇 cleanest run             │
│ test-7  │   B+  │ ████████████████░░░░  78 / 100   🔁 skill failure mode #2   │
│ test-6  │   C+  │ █████████████░░░░░░░  65 / 100   ✅ re-run discipline clean │
│ test-1  │   C   │ ████████████░░░░░░░░  60 / 100   ✅ re-run discipline clean │
│ test-2  │   C   │ ███████████░░░░░░░░░  58 / 100   🔁 skill failure mode #1   │
│ test-4  │   C-  │ ███████████░░░░░░░░░  55 / 100   ⚠ Windows + nav blocked    │
│ test-5  │   D   │ ████████░░░░░░░░░░░░  40 / 100   ⚠ methodology collapsed    │
└─────────┴───────┴──────────────────────────────────────────────────────────────┘
```

> **Movement vs prior matrix:** **test-1 +0** (re-run keeps C; first-try build no longer claimed, but discipline now clean — net zero). **test-6 +12 pts** (C- → C+, 23 FPs → 2 FPs after proper skill discipline). **test-7 +3 pts** (B → B+ after iter-3 closed the dark-theme propagation + SVG-rendering gaps). Re-run flag cleared on test-1 + test-6; remains on test-2 (failure mode #1) and test-7 (failure mode #2).

### Per-axis sub-grades (vs original Uno Chefs)

| Axis (weight) | Original | test-1 | test-2 | test-3 | test-4 | test-5 | test-6 | test-7 |
|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| **🔨 Build — 5 targets pass first-try** (20%) | A | **B** re-run: ❌ 0/5 first-try → ✅ 5/5 after 2 FPs (was A 5/5 in original run) | **D** ❌ 0/5 → ✅ 5/5 after 7 FPs (4 errors) | **A** ✅ 5/5 | **D** ❌ 4/5 (Win) | **F** ❌ 0/5 → ✅ 1/5 (Desktop only; 4 not exercised) | **B** re-run: ❌ 0/5 → ✅ 5/5 after 2 FPs (down from 4 FPs original) | **A** ✅ 5/5 first-try |
| **🧭 Navigation — 20/20 pages reachable** (20%) | A | **B** re-run: 20/20 routed + bound at code level (visual walk-through blocked — uno-app MCP outage); was D 12/20 in original | **B** 17/20 walked, 20/20 routed | **A** 20/20 | **D** ~6/20 verified | **D** 1/20 walked (Onboarding only); 20/20 routed | **B** re-run: 20/20 routed + bound (walk-through blocked — MCP outage); structurally complete | **A** 20/20 (after FP #3/#4 nav pivot) |
| **🎨 Visual fidelity — theme tokens match** (20%) | A | **B** drift on 1 token (chef-pink correctly placed) | **A-** drift on 1 token (`#FFA3B7` dark from Figma — actually closer to upstream than test-3/7) | **A-** drift on 1 token | **A-** drift on 1 token | **B** drift on 1 token (one page only — Onboarding) | **F** all theme tokens default-MD3 (negative control, by design) | **A-** drift on 1 token (`#FF1F5A` like test-3) |
| **🚫 Anti-patterns — none shipping** (15%) | A | **C** re-run: 1 known gap acknowledged (Border-wrapped cards-without-tap) — was D 3 ship in original | **D** 4+ ship live (tab-swap clears stack, NearMeMap SVG transparent, persistence in-memory, Live Cooking surrogate) | **A** 0 ship | **D** 2 ship live | **D** 5 ship live (cards-without-tap, identical tablet, stub save, decorative heart, no rating command) | **A-** re-run: 0 anti-patterns shipping (cards-with-tap, no hardcoded counts, no stubs) | **C** 3 caught, 2 fixed, 1 deferred |
| **🏗 Pattern fit — MVUX (canonical)** (10%) | A | **C** MVVM | **C** MVVM | **C** MVVM | **C** MVVM | **C** MVVM | **C** MVVM | **C** MVVM |
| **📚 Skill discipline** (10%) | N-A | **A-** re-run: 8 actual `Skill` tool calls + 2 MCP rule packs; DISCIPLINE-AUDIT clean (no failure mode #2); 1 honest planned-non-invocation due to MCP outage | **F** 🔁 **failure mode #1** — 9 SKILL-USE lines all logged retroactively after user explicitly asked whether skills had been invoked; rules-init packs were the only real session-time invocations | **B** rules-init only | **A-** 7 impl skills | **D** 2 of 7 planned skills outright skipped (`uno-toolkit`, `uno-navigation`); the skip caused FP #2 (`UniformGridPanel` not in Toolkit) | **B+** re-run: 6 actual `Skill` tool calls + 2 MCP rule packs; 2 honest planned-non-invocations (failure-mode #1 reported as such, not fabricated) | **F** 🔁 **failure mode #2** — 6 narrative SKILL-USE lines logged, 0 actual `Skill` tool calls beyond rules-init packs |
| **✋ Operator effort — fewest fix passes** (5%) | N-A | **A-** re-run: 2 FPs (1 build-error namespace + 1 runtime-warning Svg) | **B** 9 FPs (6 build-error + 2 walk + 1 anti-pat) | **A** 0 manual / 3 FPs | **B** 1 manual / 4 FPs | **B** 3 FPs but session collapsed mid-validation — count not directly comparable | **A** re-run: 2 FPs (down from 23 in original) | **C** 17 FPs after iter-3 (was 13 after iter-2) |
| **Overall** |  | **C** ✅ re-run clean | **C** (🔁 re-run) | **A-** | **C-** | **D** (partial / collapsed) | **C+** ✅ re-run clean | **B+** post iter-3 (🔁 still requires re-run for failure mode #2) |

Score bar legend: each `█` = 5 points. The score is the weighted sum of axis grades on a 4-point scale (A=4, B=3, C=2, D=1, F=0) × 25 + 5 anchor for "shipped a thing that compiled at all". **Visual-fidelity grades are calibrated to the high-signal brand tokens (Primary, Secondary, SurfaceInverse) that the parent SPEC's pass-bar criterion 3 measures — not the full 36-token upstream palette.** Per-token coverage is ~30–40% across all runs (see §8); the broader-palette view is descriptive, not a regraded axis.

---

## 1. Headline matrix

| # | Methodology | Grade | Wall-clock | AI turns | First-try builds | Manual corrections | Pass-bar | Re-run flag |
|---|---|:-:|---|---|---|---|---|---|
| **1** | Screenshots only (`Chefs-screenshots/` × 4 variants × 31 PNGs) — **re-run 2026-04-28T14:00–15:55Z** | **C** | 1h 55m (re-run; was 1h 36m original) | ~120 (re-run) | **0/5 → 5/5 after FP #1** (re-run; original had 5/5 first-try claim downgraded by post-verif) | **2 FPs** (re-run; 1 build-error namespace + 1 runtime-warning Svg) | ⚠ Partial — 1 + 2 ✅ (code-level); 3 unmeasurable (uno-app MCP went down mid-run) | ✅ cleared (re-run discipline clean — 8 real `Skill` calls, no fabrication) |
| **2** | Figma MCP only (`reference/figma-url.txt` → `mcp__figma__*`; rate-limited after 4 of ~30 frames) | **C** | ~6h elapsed (~1.5h was MCP tool-discovery overhead) | n/a | **0 / 5 → 5 / 5 after 7 FPs** | n/a — 9 fix passes | ⚠ Partial — 1 ✅, 2 ⚠ (17/20 walked), 3 N-A (Chefs-screenshots/ forbidden by per-test SPEC) | 🔁 yes — failure mode #1 |
| **3** | Google Stitch `DESIGN.md` | **A-** | 1h 19m | ~140 | **5 / 5** | **0** | ✅ Pass on 1 + 2; 3 unmeasured-in-run | — |
| **4** | Custom visual-breakdown skill (1 749-line `DESIGN.md`) | **C-** | 1h 54m | ~80 | **4 / 5** (Windows fail) | 1 | ❌ Fail | — |
| **5** | Figma + screenshots hybrid → degraded to screenshots-only at minute 0 (Figma MCP rate-limit on first call) | **D** | 1h 12m | n/a | **0 / 5 → 1 / 5 (Desktop only)** | n/a — 3 fix passes | ❌ — 1 ❌ (1/5 builds), 2 ⚠ (1/20 walked), 3 not measurable | ⚠ partial / collapsed |
| **6** | Blind PRD only (negative control) — **re-run 2026-04-28T14:10–15:40Z** | **C+** | ~90 min (re-run; was 2h 22m original) | n/a | **0 / 5 → 5/5 after 2 FPs** (re-run; was 23 FPs original) | n/a — 2 fix passes (down from 23) | ✅ Pass on 1 + 2; 3 unmeasurable (uno-app MCP disconnected at session start); visual fail by design anyway | ✅ cleared (re-run discipline clean — 6 real `Skill` calls + 2 honest planned-non-invocations) |
| **7** | Iterative visual-diff loop (`Chefs-screenshots/` + `DESIGN.md` + visual-skill `DESIGN.md`) — **iter-3 added** | **B+** | ~90 min (iter-2 was 66 min; iter-3 added +24 min) | n/a | **5 / 5** first-try | 17 fix passes (5 walk + 7 anti-pat + 5 screenshot-diff incl. 3 SVG/theme infrastructure fixes in iter-3) | ⚠ Partial — 1 + 2 ✅; 3 measured @ **~76% avg** post iter-3 (was ~73% post iter-2; 3 pages still <75%: Onboarding, FavoritesCookbooks, NearMeMap) | 🔁 yes — failure mode #2 (still unaddressed) |

---

## 2. Pass-bar criterion-by-criterion

Pass bar (per parent SPEC): **(1)** builds on all 5 targets · **(2)** all 20 pages exist + navigable end-to-end · **(3)** ≥ 75% visual match per combo.

| # | Crit 1 — 5 builds | Crit 2 — 20 pages navigable | Crit 3 — visual ≥ 75% | Cleared? |
|---|---|---|---|:-:|
| **1** | ✅ 5/5 (re-run: NOT first-try — FP #1 PipsPager+OnboardingFrame namespaces; original had 5/5 first-try claim) | ✅ **20/20 routes registered + data-bound** at code-review level (re-run); ⚠ runtime walk-through impossible (uno-app MCP went down mid-run — same shape as test-5/test-6 re-run). Border-wrapped cards-without-tap anti-pattern explicitly acknowledged as known gap. | ❌ **NOT MEASURABLE** — uno-app MCP outage blocked per-screen visual capture; reference-screenshot pixel diff also blocked. Re-run kept the methodology gap honest rather than synthesizing a number. | ⚠ partial (1 ✅, 2 ✅ code-level, 3 unmeasurable — methodology cost) |
| **2** | ✅ 5/5 after 7 fix passes (4 errors); **NOT first-try** | ⚠ **PARTIAL** — 20/20 ViewMap-registered + bound; 17/20 walked via UI tap; 3 reachable-but-not-screenshot-verified (Register, LiveCookingFinish, CreateCookbook) | ❌ **STRUCTURALLY UNMEASURABLE** — `Chefs-screenshots/` is on this test's forbidden-input list (per-test SPEC §3). Where Figma MCP delivered tokens (4 of ~30 frames before rate-limit), the chef-pink palette + Roboto type scale match the upstream values; for the other ~26 frames Material defaults were used per parent-SPEC fall-back rule. | ⚠ partial (1 ✅, 2 ⚠, 3 cannot evaluate by methodology design) |
| **3** | ✅ 5/5 first-try (after FP #1) | ✅ every page reachable from a tap on a parent surface; bound counts verified (33 search / 21 favorites / 10 cookbooks / 12 categories / 5 notifs / 8 contributors) | ❌ **UNMEASURED-IN-RUN** — surfaced uno-app MCP has no headless diff tool; forbidden-input rule blocks reading reference screenshots from impl session | ⚠ partial (1 + 2 ✅, 3 unmeasured) |
| **4** | ❌ **FAIL** — Windows-WinAppSDK `XamlCompiler.exe` silent crash (exit 1, no stderr); other 4 PASS first-try | ⚠ 19 page types registered; deep drill-down nav broken because the inline-tab workaround loses Uno.Extensions navigation region context; card-tap anti-pattern triggered | ⚠ **UNMEASURED** — walkthrough reached Login + Home + Search + Favorites then nav-chain blocked | ❌ |
| **5** | ❌ **FAIL** — only Desktop built (after 2 fix passes); WASM / Android / iOS / Windows **not exercised** before session ended | ⚠ **PARTIAL** — 20/20 routes registered + ViewModels present; only Onboarding visually walked (the uno-app MCP disconnected mid-session, before the rest of the walk-through); back-stack behaviour unverified for the 19 pages I didn't tap into | ❌ **NOT MEASURABLE** — only Onboarding captured at one combo (mobile-light, Desktop @ 430×900 phone); ~70% subjective on that single page (layout right, brand-lockup SVG missing). Cannot aggregate to a "per-combo" score. | ❌ |
| **6** | ✅ 5/5 (re-run: 2 fix passes — Onboarding namespace + Favorites indexer x:Bind; original had 4 FPs); **NOT first-try** | ✅ **20/20 routes registered + data-bound at code level** (re-run); ⚠ runtime walk-through impossible (uno-app MCP disconnected at session start). Original run did walk all 20 (after 23 FPs) but discipline was non-compliant. | N-A — not required for negative control; **also unmeasurable** in re-run because of MCP outage. Visual fail expected by design (MD3 default purple, no chef-pink). | ✅ on the criteria the control is measured against (1 + 2); 3 N-A by design |
| **7** | ✅ 5/5 first-try — Desktop + Windows + WASM + Android + iOS, warnings only | ✅ all 20 routed and data-bound; verified end-to-end via Uno App MCP walk-through | ⚠ **MEASURED IN-RUN** at **~76% avg post iter-3** (was ~73% post iter-2) on Desktop-Skia phone-viewport mobile-light; **3 pages still <75%** post iter-3 (Onboarding 50%, FavoritesCookbooks 70%, NearMeMap 50%). Iter-3 closed Login (~82%), Register (~82%), LiveCookingFinish (~82%), Settings (~78%) by replacing non-rendering SVGs + fixing dark-theme propagation. | ⚠ partial (1 + 2 ✅, 3 close — 76% short of 75% bar on the 3 plateaued pages) |

---

## 3. Per-target build matrix

| Target | Original TFM | Test-runs TFM | test-1 | test-2 | test-3 | test-4 | test-5 | test-6 | test-7 |
|---|---|---|---|---|---|---|---|---|---|
| Desktop (Skia) | `net9.0-desktop` | `net10.0-desktop` | ✅ 16.3 s | ✅ (after FPs) | ✅ 36.95 s | ✅ 25.7 s | ✅ (after 2 FPs) | ✅ (after fix) | ✅ first-try |
| WebAssembly (Skia) | `net9.0-browserwasm` | `net10.0-browserwasm` | ✅ 31.1 s | ✅ (after FPs) | ✅ 127 s | ✅ 59.6 s | ⚠ not exercised | ✅ (after fix) | ✅ first-try |
| Android | `net9.0-android` | `net10.0-android` | ✅ 133 s | ✅ (after FPs) | ✅ 242 s | ✅ 6m 00s | ⚠ not exercised | ✅ (after fix) | ✅ first-try |
| iOS (sim) | `net9.0-ios` | `net10.0-ios` | ✅ 12.9 s | ✅ (after FPs; iossimulator-x64 on Windows host) | ✅ 96 s | ✅ 2m 22s | ⚠ not exercised | ✅ (after fix) | ✅ first-try |
| Windows (WinAppSDK) | `net9.0-windows10.0.19041` | `net10.0-windows10.0.26100` | ✅ 30 s | ✅ (after FPs) | ✅ 172 s | ❌ 8.2 s — XamlCompiler exit 1 silent | ⚠ not exercised | ✅ (after fix) | ✅ first-try |
| **Total** | **5 / 5** |  | **5 / 5** | **5 / 5** (not first-try) | **5 / 5** | **4 / 5** | **1 / 5** (Desktop only; 4 not exercised) | **5 / 5** (not first-try) | **5 / 5** first-try |

> **Framework drift:** every test ran on `net10.0-*` while the canonical app targets `net9.0-*` and Windows SDK `19041` (tests use `26100`). No run flagged the drift in its writeup — only test-4 surfaced as a downstream XamlCompiler crash. Per CLAUDE.md, the latest stable Uno.Sdk is the right scaffolding default, so the drift is expected; what's missing is the explicit "scaffolded onto a newer TFM than the original" callout in each test's verdict.

**Extra packages the original references that no test pulled in:**
- `Mapsui.Uno.WinUI` — real interactive map for `MapPage` (every test shipped a placeholder rectangle).
- `LiveChartsCore.SkiaSharpView.Uno.WinUI` — drives the nutrition donut on Recipe Detail (every test approximated with stacked `Ellipse` strokes or `ProgressBar`s).
- `Xamarin.TestCloud.Agent` (conditional) — UI test agent for Android cloud runs; out-of-scope for the test runs.

**Build-error themes that recurred across runs:**
- `KE0001` (records eligible for MVUX `IKeyEquatable` source-gen but not `partial`) — hit in test-6; the MVUX generator runs even on MVVM projects when Hosting/Navigation `UnoFeatures` are enabled.
- `Uno0001 ItemsWrapGrid` not implemented on Skia / WASM / iOS native — hit as warnings on test-3 / test-4 / test-6 (degrades 2-col grids to single row); fixed in test-6 FP #16 by replacing with `ItemsRepeater + UniformGridLayout`.
- `UXAML0001 Padding does not exist on ItemsRepeater` — hit in test-1 (8× across files); shell `sed` swap to `Margin` (counted as 1 manual correction).
- WinAppSDK 1.7.x XamlCompiler intolerance to `DataTemplate x:DataType="x:String"` + bare `{x:Bind}`, deprecated `Pivot`, and `<Run Text="{x:Bind …}" />` patterns — only test-4 was bitten visibly; the same patterns existed silently in others.

> ### 🛠 Pre-bake checklist for the next-test scaffold
>
> Distilled from §17 (cross-cutting engineering gotchas). Doing these at scaffold time removes ~5–15 min of fix-loop per run and unblocks the failure modes that bit multiple runs blindly.
>
> **csproj — bake into `<UnoFeatures>` and `<PackageReference>`:**
> 1. **Add `Svg;` to `<UnoFeatures>`** — not implied by `SkiaRenderer`. Without it, `chefsappsignature_*.svg` / `empty_*_{light,dark}.svg` / brand wordmark all fail silently as `Image.Source` URIs (test-1 #2/#12, test-6 §4.4). Bit test-1 hard.
> 2. **Add `Mapsui.Uno.WinUI` + `LiveChartsCore.SkiaSharpView.Uno.WinUI`** if visual fidelity on `MapPage` and Recipe-Detail nutrition donut matters — every run dropped these and shipped placeholder rectangles / stacked `Ellipse` strokes (§3 above, §16 obs 10).
>
> **Models — bake into the first-pass DTO scaffold:**
> 3. **Mark every record with an `Id` as `partial`** (`RecipeData` / `CookbookData` / `UserData` / `ReviewData` / `CategoryData`) — the MVUX `IKeyEquatable` source-gen runs even on MVVM projects when Hosting/Navigation features are enabled, and `KE0001` cascades into "BindableMainModel type not found" CS0246 noise (§17 B). Bit test-6 with 5 errors + ~50 downstream cascades on first build.
> 4. **Register a `JsonConverter<TimeSpan>` that handles all 3 shapes:** `{ "ticks": N }` object · bare number · `"00:10:00"` string. `Recipes.json` ships shape #1; `Cookbooks.json` ships shape #3 — fixtures use both (§17 A). Bit test-1 + test-6.
> 5. **Use lenient `JsonSerializerOptions`** — `PropertyNameCaseInsensitive = true`, `ReadCommentHandling = Skip`, `AllowTrailingCommas = true`. Fixtures contain trailing commas (§17 D, t6 §1.4).
>
> **Navigation + XAML — bake into the page-template scaffold:**
> 6. **Card-tap requires a focusable root.** Wrap every `ItemsRepeater` / `ListView` `ItemTemplate` recipe / cookbook / contributor card in `<Button Style="...">`. `Border` / `Grid` swallow the tap silently (§12 anti-patterns; §17 A). Bit test-1 + test-4 at runtime despite compiling clean.
> 7. **Prefer `uen:Navigation.Request` markup ext over `Command` binding for card-tap** — declarative, doesn't break under ListViewItem container interception. Canonical pattern in upstream Chefs (§7, §16 obs 8).
> 8. **Register pages with `DataViewMap<Page, Model, TData>`, not `ViewMap<Page, Model>`,** whenever the VM ctor takes a payload beyond `INavigator` / DI singletons. Plain `ViewMap` silently drops `data:` arguments (§17 C). Bit test-1.
> 9. **For routed-with-payload navigation use `NavigateRouteAsync(this, "Route", data: payload)`** — `NavigateDataAsync` has no `route:` parameter (§17 C). Bit test-6.
> 10. **Replace any `<ListView><ItemsWrapGrid/></ListView>` with `<ScrollViewer><muxc:ItemsRepeater><UniformGridLayout MinItemWidth="..."/></muxc:ItemsRepeater></ScrollViewer>`** at scaffold time. `ItemsWrapGrid` renders blank on Skia / WASM / iOS native (§17 D). Bit test-3, test-4, test-6.
>
> **Operational — bake into the test-runner harness:**
> 11. **Verify Skia-Desktop launch via `Get-Process -Name <App>` (PowerShell), not Bash exit code or `tasklist`.** `dotnet run` returns 0 within seconds even on healthy launches; empty stdout ≠ silent crash; `tasklist` from Bash is unreliable (§17 A). Bit every run.
> 12. **Pre-grep `Assets/` for actual filenames before referencing them in XAML** — wrong paths fail silently with blank renders, no diagnostic. The 4 ingredient-icon `Assets/Icons/avocado.png` 404s are this exact failure shape (§13).
> 13. **Apply the starter kit before opening Claude Code** so `uno-app` MCP server is present at session start. Tool schemas may be lazy — load via `ToolSearch select:mcp__uno-app__uno_app_get_screenshot,...` (§17 E, test-3 methodology note). Bit every run that wanted criterion-3 visual scoring.
> 14. **Delete scaffold placeholders (`MainPage` / `SecondPage` / `Entity.cs`) at scaffold time, then rewrite `RegisterRoutes` wholesale** — leaving them creates dead routing + stale references (§17 D, t1 #10).
> 15. **If using Toolkit `utu:TabBar` + `uen:Region.Attached` + `Region.Navigator="Visibility"` for tab content,** verify on Skia desktop that child regions render after `IsDefault: true` lands. **test-7 hit the exact upstream-canonical pattern and saw empty regions** — pivoted to flat routing under Shell (FP #3/#4). Either smoke-test the region pattern in iter-1 with the running app, or default to flat routing and accept the architectural drift from upstream until the Toolkit issue is documented (§16 obs 14, §17 A new entry).

---

## 4. Page-by-page matrix

Per-page status from each run's verification checklist. Cell legend: ✅ reached & data-bound · ⚠ partial / behind blocker · ❌ broken at runtime · `(—)` not exercised. The first column shows what's actually shipped upstream — single page, segmented, dialog, etc. Tests are scored against the SPEC's 20-page expansion.

| # | Page | Upstream (`Views/`) | test-1 | test-2 | test-3 | test-4 | test-5 | test-6 | test-7 (visual %) |
|--:|---|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| 1 | **Splash** | (none — Resizetizer auto-handled via `ExtendedSplashScreen` in `ShellControl.xaml`) | ⚠ pre-attach | ✅ | ✅ | ✅ | ⚠ SVG splash not rendering | ✅ | ✅ N-A |
| 2 | **Onboarding** | `WelcomePage.xaml` (FlipView frames, route `Welcome`) | ✅ | ✅ | ✅ | ✅ | ✅ ~70% (lockup ❌) | ✅ | ⚠ ~50% (hero stretches) |
| 3 | **Login** | `LoginPage.xaml` (route `Login`) | ✅ | ✅ | ✅ | ✅ | ✅ routed (not walked) | ✅ | ⚠ ~70% (wordmark SVG ❌) |
| 4 | **Register** | `RegistrationPage.xaml` (route `Register`) | ✅ | ✅ routed + bound, not screenshot-verified | ✅ | ✅ | ✅ routed (not walked) | ✅ | ✅ ~75% |
| 5 | **Home** | `HomePage.xaml` | ⚠ initial✅, re-entry❌ | ✅ | ✅ | ⚠ tap broken | ✅ routed + bound (Trending/Recent/Cats/Contributors), not walked | ✅ | ✅ ~78% |
| 6 | **Search** | `SearchPage.xaml` | ✅ | ✅ (33 results bound; empty state ready) | ✅ | ⚠ grid blank on Skia | ✅ routed + bound, not walked | ✅ | ✅ ~78% |
| 7 | **Filters** | `FiltersPage.xaml` (route `Filter`, modal `!Filter`) | ✅ | ✅ | ✅ | ✅ | ✅ routed (not walked) | ✅ | ✅ ~80% |
| 8 | **Recipe Detail** | `RecipeDetailsPage.xaml` (route `RecipeDetails`) | ❌ unreachable | ✅ (4 tabs swap; Save toggle; author tap) | ✅ | ⚠ unreachable | ✅ routed + bound (4 tabs), not walked | ✅ | ✅ ~75% (Pivot underline blue) |
| 9 | **Live Cooking** | `LiveCookingPage.xaml` (`MediaPlayerElement` + steps) | ❌ unreachable | ✅ Next/Prev pager + checkbox per ingredient (no MediaElement; UrlVideo string only) | ⚠ surrogate | ⚠ static rect | ✅ routed + bound (step pager), not walked | ✅ | ✅ ~78% (overlay surrogate) |
| 10 | **Live Cooking Finish** | **dialog** — `Dialogs/CompletedDialog.xaml` (not a page) | ❌ unreachable | ✅ routed + bound, not screenshot-verified | ✅ | ⚠ unreachable | ✅ routed + bound (recipe name), not walked | ✅ | ⚠ ~70% (success SVG ❌) |
| 11 | **Favorites — All Recipes** | `FavoriteRecipesPage.xaml` segmented tab 1 (single page handles both) | ✅ | ✅ (21 from `SavedRecipes.json`) | ✅ | ✅ | ✅ routed + bound, not walked | ✅ | ✅ ~78% |
| 12 | **Favorites — My Cookbooks** | `FavoriteRecipesPage.xaml` segmented tab 2 (same page) | ⚠ in-page tab, not route | ✅ (9 from `Cookbooks.json` ∩ `SavedCookbooks.json`; FAB→Create) | ✅ | ✅ | ✅ routed + bound, not walked | ✅ | ⚠ ~70% (placeholder icon) |
| 13 | **Cookbook Detail** | `CookbookDetailPage.xaml` (route `CookbookDetails`) | ❌ unreachable | ✅ ("Breakfast" → 6 recipes; Edit pencil → Update; FAB) | ✅ | ⚠ unreachable | ✅ routed + bound, not walked | ✅ | ✅ ~78% |
| 14 | **Create Cookbook** | `CreateUpdateCookbookPage.xaml` mode=Create (one page handles both) | ✅ | ✅ routed + bound (33 candidate recipes), not screenshot-verified | ✅ | ⚠ unreachable | ✅ routed + bound (recipes pickable), not walked | ✅ | ✅ ~75% |
| 15 | **Update Cookbook** | `CreateUpdateCookbookPage.xaml` mode=Update (same page) | ❌ unreachable | ✅ (name pre-filled "Breakfast"; Cancel → detail) | ✅ | ⚠ unreachable | ✅ routed + bound (pre-filled), not walked | ✅ | ✅ ~75% |
| 16 | **Profile (own)** | `ProfilePage.xaml` (route `Profile`, mode by VM parameter) | ✅ | ✅ (James Bondi 0/450/124; my-recipes empty correct) | ✅ | ⚠ unreachable | ✅ routed + bound (current user + grid), not walked | ✅ | ✅ ~78% |
| 17 | **Other Profile** | `ProfilePage.xaml` (same page, different VM data) | ❌ unreachable | ✅ (Troyan Smith) | ✅ | ⚠ unreachable | ✅ routed + bound, not walked | ✅ | ✅ ~78% |
| 18 | **Settings** | `SettingsPage.xaml` | ✅ | ⚠ TextBox values populate via async `LoadAsync`; NightMode toggle wired to `SystemThemeHelper` | ✅ | ✅ | ✅ routed + bound (current user fields + Night Mode → IThemeService), not walked | ✅ | ⚠ ~70% (toggle headers collapsed; theme race) |
| 19 | **Notifications** | `NotificationsPage.xaml` | ✅ | ✅ (5 notifications, segments All/Unread/Read) | ✅ | ✅ | ✅ routed + bound (filtered, empty state), not walked | ✅ | ✅ ~78% |
| 20 | **Near Me Map** | `MapPage.xaml` (route `Map`) — uses `Mapsui.Uno.WinUI` | ❌ unreachable | ⚠ 8 Pins from Users; SVG `location_pin/circle.svg` render transparent on Skia (FP #6 placeholder; not re-fixed) | ✅ placeholder | ⚠ unreachable | ⚠ placeholder map (no real map asset), not walked | ⚠ placeholder | ⚠ ~50% (pins stack at 0,0) |
|   | **Pages reaching ✅** | (canonical: every page reachable) | re-run: **20 / 20 routed+bound (code-level), walk blocked by MCP outage** (was 12/20 in original) | **17 / 20 walked** (3 routed-but-not-screenshot-verified: Register, LiveCookingFinish, CreateCookbook); **20 / 20 routed + bound** | **20 / 20** | **6 / 20 verified** | **1 / 20 walked (Onboarding only); 20 / 20 routed + bound** | re-run: **20 / 20 routed+bound (code-level), walk blocked by MCP outage** (original: 20/20 walked after 23 FPs) | **20 / 20** · avg **~76% post iter-3** (was ~73%) |

> **Page-list drift surfaced by the upstream check.** The canonical `Views/` ships **16 distinct pages + 2 dialogs (`CompletedDialog`, `GenericDialog`) + 1 flyout (`ResponsiveDrawerFlyout`)**, not 20 pages. The SPEC's 20-page count splits one segmented page (Favorites) into two, one dual-mode page (CreateUpdateCookbook) into two, one parameter-driven page (Profile) into two, and counts Splash + LiveCookingFinish (a dialog) as standalone surfaces. **Every test internalized the SPEC's count and shipped 19–21 distinct page types instead of the canonical 16.** Net effect on the verdicts: tests that score "20/20" are over-shipping vs the original architecture; the original is *more* compact, not less.

**Routes that drifted between SPEC and canonical (no run noticed):**

| SPEC / test runs | Upstream `RouteMap` |
|---|---|
| `Onboarding` | **`Welcome`** |
| `Register` | **`Registration`** |
| `FavoritesAll`, `FavoritesCookbooks` | **`FavoriteRecipes`** (single segmented) |
| `CookbookDetail` | **`CookbookDetails`** |
| `CreateCookbook`, `UpdateCookbook` | **`CreateCookbook`** + **`UpdateCookbook`** (both → `CreateUpdateCookbookModel`) |
| `Filters` | **`Filter`** |
| `NearMeMap` | **`Map`** |
| `LiveCookingFinish` | **`Completed`** (dialog, not page) |
| `Profile` (own) + `OtherProfile` | **`Profile`** (single, parameterized) |

---

## 5. Architecture / pattern matrix

| Aspect | Original Uno Chefs (verified upstream) | test-1 | test-2 | test-3 | test-4 | test-5 | test-6 | test-7 |
|---|---|---|---|---|---|---|---|---|
| **Pattern** | **MVUX** (records + `IFeed`/`IState`/`IListState`; `ChefsApiClient` via Kiota) | MVVM (CommunityToolkit.Mvvm) | MVVM (CommunityToolkit.Mvvm) | MVVM (CommunityToolkit.Mvvm) | MVVM (CommunityToolkit.Mvvm) | MVVM (CommunityToolkit.Mvvm) | MVVM (CommunityToolkit.Mvvm) | MVVM (CommunityToolkit.Mvvm) |
| **State management** | `IFeed<T>` / `IState<T>` / `IListState<T>` + CommunityToolkit `IMessenger` (`WeakReferenceMessenger`) | `[ObservableProperty]` | `[ObservableProperty]` on `ChefsViewModelBase` (made `partial` in FP #1) | `[ObservableProperty]` + `[NotifyPropertyChangedFor]` | `[ObservableProperty]` only | `[ObservableProperty]` + `RelayCommand` | `[ObservableProperty]` + `BoolToVisibilityConverter` | `[ObservableProperty]` + `RelayCommand` |
| **Navigation** | `Uno.Extensions.Navigation` w/ `uen:Region.Attached="True"`; route prefix `!` for modals (Profile / Notifications / Filters) | `RouteMap`+`ViewMap`+`DataViewMap`; card-tap via `utu:CommandExtensions.Command` (broken at runtime) | `RouteMap`+`ViewMap`+`DataViewMap` (FP #7 promoted 6 entries to `DataViewMap`); BottomNavBar uses `NavigateRouteAsync('-/Route')` clearing back stack on tab swap | `RouteMap`+`ViewMap`+`DataViewMap`; tap-driven, no route strings | `RouteMap`+`ViewMap`; inline-tab workaround swallows region context | `RouteMap`+`ViewMap`; routes registered via `IRouteRegistry`/`IViewRegistry` from base knowledge (uno-navigation skill skipped); `_navigator.NavigateBackAsync` on every drill-down (unverified) | `RouteMap`+`ViewMap`+`DataViewMap`; back via `ClearBackStack→Main` | Region-based **tried + abandoned** (FP #3/#4); pivoted to **flat routing** under Shell |
| **DI** | `Microsoft.Extensions.DependencyInjection` via Uno.Extensions Hosting | same | same | same | same | same | same | same |
| **Service interfaces** | **5 singletons:** `IRecipeService` · `IUserService` · `ICookbookService` · `INotificationService` · `IShareService` (over Kiota `ChefsApiClient` + `MockHttpMessageHandler` USE_MOCKS flag) | 1 monolithic `IChefsDataService` reading `ms-appx:///Assets/data/*.json` | 1 monolithic `IDataService` (`JsonDataService` reading `ms-appx:///Assets/Data/*.json` via `StorageFile`) | 1 monolithic `IChefsService` reading `Assets/data/*.json` w/ filesystem fallback | 1 monolithic `IChefsDataService` w/ `FlexibleTimeSpanConverter` | 1 monolithic `IChefsDataService` (singleton) + `IThemeService` for runtime theme toggle | 1 monolithic `IChefService` in-memory store + `IAppThemeService` | 1 monolithic `IChefsDataService` + `IThemeToggleService` |
| **Hosting setup calls** | `UseToolkitNavigation()` · `UseAuthentication()` · `UseHttp()`+Kiota · `UseLogging()` · `UseConfiguration()` · `UseLocalization()` · `UseSerialization()` · `UseNavigation()` · `UseEnvironment(Development)` | Navigation + Logging + Configuration + Serialization | Configuration + HttpKiota (declared, no calls) + Serialization + Navigation | same | same + ToolkitNavigation | scaffold defaults | same | `UseLocalization` + `UseHttp` + `UseConfiguration` retained from scaffold |
| **Auth** | `UseAuthentication()` w/ custom auth handler (simulated `ProcessCredentials` returning fake tokens) | `_navigator.NavigateBackAsync` no-op | demo bypass | demo bypass match-by-email | demo bypass match-by-email | demo bypass | demo bypass match-by-email + first-user fallback | demo bypass |
| **Theming** | `UnoFeatures` `ThemeService` + `IAppThemeService`; `RequestedTheme` propagation | `RequestedTheme` flip | `SystemThemeHelper.SetApplicationTheme(bool)` (FP #3 — `Uno.Toolkit.UI.IThemeService` not in 6.5.x; CS0618 obsolete-warning fallback accepted) | `App.SetTheme(bool)` propagation app-wide | `NightModeChanged` event (UI flip not propagated) | Settings → `IThemeService.Toggle()` (wired but not visually validated this run) | `IAppThemeService.RegisterRoot` propagation | `IThemeToggleService.Attach` — **service-init race** vs `App.Services` (FP #13 deferred) |
| **Localization** | `UseLocalization()` + 4 locales: en / es / fr / pt-BR (`Strings/{loc}/Resources.resw`) | none | none | none | none | none | none | scaffold-default `UseLocalization` retained, no `Resources.resw` shipped |
| **Persistence** | Kiota API calls (mocked); no offline cache | None — back-nav stub | In-memory mutate via `ToggleFavoriteRecipe` / `ToggleSavedCookbook` (HashSets); JSON files read-only | In-memory mutate; survives session | In-memory mutate | Stub save commands — `NavigateBackAsync` only, no write to data layer | In-memory mutate; Remember-me / recent-searches not platform-persisted | In-memory mutate |
| **Extra DI registrations** | `IMessenger` (WeakReferenceMessenger), `MockHttpMessageHandler`, `Flyout, ResponsiveDrawerFlyout`, `AddKiotaClient<ChefsApiClient>` | none | none beyond `IDataService` | `IMessenger` not used | none | `IThemeService` | `IAppThemeService` | `IThemeToggleService` |

---

## 6. Packages & UnoFeatures matrix

| UnoFeature / package | Original | test-1 | test-2 | test-3 | test-4 | test-5 | test-6 | test-7 |
|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| `Material` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `Hosting` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `Toolkit` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `Logging` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **`MVUX`** | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ (gen still ran → KE0001) | ❌ |
| `Configuration` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `HttpKiota` | ✅ | ❌ | ⚠ declared, fixtures read via StorageFile (no Kiota calls fire) | ❌ | ❌ | ❌ | ⚠ declared, no calls fire | ⚠ scaffold default, no Kiota client |
| `Serialization` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **`Localization`** | ✅ (4 locales) | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ⚠ `UseLocalization` retained, no `Resources.resw` |
| `Navigation` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **`MediaElement`** | ✅ | ❌ surrogate hero | ❌ image hero + play-icon overlay + ProgressBar (UrlVideo string only; not wired) | ❌ surrogate (image + ProgressBar) | ⚠ added, not bound | ❌ static overlay strip w/ Segoe MDL2 glyphs (no MediaPlayerElement) | ✅ wired | ❌ surrogate (image + ProgressBar overlay) |
| `Skia` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **`ThemeService`** | ✅ | ⚠ via `RequestedTheme` only | ⚠ via `SystemThemeHelper.SetApplicationTheme` (FP #3 — `Uno.Toolkit.UI.IThemeService` not in 6.5.x) | ⚠ via `App.SetTheme` shim | ⚠ wired but not propagated | ⚠ Settings → `IThemeService.Toggle()` (wired, dark not visually verified) | ✅ via `IAppThemeService` | ⚠ `IThemeToggleService` w/ service-init race |
| **`Authentication`** | ✅ (simulated) | ❌ stub | ❌ demo bypass | ❌ demo bypass | ❌ demo bypass | ❌ demo bypass | ❌ demo bypass + fallback | ❌ demo bypass |
| `SkiaRenderer` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **`Svg`** (not in original; needed for SVG empty states on Skia) | ⚠ asset only | ⚠ raster fallback | ⚠ added; brand+empty-state SVGs work; **`Maps/location_pin.svg` + `location_circle.svg` render transparent on Skia** (NearMeMap markers) | ✅ native Skia SVG | ⚠ added but partial | ❌ **flag added (FP #3) but `SvgImageSource` warning persists** — needs explicit `Uno.WinUI.Svg` `<PackageReference>`; 8 SVGs (`chefsappsignature_*`, `empty_*`, `success_*`, `splash_screen.svg`) all not rendering | ❌ emoji fallback | ⚠ added; 6 SVGs miss (wordmark + success_*; viewBox issue) |

> Bold rows = features test runs systematically dropped vs the canonical app. None of the four tests shipped MVUX or Localization; only test-6 wired ThemeService + MediaElement properly.

---

## 7. Controls & component-library matrix

Verified against upstream `MainPage.xaml`, `RecipeDetailsPage.xaml`, `HomePage.xaml`, `LiveCookingPage.xaml`, `ShellControl.xaml`. ✅ used as designed · ⚠ partial / approximate · ❌ replaced or missing.

| Control / Component | Verified upstream usage | test-1 | test-2 | test-3 | test-4 | test-5 | test-6 | test-7 |
|---|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| **`utu:NavigationBar`** (`ChefsNavigationBarStyle` / `ChefsModalNavigationBarStyle`) | HomePage: `Style="{StaticResource ChefsNavigationBarStyle}"` + `<.PrimaryCommands>` 2 AppBarButtons (Profile, Notifications) | ⚠ hand-rolled | ⚠ utu:NavigationBar w/ inverted-surface app-bar (not yet wired into NavigationBar styles for inverted surfaces) | ✅ utu:NavigationBar | ⚠ Toolkit default (pill drift) | ⚠ `ChefAppBarBrush` near-black brush applied on 10 pages; not visually verified | ⚠ auto-back broken; explicit MainCommand override on 10 pages | ⚠ hand-rolled inline app-bar |
| **`utu:TabBar` Bottom + Vertical pair** (`BottomTabBarStyle` + `VerticalTabBarStyle`) | MainPage ships BOTH; visibility swapped via `{utu:Responsive Normal=Visible, Wide=Collapsed}` and inverse | ⚠ hand-rolled `BottomNavBar` UserControl | ⚠ utu:TabBar Bottom only (`ColoredTopTabBarStyle` verified via uno-toolkit), no Vertical | ⚠ shared `BottomNavBar` UserControl, no Vertical | ⚠ utu:TabBar Bottom only (pill blue) | ⚠ Bottom only — custom `NavTab` UserControl w/ `IsActive` cream-pill | ✅ Bottom↔Vertical pair shipped (FP #22) | ⚠ Bottom only, hand-rolled inline (TabBar+region tried, abandoned FP #3/#4) |
| **`utu:TabBar` `TopTabBarStyle`** for in-page tabs (e.g. Recipe Detail) | RecipeDetailsPage: 4 `<utu:TabBarItem>` w/ `uen:Region.Name="IngredientsTab/StepsTab/ReviewsTab/NutritionTab"` + parent Grid `uen:Region.Navigator="Visibility"` | ⚠ Buttons + IsActive bool | ⚠ `<Pivot>` + `<PivotItem>` per tab | ⚠ Buttons + IsActive bool | ⚠ deprecated `Pivot` + `PivotItem` (Win-XAML compile fail) | ⚠ Tab buttons + `IndexToVisibilityConverter` switching Grids | ⚠ Buttons + Visibility | ⚠ deprecated `Pivot` + `PivotItem` (underline blue, not pink) |
| **`utu:TabBarItem`** + `utu:TabBarItemExtensions.OnClickBehaviors="BackNavigation"` | MainPage 3× per TabBar (Home/Search/Favorites) | ⚠ Buttons | ⚠ utu:TabBarItem in `BottomNavBar` | ⚠ Buttons w/ ActiveTab DP | ✅ TabBarItem | ⚠ Buttons in Border | ⚠ Buttons in Border | ⚠ Buttons w/ pink active |
| **`utu:AutoLayout`** | RecipeDetailsPage 30+ instances; pervasive across MainPage / HomePage; `Spacing` / `Orientation` / `PrimaryAxisAlignment` / `CounterAxisAlignment` / `Justify` / `IsIndependentLayout` | ❌ Grid + StackPanel | ❌ Grid + StackPanel | ❌ Grid + StackPanel | ❌ Grid + StackPanel | ❌ Grid + StackPanel | ❌ Grid + StackPanel | ⚠ used on Filters only (per skill-usage), Grid+StackPanel elsewhere |
| **`uer:FeedView`** | HomePage (Trending / Categories / Recent / Contributors — 4× `uer:FeedView` → `ScrollViewer` → `muxc:ItemsRepeater` w/ horizontal `StackLayout Spacing="8"`); RecipeDetailsPage (Ingredients + Reviews w/ `NoneTemplate="{StaticResource EmptyTemplate}"`) | ❌ raw `ItemsControl` w/ HasItems flag | ❌ ObservableCollection + ItemsRepeater | ❌ `ItemsControl`+empty branch | ❌ `ItemsControl`+empty branch | ❌ `ItemsRepeater`+`CountToVisibilityConverter` | ❌ `ItemsRepeater`+empty branch | ❌ `GridView`+`ItemsRepeater`+empty branch (no FeedView) |
| **`utu:CardContentControl`** | HomePage recipe / cookbook / contributor cards w/ Style + Width + CornerRadius | ❌ Border-wrapped (caused tap bug) | ❌ Card w/ Click handler → `OpenRecipeCommand` (cards-with-tap) | ❌ Border w/ corner radius | ❌ Button w/ rounded bg | ❌ Border in ItemsRepeater w/ no Tapped handler (cards-without-tap) | ❌ Button + manual styling | ❌ Border + Button-wrap (FP #6–#12 fix) |
| **`Chip` / `MaterialChipStyle`** + `ChipGroup` | Filters page selection (per Design Brief §4.4) | ⚠ custom `ChipButton` | ⚠ ToggleButton w/ chip-style | ✅ custom `ChipButton` UserControl | ⚠ ToggleButton w/ 1.5px PrimaryBrush border | ⚠ ToggleButton (no MaterialChipStyle) | ⚠ raw ToggleButton | ⚠ ToggleButton |
| **`muxc:ItemsRepeater`** w/ `StackLayout` (horizontal carousels) / `UniformGridLayout` (grids) | HomePage 4× horizontal `StackLayout`; LiveCookingPage ingredient checklist; RecipeDetailsPage feeds | ⚠ `Padding` typo (FP fix) | ✅ `UniformGridLayout MinItemWidth=160` (after FP #2 replaced `utu:FlowLayout`) | ✅ `UniformGridLayout MinItemWidth=220` | ⚠ blank on Skia for ItemsWrapGrid | ✅ `UniformGridLayout` (FP #2 replaced `utu:UniformGridPanel`) | ✅ `UniformGridLayout` post-FP #5 | ✅ ItemsRepeater + GridView w/ IsItemClickEnabled |
| **`FlipView`** + `utu:FlipViewExtensions.Previous` / `.Next` markup | LiveCookingPage step pager: `<FlipView SelectedIndex="{Binding Steps.CurrentIndex}">` + `<Button utu:FlipViewExtensions.Previous="{Binding ElementName=StepsFlipView}">` | ✅ onboarding only | ✅ onboarding only | ✅ onboarding only | ✅ onboarding only | ✅ onboarding (3 frames in VM) | ✅ onboarding (Live Cooking uses pager dots, not FlipView) | ✅ onboarding + Live Cooking pager |
| **`muxc:PipsPager`** | LiveCookingPage `<muxc:PipsPager x:Name="pipsPager">` linked via `utu:SelectorExtensions.PipsPager` | ✅ onboarding | ✅ onboarding | ✅ onboarding (FP #1 namespace fix) | ✅ onboarding | ✅ onboarding | ✅ onboarding | ✅ onboarding + Live Cooking |
| **`MediaPlayerElement`** | LiveCookingPage: `<MediaPlayerElement AreTransportControlsEnabled="True" AutoPlay="True" Source="ms-appx:///Assets/Videos/CookingVideo.mp4">` + `<MediaTransportControls IsCompact="True">` | ❌ semi-transparent overlay panel | ❌ image hero + play-icon overlay + ProgressBar (no MediaElement) | ❌ image + ProgressBar surrogate | ❌ static `#222` rect + 5 glyphs | ❌ static overlay strip w/ play/scrubber/volume/PiP/cast/fullscreen Segoe glyphs (CookingVideo.mp4 bundled, not wired) | ✅ wired w/ scrubber + IsCompact | ❌ image + ▶ overlay + ProgressBar scrubber + 01:12/03:00 time (3 of 6 controls) |
| **`muxc:RatingControl`** | LiveCookingPage finish rating; Recipe Details rating display | ⚠ static glyphs | ⚠ static stars (Live Cooking Finish not screenshot-verified) | ✅ tappable star icons | ✅ tappable star icons | ⚠ 5-star renders, no command wired | ✅ 5-star rating buttons | ✅ 5-star tappable |
| **`PersonPicture`** | user avatars 32–96 px (per Design Brief §4.4) | ⚠ Border+Image | ⚠ Border+Image | ⚠ Border+Image | ⚠ Ellipse+Image | ⚠ Ellipse+Image | ⚠ Ellipse+Image | ⚠ Ellipse+Image |
| **`Divider`** (`DividerStyle`) | section separator | ⚠ Border 1px | ⚠ Border 1px | ⚠ Border 1px | ⚠ Border 1px | ⚠ Border 1px | ⚠ Border 1px | ⚠ Border 1px |
| **`ProgressRing`** | ShellControl `LoadingContentTemplate`; FeedView `ProgressTemplate` | ❌ none | ❌ none | ❌ none | ❌ none | ❌ none | ❌ none | ❌ none |
| **`AutoSuggestBox`** | SearchPage (per Design Brief §4.3) | ⚠ TextBox | ⚠ TextBox | ⚠ TextBox | ⚠ pill SearchBar TextBox | ⚠ TextBox | ✅ AutoSuggestBox (FP) | ⚠ pill SearchBox-styled TextBox |
| **`PathIcon`** + 28 `Icon_*` Data resources | LiveCookingPage `<PathIcon Data="{StaticResource Icon_Check_Circle}"/>` for ingredient checklist; RecipeDetails Carrot FAB icon via `<ut:ControlExtensions.Icon>` | ⚠ FontIcon fallbacks | ⚠ FontIcon / SymbolIcon | ⚠ Segoe Fluent / SymbolIcon | ⚠ FontIcon | ⚠ FontIcon / Symbol | ⚠ Symbol/Emoji glyphs | ⚠ FontIcon / SymbolIcon |
| **`BitmapIcon`** | empty-state illustrations w/ theme-aware URIs | ❌ FontIcon-in-circle | ⚠ Image w/ ms-appx empty-state SVGs (working) | ✅ SVG via Image (Skia native) | ⚠ raster fallback | ❌ Image w/ empty-state SVG URIs — illustrations don't render (Svg flag insufficient; needs Uno.WinUI.Svg PackageReference) | ❌ emoji glyphs | ⚠ SVG via Image — 6 outliers (wordmark + success_*) |
| **`FontIcon` + FontAwesome** (`FontAwesomeSolidFontIconStyle`) | RecipeDetails FAB Carrot glyph; Apple / Google brand glyphs | ⚠ Segoe MDL2 | ⚠ Segoe MDL2 | ⚠ Segoe MDL2 | ⚠ Material font | ⚠ Segoe MDL2 | ⚠ Material font | ⚠ Segoe / Material |
| **`Button` w/ `ChefsFabButtonStyle`** + `ut:ControlExtensions.Icon` | RecipeDetails "Start Cooking!" CTA: `Style="{StaticResource ChefsFabButtonStyle}"` + nested `<ut:ControlExtensions.Icon><FontIcon Glyph="{StaticResource Icon_Carrot}"/>` | ⚠ Button + Style | ⚠ Button + Style | ⚠ Button + Style | ⚠ Button + Style | ⚠ Button + Style | ⚠ Button + Style | ⚠ Button + Style |
| **`Flyout, ResponsiveDrawerFlyout`** (DI-registered, transient) | tablet/desktop drawer; resolved via DI lookup | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **`{utu:Responsive}` markup ext** | MainPage Bottom↔Vertical TabBar visibility swap; RecipeDetails padding/spacing scaling | ⚠ VSM on 4/20 pages | ❌ phone-only (Figma rate-limit blocked tablet variants) | ❌ phone-only | ❌ phone-only | ❌ phone-only (no AdaptiveTrigger added) | ✅ MainPage swap + RecipeDetail 2-pane (FP #22/23) | ❌ phone-only |
| **`utu:ExtendedSplashScreen`** w/ `LoadingContentTemplate` | ShellControl root: `x:Name="Splash"` + `<DataTemplate>` containing `ProgressRing` | ✅ | ⚠ default scaffold; second-layer brand pictogram not added | ✅ | ✅ | ✅ custom loading template | ✅ | ✅ |
| **`utu:StatusBar`** | HomePage / RecipeDetailsPage / etc. — Background+Foreground theming | ❌ | ⚠ utu:SafeArea applied; StatusBar not | ❌ | ❌ | ❌ | ❌ | ❌ |
| **`AppBarButton` in NavigationBar `PrimaryCommands`** | HomePage NavBar 2× (Profile, Notifications icons); RecipeDetails NavBar 2× (Share, Favorite) | ⚠ hand-rolled buttons | ⚠ hand-rolled buttons | ⚠ hand-rolled buttons | ⚠ hand-rolled buttons | ⚠ hand-rolled buttons | ⚠ hand-rolled buttons | ⚠ hand-rolled buttons |
| **`uen:Region.Attached` + `Region.Name` + `Region.Navigator="Visibility"`** | MainPage tab regions (`"Home"`, `"-/Search"`, `"FavoriteRecipes"`); RecipeDetails tab regions (`"IngredientsTab"`, `"StepsTab"`, `"ReviewsTab"`, `"NutritionTab"`) | ❌ | ❌ flat tab pattern via `NavigateRouteAsync('-/Route')` | ❌ | ❌ tried, blocker #2 reverted to inline-tab | ❌ flat tab routing | ❌ flat tab pattern, no regions | ❌ tried, abandoned FP #3/#4 (empty regions on `IsDefault: true`) |
| **`uen:Navigation.Request` / `uen:Navigation.Data` markup ext** | HomePage card-tap navigation (per Design Brief §6.1, used throughout) | ❌ Command binding | ❌ Click handler → `OpenRecipeCommand` (works at runtime) | ❌ Command binding | ❌ Command binding (caught) | ❌ no Tapped handler wired (anti-pattern) | ❌ Command binding | ❌ Command binding + IsItemClickEnabled |

---

## 8. Design tokens — color matrix

Original Uno Chefs colors verified directly from upstream `Chefs/Styles/ColorPaletteOverride.xaml` (36 tokens; the design brief listed only 14). Match column uses ✅ exact / ⚠ near (within ~10pt deltaE) / ❌ default.

| Token | Original Light | Original Dark | test-1 (L/D) | test-2 (L/D) | test-3 (L/D) | test-4 (L/D) | test-5 (L/D) | test-6 (L/D) | test-7 (L/D) |
|---|---|---|---|---|---|---|---|---|---|
| `StatusBarForegroundColor` | `#000000` | `#FFFFFF` | default | default | default | default | default | default | default |
| `SurfaceColor` | `#FFFFFF` | `#0F1012` | ✅ / `#1A1A1A` ⚠ | ✅ / `#101112` ✅ (Figma) | ✅ / ⚠ | ✅ / ⚠ | ✅ / ⚠ | MD3 ❌ | ✅ / ⚠ |
| `OnSurfaceColor` | `#1C1B1F` | `#EBECF0` | ✅ / ✅ | `#1C1B1F` / `#EBEDF0` ✅ | ✅ / ✅ | ✅ / ✅ | ✅ / ⚠ | MD3 ❌ | ✅ / ✅ |
| **`PrimaryColor`** | **`#ED3F64`** | **`#FFA3B6`** | `#E8455C` / `#F4A5B0` ⚠ | **`#E8455C` / `#FFA3B7`** ✅ (Figma — closest dark match of any run) | `#FF1F5A` ⚠ | `#E91E63` / `#F8BBD0` ⚠ | `#E8455C` / `#FFB1BC` ⚠ | **MD3 purple** ❌ | `#FF1F5A` / `#FF6B8A` ⚠ (DESIGN.md value) |
| `OnSurfaceVariantColor` | `#5A3E3E` | `#C4C8CF` | generic muted ⚠ | `#5A3F3F` / `#C5C9CF` ✅ (Figma) | generic muted ⚠ | generic muted ⚠ | generic muted ⚠ | MD3 ❌ | generic muted ⚠ |
| `OnPrimaryColor` | `#FFFFFF` | `#620015` | `#FFFFFF` ✅ | `#FFFFFF` / `#630015` ✅ (Figma) | `#FFFFFF` ✅ | `#FFFFFF` / `#4A0E20` ⚠ | `#FFFFFF` ✅ | MD3 ❌ | `#FFFFFF` ✅ |
| `OnSecondaryContainerColor` | `#1F182B` | `#FFFCEE` | not styled | `#FFFDEE` dark ✅ (Figma) | not styled | not styled | not styled | MD3 ❌ | not styled |
| `SecondaryContainerColor` | `#EEECDE` | `#494636` | `#EAE3D6` / `#4A463C` ⚠ | `#EAE3D6` / `#494737` ✅ (Figma) | `#EAE3D6` / `#3A3328` ⚠ | `#EDE4D3` / `#F5EFE3` ⚠ | `#EAE3D6` / `#4F4940` ⚠ | MD3 ❌ | not styled |
| `PrimaryInverseColor` | `#F48CA2` | `#9C0021` | not styled | not styled | not styled | not styled | not styled | MD3 ❌ | not styled |
| `PrimaryContainerColor` | `#F48CA2` | `#B72746` | not styled | not styled | not styled | not styled | not styled | MD3 ❌ | not styled |
| `OnPrimaryContainerColor` | `#FFFFFF` | `#FFE7EC` | not styled | not styled | not styled | not styled | not styled | MD3 ❌ | not styled |
| `PrimaryVariantLightColor` | `#F48CA2` | `#FFBFCC` | not styled | not styled | not styled | not styled | not styled | MD3 ❌ | not styled |
| `PrimaryVariantDarkColor` | `#910030` | `#AD2340` | not styled | not styled | not styled | not styled | not styled | MD3 ❌ | not styled |
| **`SecondaryColor`** | **`#C5C2A8`** | **`#C5C2A8`** *(same)* | `#EAE3D6` ⚠ | `#EAE3D6` ⚠ | `#EAE3D6` ⚠ | `#EDE4D3` ⚠ | `#EAE3D6` ⚠ | MD3 ❌ | `#EAE3D6` ⚠ |
| `SecondaryVariantDarkColor` | `#9C9986` | (none) | not styled | not styled | not styled | not styled | not styled | MD3 ❌ | not styled |
| `SecondaryVariantLightColor` | `#E8E6D6` | (none) | not styled | not styled | not styled | not styled | not styled | MD3 ❌ | not styled |
| `OnSecondaryColor` | `#1F182B` | `#2B291E` | not styled | not styled | not styled | not styled | not styled | MD3 ❌ | not styled |
| **`BackgroundColor`** | **`#F5F8FD`** | **`#232528`** | white default ⚠ | `#F5F8FD` light ✅ / `#242629` ✅ (Figma) | white default ⚠ | white default ⚠ | white default ⚠ | MD3 ❌ | white default ⚠ |
| `OnBackgroundColor` | `#1C1B1F` | `#F5F8FD` | ✅ / ⚠ | ✅ / ✅ | ✅ / ⚠ | ✅ / ⚠ | ✅ / ⚠ | MD3 ❌ | ✅ / ⚠ |
| **`SurfaceInverseColor`** | **`#313033`** | **`#E3E4E7`** | `#2D2D2D` ⚠ (light only) | `#2D2D2D` / `#E3E5E8` ✅ (Figma — both themes) | `#2D2D2D` / `#FFFFFF` ⚠ | `#2B2B2E` / `#ECECEC` ✅ | `#2D2D2D` / `#1A1A1A` ⚠ | MD3 ❌ | `#2D2D2D` ⚠ (`AppBarFillBrush` resource) |
| `SurfaceTintColor` | `#EDF0F6` | `#444A55` | not styled | `#444A55` dark ✅ (Figma) | not styled | not styled | not styled | MD3 ❌ | not styled |
| `OnSurfaceInverseColor` | `#F4EFF4` | `#191A1C` | not styled | not styled | not styled | not styled | not styled | MD3 ❌ | not styled |
| `SurfaceVariantColor` | `#FADDE3` | `#494B4F` | not styled | not styled | not styled | not styled | not styled | MD3 ❌ | not styled |
| `OutlineColor` | `#79747E` | `#92969C` | MD3 default | MD3 default | MD3 default | MD3 default | MD3 default | MD3 default | MD3 default |
| `OutlineVariantColor` | `#C9C5D0` | `#53565A` | not styled | `#DAD2C5` light / `#53565A` dark ⚠ (Figma) | not styled | not styled | not styled | MD3 default | not styled |
| `ErrorColor` | `#B3261E` | `#FFB4AB` | MD3 default | MD3 default | MD3 default | MD3 default (`#D32F2F` if added) | MD3 default | MD3 default | MD3 default |
| `OnErrorColor` | `#FFFFFF` | `#690005` | MD3 default | MD3 default | MD3 default | MD3 default | MD3 default | MD3 default | MD3 default |
| `ErrorContainerColor` | `#F9DEDC` | `#93000A` | not styled | not styled | not styled | not styled | not styled | MD3 default | not styled |
| `OnErrorContainerColor` | `#410E0B` | `#FFDAD6` | not styled | not styled | not styled | not styled | not styled | MD3 default | not styled |
| `TertiaryColor` | `#F7F5DD` | `#F7F5DD` *(same)* | not used | not used | not used | not used | not used | MD3 default | not used |
| `OnTertiaryColor` | `#000000` | `#000000` | not used | not used | not used | not used | not used | MD3 default | not used |
| `TertiaryContainerColor` | `#F5F3CB` | `#F5F3CB` *(same)* | not used | not used | not used | not used | not used | MD3 default | not used |
| `OnTertiaryContainerColor` | `#001D36` | `#2A2800` | not used | not used | not used | not used | not used | MD3 default | not used |
| `NutritionTrackBackgroundColor` | `#1C1B1F14` | `#242629` | not implemented | ⚠ generic | ⚠ generic | ⚠ generic | not implemented | not implemented | not implemented |
| **`NutritionProteinValColor`** | **`#159BFF`** | same | not implemented | not separately styled | PrimaryBrush ⚠ | `#159BFF`-ish blue ✅ | not separately styled | not separately styled | not separately styled |
| **`NutritionCarbsValColor`** | **`#7A67F8`** | same | not implemented | not separately styled | PrimaryBrush ⚠ | purple ✅ | not separately styled | not separately styled | not separately styled |
| **`NutritionFatValColor`** | **`#F85977`** | same | not implemented | not separately styled | PrimaryBrush ⚠ | pink ✅ | not separately styled | not separately styled | not separately styled |
| `BrandBlackColor` | `#242424` | same | n/a | n/a | n/a | `#222` ✅ | n/a | not used | not used |
| **Token-match score (light + dark, ✅/⚠)** | 36 / 36 | **~6 ✅ / ~5 ⚠ of 36** | **~12 ✅ / ~3 ⚠ of 36** (Figma's MD3 token system carried more direct matches than other runs — high-fidelity ceiling for this methodology) | **~5 ✅ / ~5 ⚠ of 36** | **~9 ✅ / ~5 ⚠ of 36** | **~5 ✅ / ~5 ⚠ of 36** | **0 ✅ / 0 ⚠ of 36** | **~5 ✅ / ~5 ⚠ of 36** |

**Bold rows = the high-signal brand tokens.** No test re-styled the secondary, tertiary, or container ramps — every test took the Uno.Material defaults for ~25 of 36 tokens. test-6 (blind PRD) shipped 0 brand tokens, exactly as the negative-control hypothesis predicts. The dark-theme `PrimaryColor` value `#FFA3B6` (a pastel pink with deep-maroon text per `OnPrimaryColor: #620015`) was approximated by test-1 (`#F4A5B0`) and test-4 (`#F8BBD0`) but missed by test-3 (which reused the same `#FF1F5A` for both themes).

---

## 9. Typography matrix

Original scale from `MaterialFontsOverride.xaml` + Material Toolkit defaults.

| Style | Original | test-1 | test-2 | test-3 | test-4 | test-5 | test-6 | test-7 |
|---|---|---|---|---|---|---|---|---|
| `TitleLarge` | 22px SemiBold | H1 22 bold ✅ | `ChefsTitleLarge` 22 SemiBold ✅ (from Figma `UNO Semantic`) | H1 24 Bold ⚠ | Display 26 / H1 22 ✅ | `ChefH1` ✅ | MD3 `DisplaySmall` ⚠ | 24pt Bold (page titles, "Hurray!") ⚠ |
| `TitleMedium` | 18px Normal | H2 18 semibold ⚠ | `ChefsTitleMedium` 16 Medium ⚠ (Figma drift: 16 not 18) | H2 18 Bold ⚠ | H2 20 ⚠ | `ChefH2` ⚠ | MD3 `HeadlineSmall` ⚠ | 18pt Bold (section headers) ⚠ |
| `TitleSmall` | 14px Medium | not distinct | `ChefsTitleSmall` 14 Medium ✅ | not distinct | H4 14 ⚠ | `ChefSectionTitle` ⚠ | MD3 `TitleMedium` ⚠ | not distinct |
| `BodyLarge` | 16px Normal | not distinct | `ChefsBodyLarge` 16 Medium ⚠ (Figma weight drift) | `BodyLargeStyle` 16 SemiBold ⚠ | Body1 16 ✅ | `ChefBody` ⚠ | MD3 `BodyLarge` ✅ | 16pt SemiBold (card titles, primary buttons) ⚠ |
| `BodySmall` | 12px Normal | Caption 12 muted ⚠ | `ChefsBodySmall` 12 Medium ⚠ | `CaptionStyle` 12 Reg muted ✅ | Caption 12 ✅ | `ChefMuted` ⚠ | MD3 `BodyMedium` ⚠ | 12-13pt (captions/secondary) ✅ |
| `LabelLarge` | 14px Medium | not distinct | not distinct (Figma had `LabelLarge 14 Med 0.1ls` but mapped to Title Small) | not distinct | not distinct | `ChefViewAllLink` ⚠ | MD3 `LabelLarge` ✅ | 14pt body ⚠ |
| `LabelMedium` | 12px Medium | not distinct | not distinct | not distinct | not distinct | not distinct | MD3 `LabelMedium` ✅ | not distinct |
| `LabelSmall` | 10px Medium | not distinct | not distinct | not distinct | not distinct | not distinct | not distinct | not distinct |
| `CaptionMedium` / Small / Large | 10–14px | merged into Caption | `CaptionMedium` 12 Med 0.4ls (Figma) — merged into BodySmall | merged into BodySecondary | not distinct | `ChefCaption` ⚠ | merged into Body* | merged into 12-13pt secondary |
| **Tier count shipped** | ~11 | 5 | 5 (custom `Chefs*` keys) | 5 | **9** (most granular) | 8 (`Chef*` custom keys) | 8 (MD3 defaults, not Chefs scale) | 5 |
| **Font family** | Material default | Roboto | Roboto (Figma `UNO Semantic`) | Roboto | Roboto | Roboto | Roboto | Roboto (Material default) |

---

## 10. Inputs allowed / forbidden per run

| Input | Original | test-1 | test-2 | test-3 | test-4 | test-5 | test-6 | test-7 |
|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| `Chefs-screenshots/` PNGs | n/a | ✅ sole visual input | ❌ forbidden | ❌ forbidden | ❌ forbidden | ✅ intended primary; was the actual fall-back when Figma MCP died | ❌ forbidden | ✅ primary visual ground truth (also drives diff loop) |
| `reference/figma-url.txt` (~30 frames) + Figma MCP | n/a | ❌ | ✅ **sole visual input** (rate-limited after 4 frames) | ❌ | ❌ | ✅ co-input (rate-limited on first call → 0 frames) | ❌ | ❌ |
| `reference/DESIGN.md` (Stitch) | n/a | ❌ | ❌ forbidden | ✅ sole visual input | ❌ | ❌ forbidden | ❌ forbidden | ✅ token / typography callouts |
| `reference/visual-skill-output/DESIGN.md` (custom skill) | n/a | ❌ | ❌ forbidden | ❌ | ✅ sole visual input | ❌ forbidden | ❌ forbidden | ✅ supplementary tokens |
| `reference/PRD.md` | n/a | ❌ | ❌ forbidden | ❌ | ❌ | ❌ forbidden | ✅ sole spec input | ❌ forbidden |
| `reference/API-CONTRACT.md` | n/a | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `reference/data/*.json` (7 fixture files) | n/a | ✅ | ✅ (copied to `Assets/Data/`) | ✅ | ✅ | ✅ (copied to `Assets/Data/`) | ✅ | ✅ (copied to `Assets/Data/`) |
| `reference/assets/` (94 bundled) | n/a | ✅ | ✅ (Welcome filenames had to be remapped — FP #5) | ✅ | ✅ | ✅ (8 SVGs not rendering — FP #3 unresolved) | ✅ | ✅ (88 of 94 resolved at runtime; 6 SVG outliers) |
| Remote Uno Chefs source | n/a | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `reference/forbidden/Chefs-*-Brief.md` | (these docs) | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |

---

## 11. Skill-usage discipline

| # | SKILLS-PLAN declared | rules-init packs | Implementation skills | Verification skills | Compliance |
|---|:-:|:-:|---|---|:-:|
| **1 (re-run)** | ✅ | ✅ both | ✅ **7 actual `Skill` tool calls** (`uno-platform-agent`, `winui-xaml`, `uno-navigation`, `uno-material`, `userinterface-wiki-uno`, `uno-toolkit`, `uno-extensions-services` — 1 each) | ✅ `uno-app-ui-testing` (1); `uno-app-test-assertions` not invoked due to MCP outage (honest planned-non-invocation, not fabricated) | ✅ **DISCIPLINE-AUDIT clean** — 8 actual Skill calls = 8 SKILL-USE lines; no failure mode #2 |
| **2** | ✅ | ✅ both | ❌ **failure mode #1** — 9 SKILL-USE lines all logged retroactively after user explicitly asked whether skills had been invoked; rules-init packs were the only real session-time invocations | retroactive | 🔁 must re-run |
| **3** | ✅ | ✅ both (`agent_rules_init` + `usage_rules_init`) | ⚠ planned but only knowledge-internalized; no invocations of `uno-platform-agent`, `uno-navigation`, `uno-toolkit`, `uno-material`, `uno-extensions-services`, `winui-xaml`, `userinterface-wiki-uno` | ✅ `uno-app-ui-testing` (1) + `uno-app-test-assertions` (1) | ⚠ partial — rules-init + verification yes, impl-skill invocation no |
| **4** | ✅ | ✅ | ✅ **7 implementation skills** (`uno-platform-agent`, `winui-xaml`, `uno-material`, `userinterface-wiki-uno`, `uno-toolkit`, `uno-navigation`, `uno-extensions-services`) | ❌ planned `uno-app-ui-testing` / `uno-app-test-assertions` not applied (walkthrough hit nav blocker before assertion phase) | ⚠ partial — best implementation-skill discipline, verification phase blocked |
| **5** | ✅ | ✅ both | ⚠ 4 actual `Skill` tool calls (uno-platform-agent, winui-xaml, uno-material, uno-extensions-services + uno-migration-troubleshoot for FP debug); skipped `uno-toolkit` + `uno-navigation` invocation — discipline gap caused FP #2 (UniformGridPanel not in Toolkit) | ❌ MCP disconnect blocked uno-app-ui-testing / uno-app-test-assertions | ⚠ partial — discipline gap-driven build error |
| **6 (re-run)** | ✅ | ✅ both | ✅ **6 actual `Skill` tool calls** (uno-navigation, uno-material, uno-toolkit, uno-extensions-services, winui-xaml, uno-app-ui-testing — 1 each); 2 honest planned-non-invocations (uno-platform-agent, userinterface-wiki-uno = failure mode #1, reported as such, not fabricated) | ✅ uno-app-ui-testing (1); MCP outage blocked actual walk-through but skill was loaded | ✅ **DISCIPLINE-AUDIT clean** — 6 Skill calls = 6 SKILL-USE lines; no failure mode #2; failure mode #1 honestly reported on 2 rows |
| **7** | ✅ | ✅ both rules-init packs invoked | ❌ **failure mode #2** — `SKILL-USE` log lines + skill-usage summary table both populated for `uno-platform-agent`, `winui-xaml`, `uno-toolkit`, `uno-material`, `uno-extensions-services`, `uno-app-ui-testing`, `uno-app-test-assertions`, **but 0 actual `Skill` tool calls** were made for any of them. Lines described where each skill's domain applied to the work; no tool was invoked. | ❌ same — `uno-app-ui-testing`/`-assertions` claimed in table, not actually called | 🔁 must re-run — failure mode #2 (corrected in test-7.md after the run; methodology invalidated). The two MCP rule-pack tools (`mcp__uno__uno_platform_agent_rules_init`, `mcp__uno__uno_platform_usage_rules_init`) DID run — those are the only real tool invocations in the discipline ledger. |

**Cross-test observation (already noted in test-3):** when the rules-init pack is loaded once, agents tend to internalize the patterns and stop re-querying the per-skill docs. Methodology limitation that **enables failure mode #2** — the rules-init context primes the agent to write code from internalized patterns and then describe it as "skill applied" without making the per-task `Skill` tool call. Parent SPEC §"Skill-usage discipline" was hardened on 2026-04-28 to close this loophole: the `Skill` tool call now must come *before* the code, the `SKILL-USE` log line must include a `tool-call-summary` field referencing the skill's actual return, and a self-audit gate at workflow step 10.5 cross-checks transcript invocations vs claimed counts before "Done."

**Score across the 7 runs (post re-runs):** test-1 re-run ✅ clean, test-2 🔁 still failure mode #1, test-3 ⚠ partial (no impl-skill invocations), test-4 ⚠ best impl discipline / no verification, test-5 ⚠ discipline gap caused build error, test-6 re-run ✅ clean, test-7 🔁 still failure mode #2. **2 of 7 are now clean (test-1 re-run + test-6 re-run); test-4 had the strongest impl discipline; test-2 + test-7 still need re-runs.**

---

## 12. Anti-patterns observed (parent SPEC §"Anti-patterns to avoid")

| Anti-pattern | test-1 | test-2 | test-3 | test-4 | test-5 | test-6 | test-7 |
|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| Cards-without-tap | ❌ confirmed live (Border-wrap) | ✅ avoided — Click handlers wired on all recipe / cookbook / contributor cards (Avocado Toast → RecipeDetail; Troyan Smith → OtherProfile verified by walk-through) | ✅ avoided | ❌ confirmed live (ListViewItem swallows click) | ❌ confirmed live — `OpenRecipeCommand` not bound to ItemTemplate; heart icon decorative; rating no command (5 noted) | ✅ avoided (FP #11) | ⚠ caught + fixed (FP #6–#12: Button-wrap + IsItemClickEnabled) |
| Empty state over real data | ✅ avoided | ✅ avoided (5/5 implemented; uses Material defaults instead of bespoke illustrations) | ✅ avoided | ✅ avoided | ⚠ logic + copy + layout in place; SVG empty-state illustrations don't render (Uno.WinUI.Svg gap) | ✅ avoided | ✅ avoided |
| Tab content empty (Recipe Detail) | not exercised | ✅ all 4 tabs render via Pivot/PivotItem | ✅ all 4 tabs render | ✅ all 4 tabs render | ✅ 4 tabs wired w/ IndexToVisibilityConverter (not visually walked) | ✅ | ✅ all 4 render (Pivot underline blue not pink) |
| Hard-coded counts (`"12 recipes"` literals) | ✅ avoided | ✅ avoided (no literals; Profile "Recipes: 12" only from data) | ✅ avoided | ✅ avoided | ⚠ Profile shows `"Recipes: 12"` driven by current-user record value | ✅ avoided | ✅ avoided |
| `NotImplementedException` stubs | ✅ avoided | ✅ avoided | ✅ avoided | ✅ avoided | ✅ avoided | ✅ avoided | ✅ avoided |
| Stub commands (no-op `RelayCommand`) | ❌ Save/Update/Sign Up | ⚠ Settings/Profile Save not persisted (in-memory mutate only); functionally similar | ✅ avoided | ⚠ Profile read-only | ❌ confirmed live — Cookbook/Settings/Profile Save commands `NavigateBackAsync` only (no write) | ⚠ ForgotAsync / ShareAsync stubs (FP #17/#18 fixed) | ✅ avoided — all RelayCommands resolve via INavigator |
| Back-chevron missing on stack page | not fully exercised | ✅ present on RecipeDetail/LiveCooking/CookbookDetail/UpdateCookbook/Profile/OtherProfile/Settings; modal close on Filters/Notifications | ✅ present everywhere | ⚠ workaround-related nav loss | ⚠ wired via `_navigator.NavigateBackAsync(this)` on every drill-down (unverified — only Onboarding walked) | ⚠ Toolkit auto-back broken on Skia desktop; explicit MainCommand override on 10 pages | ✅ present on every stack-pushed page |
| Routed-but-empty | ❌ Home re-render bug | ✅ avoided (no dead routes observed) | ✅ avoided | ✅ avoided | not assessed | ⚠ caught FP #4 then fixed | ⚠ caught + fixed (FP #3/#4: empty Region.Navigator regions on `IsDefault: true`) |
| Toggle state-only | not assessed | ⚠ NightMode toggle wired to `SystemThemeHelper.SetApplicationTheme(bool)`; dark-theme flip not visually verified | not assessed | not assessed | ⚠ NightMode toggle wired to `IThemeService` but not visually validated | not assessed | ⚠ **Night Mode flips IsOn but theme doesn't propagate** (FP #13 deferred — `ThemeToggleService.Attach` race vs `App.Services` init) |
| Theme dead-spots | ⚠ unverified | ⚠ NavigationBar inversion not wired into `utu:NavigationBar` styles (lightweight styling pass would flip) | ✅ flip propagates | ⚠ wired but not propagated | ⚠ unverified — dark not exercised | ✅ propagates | ⚠ WindowChrome titlebar always charcoal regardless of theme (Uno Desktop limit, not addressable from app code) |
| Asset 404s | not assessed | ⚠ Welcome filenames mismatched (FP #5 corrected); `Maps/map.jpg` doesn't exist (FP #6 placeholder); SVGs `location_pin.svg`/`location_circle.svg` render transparent on Skia | ⚠ ingredient icons | ⚠ ingredient icons | ⚠ 8 SVGs miss (`chefsappsignature_*`, `empty_*`, `success_*`, `splash_screen.svg`); ingredient icons | ⚠ ingredient icons | ⚠ 6 SVGs miss (wordmark + success_*; viewBox issue) |
| Identical tablet layout | ❌ 16/20 phone-stretch | ❌ phone-only — Figma rate-limit blocked tablet variants from being pulled | ❌ phone-only | ❌ deferred | ❌ identical phone+tablet — no AdaptiveTrigger | ✅ shipped FP #21–#23 | ❌ no tablet visual states defined |
| **Anti-patterns shipping live at end** | **3** | **4** (tab-swap clears stack + NearMeMap SVG transparent + persistence in-memory + Live Cooking surrogate) | **0** | **2** | **5** (cards-without-tap + identical tablet + stub save + decorative heart + no rating command) | **0** (9 caught, all fixed) | **2** (toggle theme deferred + WindowChrome) |

---

## 13. Codebase comparison matrix — cross-cutting

Cross-tabulating each run's codebase comparison's **Match** column for rows that varied:

| Aspect | test-1 | test-3 | test-4 | test-6 | test-7 |
|---|:-:|:-:|:-:|:-:|:-:|
| Pages — count (20) | ✅ | ✅ | ✅ (19 distinct + Splash auto) | ✅ | ✅ (19 explicit Pages + Splash auto) |
| Navigation graph | ⚠ post-verif | ✅ | ⚠ deep drill-down broken | ⚠ back is ClearBackStack→Main, not true pop | ✅ flat-routed under Shell after region-based abandoned (FP #3/#4) |
| Theme — light + dark | ✅ | ✅ | ⚠ dark not visually verified | ✅ verified app-wide flip | ⚠ both themes wired; runtime toggle has service-init race (FP #13 deferred) |
| Theme — primary color (chef-pink) | ✅ `#E8455C` | ⚠ `#FF1F5A` (DESIGN.md drift) | ✅ `#E91E63` per DESIGN.md | ❌ MD3 default purple | ⚠ `#FF1F5A` light / `#FF6B8A` dark (same DESIGN.md drift as test-3) |
| Theme — surface inversion | ✅ | ✅ | ✅ | ❌ MD3 default | ✅ `#2D2D2D` via `AppBarFillBrush` resource |
| Theme — secondary cream | ✅ | ✅ | ✅ | ❌ MD3 default | ✅ `#EAE3D6` |
| Typography scale | ✅ | ✅ 5-tier | ✅ 9-tier (most granular) | ⚠ MD3 tokens, not reference's | ✅ 5-tier (24/18/16/14/12-13) |
| Card-tap navigation | ❌ post-verif | ✅ | ❌ | ✅ (post-fixes) | ✅ (post-FP #6–#12: Button-wrap + IsItemClickEnabled) |
| Bottom nav (3 tabs) | ✅ | ✅ | ⚠ pill color drifts | ⚠ 4 tabs, not 3 | ✅ 3 tabs (pink icon + cream pill on Search/Favorites) |
| Tablet adaptive layout | ⚠ 4/20 pages | ❌ phone-only | ❌ deferred | ✅ shipped (FP #21–#23) | ❌ no tablet visual states |
| SVG rendering | ⚠ wordmark + splash text-fallback | ✅ native Skia SVG | ⚠ wordmark text+icon | ❌ emoji fallback | ⚠ 88/94 — empty-state SVGs render; 6 outliers miss (wordmark `chefsappsignature_*.svg` + `success_*.svg`; viewBox issue) |
| Live Cooking media player | not exercised | ⚠ surrogate | ⚠ static rect | ⚠ basic transport, no PiP/cast/fullscreen | ⚠ image + ▶ + ProgressBar scrubber + 01:12/03:00 (3 of 6 controls) |
| Empty states (5 required) | not assessed in walk | ✅ 5/5 | ✅ 5/5 | ⚠ emoji not SVG | ✅ 5/5 (Profile-no-recipes confirmed for james.bondi w/ Recipes=0) |
| Recipe Detail tabs (4) | not exercised | ✅ 4/4 | ✅ 4/4 | ✅ | ⚠ 4/4 via Pivot (underline blue not pink) |
| Persistence on save | not assessed | ⚠ in-memory only | ⚠ in-memory only | ⚠ in-memory only | ⚠ in-memory only (`SaveCookbookAsync`, `UpdateCurrentUserAsync`) |
| Asset coverage (94 bundled) | not assessed | ⚠ ingredient icons missing (input gap) | ⚠ same | ✅ pre-copy step worked | ⚠ 88/94 (6 SVG outliers) |
| Localization (4 locales) | ❌ | ❌ | ❌ | ❌ | ⚠ `UseLocalization` retained, no `Resources.resw` |

The "ingredient icon" gap (Recipes.json references `ms-appx:///Assets/Icons/avocado.png` etc., but `reference/assets/Icons/` only ships Chefs branding + close.svg) is **input loss, not implementation loss** — affects every run identically.

---

## 14. Manual corrections / fix-pass volume

| # | Manual operator corrections | Compile-error FPs | Walkthrough/anti-pattern FPs | Total FPs |
|---|---|---|---|---|
| **1 (original)** | 2 (sed `Padding`→`Margin`; `ItemsControl`→`ItemsRepeater` for horizontal lists on Skia desktop) | (logged not aggregated) | partial gap-closure + verification with new defects | not aggregated |
| **1 (re-run)** | 0 | 1 (FP #1: PipsPager + OnboardingFrame namespaces) | 1 (FP #2: Uno.WinUI.Svg missing — runtime warning) | **2** |
| **2** | (not aggregated) | 6 (4 build errors → 6 build-error fix passes incl. ChefsViewModelBase partial, scaffold stub removal, DataViewMap registration, FlowLayout swap, Welcome filenames, IThemeService → SystemThemeHelper) | 3 (2 walkthrough text-wrap + 1 anti-pattern map asset) | **9** |
| **3** | 0 | 1 (FP #1) | 2 (3 total) | **3** |
| **4** | 1 (Hot Design dialog dismissal) | 1 (Pivot self-close) | 3 behavioural (Shell explicit Login nav, MainShell inline-tab workaround, Home card-tap rewire) | **4** |
| **5** | 0 | 2 (global usings; UniformGridPanel→ItemsRepeater) | 1 (runtime-warning Svg flag added but not resolving) | **3** (session collapsed mid-validation) |
| **6 (original)** | (not aggregated) | 4 (KE0001 + 2× CS0246 + Uno0001 WrapGrid) | 19 (walkthrough 7 + anti-pattern 12) | **23** |
| **6 (re-run)** | 0 | 2 (Onboarding namespace hoist + Favorites x:Bind indexer → explicit properties) | 0 (walk-through blocked, no behavioural fixes possible) | **2** |
| **7 (post iter-3)** | (not aggregated) | 0 (5/5 first-try) | 5 walkthrough (#3 Frame swap, #4 flatten routes, #5 TimeSpan converter, #13 theme propagation deferred, #14 ThemeToggleService.Attach moved to App.OnLaunched) + 7 anti-pattern (#6–#12 cards-without-tap) + 5 screenshot-diff (Onboarding hero, #15 Login wordmark, #16 Register wordmark, #17 LiveCookingFinish illustration, plus 1 earlier) | **17** (was 13 post iter-2; iter-3 added #14/#15/#16/#17) |

Reading: **test-3 still has the lowest correction volume (0 manual, 3 fix passes), with test-1 re-run + test-6 re-run now tied at 2 FPs each.** test-6's drop from 23 → 2 fix passes is the biggest delta in the experiment — proper skill discipline + rules-init at session start prevented 21 of the original 23 fix passes. test-7 sits at 17 FPs after iter-3 (was 13 after iter-2) — iter-3 added 4 infrastructure fixes (1 walkthrough + 3 screenshot-diff). test-4's count is artificially compressed by the Windows blocker truncating the verification phase.

**Re-run takeaway:** the original test-1 (5/5 first-try) and test-6 (23 FPs) numbers were **methodology artifacts of pre-discipline runs** — re-running both with proper rules-init + actual `Skill` tool calls produced 2 FPs each but cost the "5/5 first-try" claim on test-1 (FP #1 namespace was always real; the original run just happened to scaffold without it). The honest cross-test FP-count picture is now: 0 (test-3 manual) / 2 (re-runs) / 3 (test-5 partial) / 4 (test-4) / 9 (test-2) / 17 (test-7 iterative).

---

## 15. One-line verdicts

- **test-3 (Stitch DESIGN.md) — A-:** cleanest run on every dimension that *was measured* — 5/5 builds first-try, 0 manual corrections, 20/20 pages navigable, no anti-patterns hit. Ceiling is criterion 3: per-combo visual scoring needs an out-of-session validator.
- **test-7 (iterative visual-diff loop, post iter-3) — B+ (🔁 re-run still required for failure mode #2):** 5/5 builds first-try, 20/20 pages routed and data-bound, **~76% avg visual match measured in-run post iter-3** (was ~73% post iter-2; 3 pages under 75% threshold post iter-3, was 5 pages post iter-2). Iter-3 closed dark-theme propagation app-wide (#14), Login/Register wordmark SVG → styled TextBlock fallback (#15/#16), and LiveCookingFinish success illustration → emoji+badge fallback (#17). Still hit **failure mode #2** of the skill-usage discipline — 6 narrative `SKILL-USE` lines logged, 0 actual `Skill` tool calls beyond the rules-init MCP packs. The structural-debugging finding (walk-through caught empty `Region.Navigator` regions + TimeSpan deserialization in iter-1) is real and load-bearing, but the methodology label "this is what the iterative loop + Uno's AI stack delivers" is wrong until the re-run lands the discipline cleanly. **Iter-3 found a mixed result on the loop's ceiling claim:** 3 of 5 below-75% pages were closed by replacing non-rendering primitives with simpler fallbacks (not by the diff loop closing fidelity gaps). The 3 still-plateaued pages (Onboarding hero proportion, FavoritesCookbooks 4-image collage, NearMeMap pin positioning) need real infrastructure work (custom controls, native Map). The ceiling thesis still holds.
- **test-6 re-run (blind PRD, negative control) — C+:** the re-run is the most informative data point in the matrix from a discipline perspective. Same blind-PRD methodology, same MD3 default visual fidelity — but **2 fix passes vs 23 in the original run**. Proper rules-init + skill invocation at session start prevented 21 of the original's fix passes. Still proves a blind PRD *can* produce a working 20-page Uno app; the visual fidelity loss to MD3 defaults is the experiment's load-bearing data point. Visual unmeasured because uno-app MCP disconnected at session start (unrelated to PRD methodology). Discipline ledger now clean.
- **test-1 re-run (screenshots-only) — C:** discipline-clean re-run with 8 actual `Skill` tool calls + clean DISCIPLINE-AUDIT. Build is no longer 5/5 first-try (FP #1 needed for namespace fix); 20/20 pages routed + data-bound at code level; visual unmeasured because uno-app MCP went down mid-run (third occurrence — same MCP fragility as test-5 + test-6 re-run). Border-wrapped cards-without-tap acknowledged as known gap. The original "5/5 first-try" claim was a methodology artifact — the re-run produced more honest numbers.
- **test-2 (Figma MCP only) — C (🔁 re-run required):** Figma MCP at the **View seat tier rate-limited after ~5 calls (4 of ~30 frames retrieved)** — making the "Figma is the sole input" methodology structurally incomplete for a 20-page app on this seat. What got through *was* high-quality (chef-pink `#E8455C` light / `#FFA3B7` dark, secondary cream `#EAE3D6` / olive dark `#494737`, surface inverse `#2D2D2D`, Roboto type scale) and matched upstream tokens as well as test-3 / test-7. The remaining ~26 frames fell back to Material defaults. **Headline finding is methodology cost, not fidelity.** Build was not first-try (4 errors → 7 fix passes) and 17/20 pages were walked via UI. Skill discipline failure mode #1 — all skill invocations logged retroactively after user prompted on it, not in-flow.
- **test-4 (custom visual-breakdown skill) — C-:** the visual-skill `DESIGN.md` was the highest-leverage input on token decisions (color + audit + open-questions defaults eliminated dozens of micro-decisions), but blocked by Windows-WinAppSDK XAML intolerance and an inline-tab nav workaround that ate region context. Best implementation-skill discipline of the four runs.
- **test-5 (Figma + screenshots hybrid → screenshots-only) — D (partial / methodology collapsed):** the hybrid premise died on the **first Figma MCP call** (View seat — same rate-limit as test-2 but earlier). Methodology fell back to screenshots-only at minute 0; uno-app MCP then disconnected mid-session, blocking the per-page screenshot-diff. Ended with **Desktop build green only** (other 4 targets not exercised), Onboarding visually walked, 19 pages routed-but-unverified. Did not clear the pass bar. The honest read: **test-5 ≈ test-1** in this run (the expected hybrid > pixels-only delta did not materialize). The methodology-cost / session-recoverability findings are the load-bearing observations.

---

## 16. Tentative observations from comparing the four

Treat as hypotheses to be confirmed/refuted once test-2 + test-5 + test-7 run, not as conclusions.

1. **Visual-input modality changes which dimension fails, not whether the run passes.** Screenshots → deep nav fails. DESIGN.md → builds clean and nav works, but per-combo visual scoring needs out-of-session validation. Blind PRD → visual fidelity collapses to framework defaults but functional structure holds.
2. **First-try build success is uncorrelated with pass-bar pass.** test-1 had 5/5 first-try and didn't clear the bar; test-6 had 0/5 first-try and did clear (criteria 1 + 2). The build matrix is a necessary but weak signal.
3. **Card-tap is the most consistently broken pattern across modalities.** test-1 and test-4 ship it broken at runtime despite Command wiring compiling; test-3 and test-6 only get it right with explicit Button-wrapping.
4. **Tablet adaptive layout fails in every run except test-6.** Inputs (DESIGN.md, screenshots, PRD) all under-specify it; only test-6's late improvement session (FP #21–#23) shipped a real responsive swap. Input-format problem more than per-run failure.
5. **Every run dropped MVUX, Localization, and Authentication** despite the original calling `UseMvux()`, `UseLocalization()` (4 locales), and `UseAuthentication()`. The MVVM default in operator memory + the absence of `Strings/{loc}/Resources.resw` in the input set steers every run that way; auth was downgraded to demo bypass in all 4 runs.
9. **Page-list inflation:** the parent SPEC's "20 required pages" expands the upstream's **16 distinct pages + 2 dialogs + 1 flyout** by treating segmented states (Favorites All/Cookbooks), parameterized pages (Profile own/other), dual-mode pages (CreateUpdateCookbook), and dialogs (CompletedDialog) as separate surfaces. Tests inherit the inflation and ship 19–21 page types — the run that scores best on "20/20 reachable" is over-shipping vs the canonical architecture.
10. **Real interactive components (`Mapsui.Uno.WinUI`, `LiveChartsCore.SkiaSharpView.Uno.WinUI`) are dropped by every run.** Map → placeholder rectangle; nutrition donut → stacked `Ellipse` strokes. The cross-cutting failure is "AI doesn't reach for charting/mapping NuGet packages unless explicitly told."
6. **The skill-usage discipline as written under-counts implementation-skill use** — once `rules-init` is loaded, agents internalize patterns. test-3's writeup explicitly flags this. Worth re-shaping the discipline before the public stunt.
7. **WinAppSDK is the most fragile target.** Only test-4 was caught visibly, but the silent crash mode means other runs may have been one XAML-pattern change away from the same failure.
8. **The original's component library is systematically substituted for primitives** in every run. The runs reach for `Border + Grid + StackPanel` instead of Toolkit composites — a literal "didn't know they existed" outcome. Concrete substitutions verified upstream:
   - **`utu:AutoLayout` (30+ uses on RecipeDetailsPage alone) → 0 uses across all 4 tests.** Every run reached for `Grid + StackPanel + Padding/Margin` instead.
   - **`uer:FeedView` (6+ uses across HomePage + RecipeDetailsPage) → 0 uses across all 4 tests.** Every run reimplemented async-data display from scratch with `ItemsControl` + a `HasItems` bool.
   - **`utu:TabBar TopTabBarStyle` for in-page tabs (RecipeDetails 4-tab) → 0 uses.** Tests reached for deprecated `Pivot` (test-4, caused Win-XAML compile fail) or hand-rolled `Button[]` + `Visibility`.
   - **`uen:Region.Attached` + `Region.Name` + `Region.Navigator="Visibility"` (MainPage tab content + RecipeDetails tab content) → 0 uses.** Tests used flat content swaps and Bool-to-Visibility, which is why `_navigator.NavigateRouteAsync` lost region context in test-4.
   - **`uen:Navigation.Request` / `uen:Navigation.Data` markup ext → 0 uses.** Every run wired `Command="{Binding OpenRecipeCommand}"` instead, which broke at runtime in test-1 (`utu:CommandExtensions` non-firing) and test-4 (ListViewItem container intercepts click).
11. **Real `MediaPlayerElement` is reachable in 6 lines of XAML** (`AreTransportControlsEnabled="True"` + `AutoPlay="True"` + `Source="ms-appx:..."` + `MediaTransportControls IsCompact="True"`). Three of four tests built a placeholder with overlays, glyphs, and rectangles instead of using the control. Only test-6 wired the real one — and only because the blind PRD's REQ-F8 explicitly named `MediaPlayerElement`.
12. **Live Cooking step pager is `FlipView` + `utu:FlipViewExtensions.Previous`/`.Next` markup, not Buttons + index.** Original wires `<Button utu:FlipViewExtensions.Previous="{Binding ElementName=StepsFlipView}">` — single declarative attached property. All tests built it as Button + Command + index increment.
13. **The iterative visual-diff loop has a ceiling: structural debugging vs fidelity polish.** test-7 (the only run that exercised the loop) converged to ~73% avg visual match in 2 iterations on functional/structural issues, then capped early because the remaining 5-page gap below 75% required infrastructure changes (SVG asset re-export, custom toggle template, custom WindowChrome, real Map control) that the visual-correction loop is not designed to address. **The loop's actual load-bearing value showed up as structural debugging:** walk-through in iter-1 caught (a) empty `Region.Navigator` regions when `IsDefault: true` + `Region.Navigator="Visibility"` don't auto-instantiate, (b) silent JSON deserialization failure on `TimeSpan {ticks: N}` shape → empty Trending/RecentlyAdded carousels. **Both would have shipped silently in a one-shot run with green builds.** The MCP attach + walk converted ~4 minutes into 2 root-cause fixes (FP #3/#4 + #5) that unblocked the whole run.
14. **Region-based navigation with `IsDefault: true` + `Region.Navigator="Visibility"` doesn't auto-instantiate child regions on Skia desktop** — a new Tier-A gotcha surfaced by test-7 (FP #3/#4). The pattern compiles cleanly and routes register, but tab regions render empty. test-7 pivoted to **flat routing under Shell** instead. test-4 hit the same shape (its Blocker #2 inline-tab workaround swallows region context) — confirms it's not a one-off. The Toolkit composite + region pattern that **upstream uses pervasively** has a runtime trap not documented anywhere visible.
15. **🆕 The `uno-app` MCP is the single biggest delivery risk in the experiment — confirmed across 4 of 7 runs.** Test-1 re-run: MCP went down mid-run after capturing initial console-log triage. Test-5: MCP disconnected mid-session after Onboarding, blocking the per-page screenshot pass. Test-6 re-run: MCP was disconnected at session start (the deferred-tools system reminder reported them unavailable). Test-3: MCP was running but tool *schemas* were lazy — calls returned `InputValidationError` until an explicit `ToolSearch select:mcp__uno-app__*` resolved them. **Only test-7 had clean MCP-driven visual validation throughout.** Operational implication for the public stunt: (a) verify `mcp__uno-app__*` tool schemas at session start via explicit ToolSearch, (b) treat mid-session disconnects as a known recoverability failure mode (restart Claude Code session if `uno_app_*` tools become unavailable), (c) the methodology score should explicitly note when the MCP outage capped criterion-3 measurement so it's not conflated with implementation quality.
16. **🆕 Test-1 + test-6 re-runs reveal that "first-try build success" is a brittle proxy for run quality.** The original test-1's 5/5 first-try claim was a methodology artifact — the re-run with proper skill discipline produced 0/5 first-try because FP #1 (PipsPager + OnboardingFrame namespaces) was always real, just missed in the original's looser checklist. The original test-6's 23 fix passes inflated by the same artifact in reverse — proper rules-init + skill calls at session start prevented 21 of those. **Discipline state, not pure scaffolding luck, dominates the FP volume.** This is the load-bearing finding from the re-runs: the methodology controls the FP curve more than the input modality does.
17. **🆕 Why the Uno Toolkit composites are systematically substituted for primitives (the WHY behind observation #8).** The "what" — `utu:AutoLayout`, `uer:FeedView`, `utu:TabBar TopTabBarStyle`, `uen:Region.Attached`, `uen:Navigation.Request` all at 0 uses across all 7 runs vs pervasive use upstream — is documented. The "why" is five compounding mechanisms:
    1. **Inputs don't name them.** Every visual input format (screenshots, Stitch DESIGN.md, visual-skill DESIGN.md, PRD, Figma frames) describes *what to render*, not *which Uno control to use*. Agents fill the gap with primitives they're confident about — `Grid`, `StackPanel`, `Border`, `ItemsRepeater` — because those work on every target out of the box.
    2. **Pretraining priors dominate.** There are orders of magnitude more WPF / WinUI / UWP examples online using `Grid + StackPanel` than Uno-specific examples using Toolkit composites. When an agent infers "vertical container with consistent spacing," its default reach is `<StackPanel Spacing="12">`, not `<utu:AutoLayout Orientation="Vertical" Spacing="12" Justify="Stretch">` — the StackPanel pattern is statistically dominant.
    3. **The `uno-toolkit` skill returns reference docs, not "replace this primitive here."** Test-4 invoked `uno-toolkit` 7 times and *still* shipped `Grid + StackPanel` for layout primitives. The skill confirms `utu:TabBar` exists, validates `BottomTabBarStyle`, etc. — but it doesn't surface "your StackPanel above should be `utu:AutoLayout`." Skills load knowledge; they don't refactor in-progress code.
    4. **`uer:FeedView` is structurally MVUX-shaped.** It expects `IFeed<T>` / `IState<T>` from MVUX. All 7 runs went MVVM (operator default + absent MVUX inputs). On an `ObservableCollection`, FeedView loses its reactive plumbing, and the equivalent MVVM pattern is `ItemsRepeater + HasItems-bool + empty branch` — which is exactly what every run shipped. The MVUX→MVVM substitution propagates downstream into "no FeedView."
    5. **`uen:Region.Attached` has a documented runtime trap on Skia desktop** (observation #14). Test-7 (FP #3/#4) and test-4 (Blocker #2) both tried the upstream-canonical `IsDefault: true` + `Region.Navigator="Visibility"` pattern, hit empty regions at runtime, and pivoted to flat routing. Once that lesson lands in shared memory / docs / agent context, future runs *avoid* the canonical pattern preemptively. The framework's own runtime gap is making the substitution stickier across runs.

    **Cross-cutting takeaway:** agents reach for Toolkit composites only when (a) the input names them, or (b) the canonical pattern fails loudly enough to force a docs lookup mid-run. Visual inputs do neither. The fix isn't "tell the agent to use the toolkit harder" — it's seeding inputs (DESIGN.md, PRD, Figma component map) with explicit Uno control mappings, or running a follow-up "Toolkit composites" experiment that *does* name them in the input set (already logged as an open question below).

---

## 17. Cross-cutting engineering gotchas (from `test-N-learnings.md`)

Synthesized from `test-1-learnings.md` (20 items, screenshot-only run) and `test-6-learnings.md` (10 sections, blind-PRD MVUX run). These are the cliffs the next test should not need to re-discover. Cross-test attribution shows which runs bit each one (✅ explicitly logged · ⚠ implicitly hit · — not relevant or not exercised).

### A. Universal — bit every run that exercised the path

| Gotcha | Source | t1 | t3 | t4 | t6 | t7 | Resolution / takeaway |
|---|:-:|:-:|:-:|:-:|:-:|:-:|---|
| **`Svg` is NOT in default `UnoFeatures`** even with `SkiaRenderer` — SVG `Image.Source` silently fails | t1 #2/12 | ✅ | ⚠ | ⚠ | ✅ | ⚠ added but 6 SVGs still miss (viewBox issue in those specific files) | Add `Svg;` to `<UnoFeatures>` up front whenever `reference/assets/Images/` is in the input. Asset pack ships brand wordmark + empty-state SVGs that won't render without it. **test-7 surfaced a downstream variant: even with `Svg` enabled, SVGs with malformed viewBox attributes (chefsappsignature_*, success_*) silently render blank.** Pre-render SVGs at build time to PNGs as a fallback. |
| **TimeSpan in fixtures is two-encoded:** `{"ticks": 9600000000}` (Recipes.json) AND `"00:10:00"` (Cookbooks.json) | t1 #4 + t6 §1.2 + t7 FP#5 | ✅ | ✅ | ✅ | ✅ | ✅ FP #5 (root-cause for empty Trending carousels) | Custom `JsonConverter<TimeSpan>` with a `reader.TokenType` switch handling all 3 shapes (object w/ ticks · bare number · ISO string). **Confirmed across all 5 measured runs.** API-CONTRACT.md docs the DTOs but not the wire shape — always sniff before trusting the default serializer. |
| **`dotnet run -f net10.0-desktop` returns exit 0 (or "127" via Bash background) within seconds — looks like crash, isn't** | t1 #3 + t6 §6.1 | ✅ | ✅ | ✅ | ✅ | ✅ | Verify launch via `Get-Process -Name <App>` (PowerShell). Empty stdout ≠ silent crash; Uno's `ILogger` doesn't route to `Console.Out` by default. `tasklist` from Bash is unreliable — use PowerShell. |
| **Image refs that don't resolve don't error.** Wrong asset path (`ms-appx:///Assets/Maps/map.png` when only `.svg` ships) → blank render, no diagnostic | t6 §4.5 | ⚠ | ⚠ | ⚠ | ✅ | ⚠ same shape on 6 SVG outliers | Pre-grep `Assets/` for actual filenames before referencing in XAML. The 4 ingredient-icon `ms-appx:///Assets/Icons/avocado.png` 404s in §13 are this exact failure shape. |
| **Cards inside `ItemsRepeater`/`ListView` `ItemTemplate` need a focusable root** (Button, ToggleButton) — never `Border` or `Grid` | t1 #6 + t6 §4.1 + t7 FP#6–#12 | ✅ | ✅ | ✅ | ✅ | ✅ FP #6–#12 (7 cards-without-tap fixes across Home/Search/Fav/CookbookDetail/Profile/OtherProfile) | Wrap each card in `<Button Style="...">` or set `IsItemClickEnabled="True"` on `GridView`. From card code-behind, walk the visual tree to find the page-level VM (`while (parent.DataContext is not BindableMainModel) parent = VisualTreeHelper.GetParent(parent)`) — `DataTemplate` `DataContext` is the *item*, not the page VM. **Confirmed across all 5 measured runs.** |
| **Region-based navigation with `IsDefault: true` + `Region.Navigator="Visibility"` doesn't auto-instantiate child regions on Skia desktop** | t7 FP #3/#4 + t4 Blocker #2 | not exercised | not exercised | ⚠ inline-tab swallows region context | not exercised | ✅ caught + abandoned for flat routing | **New Tier-A gotcha surfaced by test-7.** The pattern compiles, routes register, but tab regions render empty. Workaround in test-7: pivot to flat routing under Shell. The Toolkit composite + region pattern that **upstream uses pervasively** (§7) has a runtime trap not documented anywhere visible. |

### B. MVUX-only (test-6 hit all of these; tests 1/3/4 dodged by going MVVM)

| Gotcha | Source | Resolution / takeaway |
|---|:-:|---|
| **`KE0001` — every record with `Id` MUST be `partial`** (MVUX `IKeyEquatable` source-gen extends them) | t6 §1.1 | Mark `RecipeData`/`CookbookData`/`UserData`/`ReviewData`/`CategoryData` partial. Records w/o `Id` (Ingredient/Step/Nutrition/LoginRequest/Notification/OnboardingSlide) don't need it. **Note: the gen runs even on MVVM projects when Hosting/Navigation `UnoFeatures` are enabled** — test-6 hit this on a non-MVUX scaffold. |
| **`IFeed<ImmutableList<T>>` ≠ `IFeed<IImmutableList<T>>`** — `IFeed<T>` is invariant; `List.ToImmutableList()` returns concrete | t6 §2.1 | Always declare feeds + states with the **concrete `ImmutableList<T>`**, never the interface form. Bonus: dodges `State<...>.Value(this, () => ImmutableList<T>.Empty)` overload-resolution ambiguity. |
| **`Feed.Select(async ...)` doesn't compile — use `SelectAsync`** | t6 §2.2 | Sync overload is `.Select(Func<T,T2>)`; async is `.SelectAsync(Func<T, CancellationToken, ValueTask<T2>>)`. |
| **`IFeed<T>.Refresh()` requires a `CancellationToken`** | t6 §2.3 | Don't reach for manual `.Refresh()`. Structure feeds to observe an `IState` that changes when the underlying data does — MVUX re-evaluates automatically. |
| **`BindableMainModel` "type not found" cascades from upstream errors** | t6 §2.4 | Source generators don't emit if input source has earlier compile failures. Resolve KE0001 / SelectAsync / partial-record errors first; CS0246 on bindables clears. To future-proof against generator renames, walk the visual tree matching `GetType().Name.EndsWith("BindableMainModel")` reflectively. |
| **`IFeed<T?>` clashes with `Feed.Async<T>`'s `notnull` constraint** (CS8714/8620/8621 warnings) | t1 #5 | MVUX's contract is closer to "always produces a value" than "might be null." Model true no-data states as `IFeed<Option<T>>` or use `Feed.AsyncEnumerable` w/ empty sequence. Don't reach for `T?` out of habit. |
| **MVUX auto-generates `IAsyncCommand` from `(async) ValueTask Method()` / `Method(T arg)`** — no manual `RelayCommand` needed | t1 #8 | `public async ValueTask OpenRecipe(RecipeData recipe) => …` becomes `Command="{Binding OpenRecipe}" CommandParameter="{Binding}"`. Parameter type inference works. |

### C. Navigation gotchas

| Gotcha | Source | Resolution / takeaway |
|---|:-:|---|
| **`NavigateDataAsync` has no `route:` parameter** | t6 §3.1 | Use `NavigateRouteAsync(this, "RouteName", data: payload)` for routed-with-payload. `NavigateDataAsync(this, TData)` picks the route from the registered `DataViewMap<TPage, TModel, TData>`. |
| **`ViewMap<Page, Model>` silently drops `data:` arguments at navigation time** | t1 #7 | Register as `DataViewMap<Page, Model, TData>` whenever the VM ctor takes a payload beyond `INavigator` / DI singletons. test-1 spent a debug cycle on "always shows Avocado Toast" before catching this. |
| **Plain `ViewMap` + matching ctor `string?` parameter still receives `data:` strings** | t1 #17 | If your ctor parameter type matches `data:`, plain `ViewMap` works. `DataViewMap` is needed for deep-linking / state-restoration / typed-route generation, not for ctor-param marshalling. |
| **Route map under Shell is flat — every named route is a `Nested:` entry of the root** | t6 §3.2 | No multi-level nesting needed for ~16 screens. `IsDefault: true` marks the launch route. |

### D. XAML / build / tooling

| Gotcha | Source | Resolution / takeaway |
|---|:-:|---|
| **`MultiBinding` with an undefined converter compiles, crashes at first render** | t1 #9 | Prefer composite feeds (`Feed.Combine(A, B).Select(t => …)`) over `MultiBinding`. If you must use `MultiBinding`, register the converter in `App.xaml` resources first. |
| **`utu:Chip` selected-state needs `Style="{StaticResource MaterialChipStyle}"`** — without it, `IsChecked` toggles but visual is identical | t6 §4.3 | Apply the toolkit style. test-6 shipped this as one of the 18 deviations vs PRD. |
| **`x:Bind` falls back to runtime binding without `x:DataType` on the `DataTemplate`** | t6 §4.2 | Always set `x:DataType` — loses compile-time checking and is slower without it. |
| **`EmitCompilerGeneratedFiles=true` poisons the build** with ~50 CS0101 duplicates after one build cycle | t6 §5.1 | Don't enable unless you also `<Compile Remove="generated/**"/>`. To diagnose generator output: build once, copy file out, turn property off, delete folder. |
| **`Uno0001 ItemsWrapGrid not implemented` is a Skia/WASM/iOS warning, not an error** — falls back to default flow layout | t6 §5.3 | Replace `<ListView><ItemsWrapGrid/></ListView>` with `<ScrollViewer><muxc:ItemsRepeater><UniformGridLayout/></muxc:ItemsRepeater></ScrollViewer>`. test-6 fixed in FP #5/#16; test-3 + test-4 left as warning (rendered single-row degraded). |
| **Template placeholder files (`MainPage`/`SecondPage`/`Entity.cs`) must be `rm`-ed at scaffold time, not edited around** | t1 #10 | Leaves dead routing + stale references. Delete them, then rewrite `App.xaml.cs` `RegisterRoutes` wholesale. Generated partial classes regenerate on next build. |
| **`Assets/data/*.json` auto-globs as `Content` under Uno Single Project** — no manual `<Content Include="..."/>` needed | t1 #15 | `ms-appx:///Assets/data/Recipes.json` resolves without csproj edits. Different from classic WinUI templates. |
| **`uno.themes.winui.markup` package warning during NuGet restore** is harmless if you don't use C# Markup | t6 §5.4 | Comes from `Uno.Dsp.Tasks` transitive dep; doesn't block build. |
| **NU1903 vulnerability warnings on `System.Security.Cryptography.Xml` + `Tmds.DBus.Protocol`** are scaffold-default transitives | t6 §5.2 | Don't fail builds. Override package versions in a real product; safe to ignore during test runs. |

### E. MCP + session

| Gotcha | Source | Resolution / takeaway |
|---|:-:|---|
| **`uno-app` MCP server tools don't surface even when `.mcp.json` declares it** | t1 #1 + t6 §8.1 | Verify `dotnet dnx -y uno.devserver --mcp-app` runs cleanly outside the harness; harness silently drops failed servers. May need to apply starter kit *before* opening Claude Code. test-3 found a deferred-schema fetch path: `ToolSearch select:mcp__uno-app__uno_app_get_screenshot,...` resolves the tools when names appear but schemas are lazy. |
| **🆕 `uno-app` MCP can disconnect mid-session, killing the visual-validation pass.** Hit in test-1 re-run (mid-run), test-5 (mid-session after Onboarding), test-6 re-run (at session start). | t1-rerun, t5, t6-rerun | This is now a **confirmed cross-cutting cost** — 3 of 4 measured runs that depended on the MCP for criterion-3 visual capture lost the validation channel. Operational mitigation: (a) explicit `ToolSearch select:mcp__uno-app__*` at session start to force schema load, (b) save `uno_app_get_runtime_info` output early as the canary, (c) restart the Claude Code session if `uno_app_*` tools are reported as deferred-disconnected, (d) report criterion-3 as "unmeasurable due to MCP outage" rather than synthesizing a number from PowerShell GDI+ fallback (test-5 surfaced this fall-back as not equivalent to MCP-driven capture). |
| **Auto-memory could leak visual context across tests if not policed** | t6 §8.3 | Future-test methodology should confirm `~/.claude/projects/.../memory/` doesn't carry visual hints between blind / non-blind runs. |
| **Forbidden-input rules were honored in every measured run** — `Chefs-screenshots/`, `reference/forbidden/`, remote upstream, etc. | t6 §8.2 | The forbidden-input gate works as designed; no leakage observed. |

### F. Engineering wins worth recording (test-6 §9)

The "what worked smoothly" list isn't a gotcha — it's load-bearing for any next-test setup:

- `dotnet new unoapp -preset recommended -presentation mvux -markup xaml -theme material -platforms desktop|wasm|android -renderer skia -tfm net10.0` produces a working scaffold first try.
- `Feed.Async(async ct => …)` is ergonomic for the simple "fetch once, expose as IFeed" case.
- DI via `ConfigureServices((context, services) => services.AddSingleton<…>())` needs no reflection / attributes.
- `Image` w/ `ms-appx:///Assets/X.png` resolves on every target without per-platform tweaks.
- Uno.Material default palette + `MaterialFilledButtonStyle` / `MaterialOutlinedButtonStyle` / `MaterialTextButtonStyle` carry the entire button surface without custom styling.
- `utu:SafeArea.Insets="VisibleBounds"` on page root respects notch/status-bar without per-platform code.

### Reading

- **Universal gotchas (Tier A) bit every run** that touched the path: SVG, TimeSpan, dotnet-run exit code, image-ref silent failure, card-tap requires focusable root, **region-attached doesn't auto-instantiate (new from test-7)**. Adding these as up-front checklist items should remove ~5–10 min per run.
- **MVUX gotchas (Tier B) only bit test-6** because tests 1/3/4/7 all went MVVM. **If the public stunt re-runs tests 1+6 under MVUX (per the re-run flag), expect Tier B to bite both.** Worth pre-baking `partial` records, concrete `ImmutableList<T>` typing, and `SelectAsync` into a project-template gate.
- **Navigation gotchas (Tier C) split by methodology:** test-1 hit `ViewMap`-vs-`DataViewMap`; test-6 hit `NavigateDataAsync`'s lack of `route:`; test-7 hit region-based-with-`IsDefault` empty-regions and pivoted to flat routing. All easy to miss first time, trivial to fix once known.
- **Tier D (XAML/tooling) is the long tail** — most are warnings or first-time-only friction; `EmitCompilerGeneratedFiles` poisoning is the only one that *blocks* a build catastrophically.
- **Test-7 added one structural finding the others missed:** the iterative visual-diff loop has a *ceiling*. It surfaces structural bugs (empty regions + JSON deser errors) that compile-clean but render-empty in iter-1, then plateaus once the remaining gap requires infrastructure changes (asset re-export, custom templates, native controls). **Operationally this means: budget the loop for 2 iterations of structural fixes, then escalate to infrastructure work — don't expect the loop to close arbitrary fidelity gaps.**

---

## Open questions before this matrix becomes a public summary

- ~~When do test-1 and test-6 get re-run under the skill-usage discipline?~~ **Done 2026-04-28** — both re-runs landed clean discipline ledgers. **New question:** when does test-2's retroactive-skill-invocation gap get re-run, and test-7's failure-mode #2 get re-run with real `Skill` tool calls? Both still carry 🔁 flags.
- Who runs the out-of-session validator pass (read `Chefs-screenshots/` + per-page SSIM/pixel-match against captured `results/screenshots/test-N-*.png`) so criterion 3 stops being "unmeasured" on test-3 / test-1 re-run / test-6 re-run? **test-7 has the in-run measurement (~76% avg post iter-3) — the same pipeline could be applied retroactively to runs that captured screenshots.**
- Is the 4-vs-3 bottom-nav tab count in test-6 a PRD interpretation that should be retroactively normalized, or kept as-is to preserve negative-control authenticity?
- Is the systematic MVUX → MVVM substitution worth flagging as its own experiment axis (input-format vs default-pattern bias)? **All 7 measured runs went MVVM despite upstream MVUX — this is now a confirmed cross-test pattern across both pre-discipline and post-discipline runs.**
- Should there be a follow-up "Toolkit composites" experiment that explicitly seeds the input set with `utu:AutoLayout` / `uer:FeedView` / `utu:TabBar TopTabBarStyle` examples? The current run set proves these aren't found by any methodology when the inputs don't name them.
- The verification fetches confirm `uen:Navigation.Request` markup extension is the canonical card-tap pattern. Worth adding as an explicit anti-pattern entry: "card-tap wired via `Command`/`utu:CommandExtensions.Command` rather than `uen:Navigation.Request`" — it's the runtime-failure mode in test-1 and test-4.
- **From test-7:** is the iterative visual-diff loop's structural-debugging contribution worth its own experiment axis? Two of test-7's 17 fix passes (#3/#4 region empty + #5 TimeSpan) closed structural gaps that one-shot would have shipped silently. The loop is being evaluated for fidelity polish but its actual highest-leverage value showed up in iter-1 at the structural layer. Worth measuring as "iter-1 structural catches" separately from "iter-2+ fidelity convergence." **Iter-3's contribution was different again** — it surfaced 3 SVG-rendering / theme-propagation gaps that needed *infrastructure-level* fixes (replace SVGs with TextBlock/emoji fallbacks, move ThemeToggleService.Attach timing). Whether the loop's job ends at structural gates or extends to infrastructure workarounds is now an open methodology design question.
- **🆕 The `uno-app` MCP outage now caps criterion-3 measurement on 3 of 7 runs** (test-1 re-run, test-5, test-6 re-run). Without an out-of-session validator that can read `Chefs-screenshots/` + the captured `results/screenshots/test-N-*.png` set, criterion 3 stays "unmeasurable due to MCP" on those rows even though their underlying implementation may be sound. Worth deciding whether the experiment closes that gap before the public stunt or accepts the criterion-3 N-A.
- **🆕 First-try build success is a brittle proxy** (test-1 re-run vs original; test-6 re-run vs original). Should the headline matrix downweight the "first-try" axis for the public summary in favor of "fix-pass volume after rules-init at session start"? The latter discriminates discipline state more cleanly than the former.
- **New from test-7:** is the `IsDefault: true` + `Region.Navigator="Visibility"` empty-regions failure a Skia-desktop-only issue, or does it reproduce on WASM/Android/iOS? test-7 only walked Desktop. Worth a focused repro before the public writeup names it as a Tier-A gotcha.

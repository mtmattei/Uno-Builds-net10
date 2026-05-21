# Test 7 — Iterative visual-diff loop

**Parent spec:** `../SPEC.md` (read first for constants, scoring model, and pass bar)

**Skill-usage discipline:** see `../SPEC.md` §"Skill-usage discipline" — selection, invocation, and logging are mandatory in every measured run.

## Input

- **Any visual input from tests 1–5's reference assets** (default: the hybrid from test 5 once Figma is prepped; fall back to `../Chefs-screenshots/` while 2/5 are deferred).
- `../reference/API-CONTRACT.md` + `../reference/data/*.json` + `../reference/assets/` — shared data + bundled content layer (applies to all tests). Copy `../reference/assets/` contents into the scaffolded app's `Assets/` folder before first build so `ms-appx:///Assets/...` URIs resolve.
- **A closed feedback loop via Uno App MCP** — the defining method under test.

## Method (the variable under test)

Instead of one-shot generation, iterate:

1. Generate or refine UI.
2. Build (Desktop-Skia only during the loop for speed).
3. Uno App MCP: launch, navigate, screenshot each screen.
4. Compare each screenshot against `../Chefs-screenshots/` per variant → target mapping in `../SPEC.md`.
5. For screens under threshold, feed the diff back to the agent with a targeted correction prompt.
6. Repeat until all screens ≥ 75%.
7. Final step: full build on all five targets.

## Procedure

**Pre-session prep** must already be complete before opening Claude Code in this folder — see parent SPEC §"Per-run workflow → Phase 1". The expected pre-session state is: starter-kit files copied in, `dotnet new unoapp` scaffold present, `reference/assets/` copied into `ChefsTest7/Assets/`. If any of that is missing, abort and complete prep — do not scaffold inside the measured run (the `uno-app` MCP fails when a session opens in a folder with no `.sln`).

In the measured session:

1. Read `./SPEC.md` + `../SPEC.md`. Log start time to `../results/test-7.log`.
2. Choose and log the initial visual input (screenshots / DESIGN.md / visual-skill-output / Figma / hybrid). Log the choice prominently in `test-7.md` — the starting point is part of the result.
3. Initial implementation pass on the existing `ChefsTest7/` scaffold using the chosen input. Cover **all 20 required pages from parent SPEC §"Required screens"** — even pages the chosen input doesn't visually anchor must exist, render fixture data, and be navigable; fall back to Material defaults where needed. The diff loop will pull each page above 75% iteratively. **Do not re-run `dotnet new unoapp`.**
4. Build (Desktop-Skia is fastest for the loop). Launch the app: `dotnet run -f net10.0-desktop` (background). Confirm `uno-app` MCP attached via `uno_app_get_runtime_info`. See `CLAUDE.md` → "Using the uno-app MCP" for lifecycle.
5. Run the iteration loop described under §Method above. Cap at **10 iterations** (if convergence hasn't happened by then, that's the signal — not a time-based gate). Restart the running app between iterations as needed.
6. Log every iteration's per-screen score to `../results/test-7.log`.
7. Final full-target build (all five platforms).
8. Log outcome to `../results/test-7.md`. Stop timer.

## Forbidden inputs

Do **not** read during the run:
- `../reference/PRD.md` (test 6's input — not the intent of this test)
- `../reference/forbidden/` (quarantined source-adjacent briefs)
- Any remote Uno Chefs source

All other visual inputs (`../Chefs-screenshots/`, `../reference/DESIGN.md`, `../reference/visual-skill-output/`, Figma once prepped) are **fair game** — the whole point of test 7 is the loop, not input restriction.

## Pass criteria

See `../SPEC.md` → Pass bar + Scoring model. Every target × viewport × theme combo must independently clear 75%.

Before declaring the run done, complete every step in `../SPEC.md` §"Pre-handoff verification" and paste the per-page checklist into `../results/test-7.md`. The diff loop should be driving the per-page acceptance categories to pass; each loop iteration's pre-handoff sweep should show fewer ❌ than the previous one. Avoid every entry in `../SPEC.md` §"Anti-patterns to avoid".

Additionally record in `test-7.md`:
- Initial input chosen + rationale.
- Per-iteration delta: which screens improved, which regressed, which stayed flat.
- Iterations to convergence (or "did not converge in 10").
- Wall-clock delta vs. the single-shot version of the same input (e.g., compare to test 5 if test 5 used the same starting point).

## Notes

- This run's value is measuring whether the loop's overhead is worth it. If test 7 barely moves fidelity vs its one-shot equivalent, the loop isn't pulling its weight.
- If test 7 is the only run to clear 90%+ across all screens, that's a strong signal and the story centers on the loop.

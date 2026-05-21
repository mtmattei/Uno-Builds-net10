# Test 6 — Blind PRD (negative control)

**Parent spec:** `../SPEC.md` (read first for constants, scoring model, and pass bar)

**Skill-usage discipline:** see `../SPEC.md` §"Skill-usage discipline" — selection, invocation, and logging are mandatory in every measured run.

## Input

- `../reference/PRD.md` — the blind, implementation-agnostic PRD. **Sole specification input. No visuals of any kind.**
- `../reference/API-CONTRACT.md` + `../reference/data/*.json` + `../reference/assets/` — shared data + bundled content layer (applies to all tests). Copy `../reference/assets/` contents into the scaffolded app's `Assets/` folder before first build so `ms-appx:///Assets/...` URIs resolve.

## Procedure

**Pre-session prep** must already be complete before opening Claude Code in this folder — see parent SPEC §"Per-run workflow → Phase 1". The expected pre-session state is: starter-kit files copied in, `dotnet new unoapp` scaffold present, `reference/assets/` copied into `ChefsTest6/Assets/`. If any of that is missing, abort and complete prep — do not scaffold inside the measured run (the `uno-app` MCP fails when a session opens in a folder with no `.sln`).

In the measured session:

1. Read `./SPEC.md` + `../SPEC.md`. Log start time to `../results/test-6.log`.
2. Read `../reference/PRD.md` in full.
3. Implement **all 20 required pages from parent SPEC §"Required screens"** in the existing `ChefsTest6/` scaffold, using Uno Material defaults for all visual decisions (colors, typography, spacing, component chrome). Where the PRD describes behavior/structure, follow it; where it's silent on visuals (which is everywhere), fall back to defaults. Every required page must exist, render the fixture data, and be navigable end-to-end. **Do not re-run `dotnet new unoapp`.**
4. Build all five targets per parent SPEC.
5. Launch the app: `dotnet run -f net10.0-desktop` (background). Confirm `uno-app` MCP attached via `uno_app_get_runtime_info`. See `CLAUDE.md` → "Using the uno-app MCP" for lifecycle.
6. Screenshot via `uno_app_get_screenshot`; compare to `../Chefs-screenshots/` at validation time. **Low match expected — this is the control.**
7. Log to `../results/test-6.md` and `../results/test-6.log`. Stop timer.

## Forbidden inputs

Do **not** read during the run:
- `../Chefs-screenshots/` (visual reference — validation only, via Uno App MCP)
- `../reference/DESIGN.md`, `../reference/DESIGN-NOTES.md` (test 3's input)
- `../reference/visual-skill-output/` (test 4's input)
- `../reference/figma-url.txt` (tests 2, 5)
- `../reference/forbidden/` (quarantined source-adjacent briefs)
- Any remote Uno Chefs source

## Pass criteria

This run's **functional** pass criterion is: all 20 required pages navigable + builds on all five targets. Even as the negative control, completeness still matters — the comparison only works if scope is identical.

Visual match is **recorded but not required** for this control. The whole point of test 6 is to isolate what the visual channel contributes — the gap between test 6's visual-match score and every other test's score is the measured "visual-input lift" across the experiment.

Before declaring the run done, complete every step in `../SPEC.md` §"Pre-handoff verification" and paste the per-page checklist into `../results/test-6.md`. The 8 acceptance categories apply equally to a visual-blind run — the test is whether each page is *structurally* complete (routed, data-bound, both states, navigable, no exceptions) even when the styling is Material default. Avoid every entry in `../SPEC.md` §"Anti-patterns to avoid".

## Notes

- Do **not** weaken this control by sneaking in visual cues via the PRD. If something in the PRD feels too visually-specific, log it and propose a strip.
- `../reference/PRD.md` has been pre-stripped of source-code identifiers, framework references, and visual leakage. It should read as implementation-agnostic behavior.

# Test 1 — Screenshots only

**Parent spec:** `../SPEC.md` (read first for constants, scoring model, and pass bar)

**Skill-usage discipline:** see `../SPEC.md` §"Skill-usage discipline" — selection, invocation, and logging are mandatory in every measured run.

## Input

- `../Chefs-screenshots/` — canonical PNGs in 4 variants (`Chef App-mobile-light/`, `Chef App-mobile-dark/`, `Chef App-tablet-light/`, `Chef App-tablet-dark/`). **Sole visual input.**
- `../reference/API-CONTRACT.md` + `../reference/data/*.json` + `../reference/assets/` — shared data + bundled content layer (applies to all tests). Copy `../reference/assets/` contents into the scaffolded app's `Assets/` folder before first build so `ms-appx:///Assets/...` URIs resolve.

## Procedure

**Pre-session prep** must already be complete before opening Claude Code in this folder — see parent SPEC §"Per-run workflow → Phase 1". The expected pre-session state is: starter-kit files copied in, `dotnet new unoapp` scaffold present, `reference/assets/` copied into `ChefsTest1/Assets/`. If any of that is missing, abort and complete prep — do not scaffold inside the measured run (the `uno-app` MCP fails when a session opens in a folder with no `.sln`).

In the measured session:

1. Read `./SPEC.md` + `../SPEC.md`. Log start time to `../results/test-1.log`.
2. Load all screenshots in `../Chefs-screenshots/` (all 4 variants).
3. Infer screens, navigation graph, components, typography, color palette, spacing, and states purely from the images.
4. Implement **all 20 required pages from parent SPEC §"Required screens"** in the existing `ChefsTest1/` scaffold. Use Uno Toolkit / Uno Material where appropriate. **Do not re-run `dotnet new unoapp`.** Scope is constant across every test — even pages whose visual details aren't fully resolvable from the screenshots must exist, be navigable, and render the fixture data; fall back to Material defaults for anything ambiguous.
5. Build all five targets per parent SPEC.
6. Launch the app: `dotnet run -f net10.0-desktop` (background). Confirm the `uno-app` MCP attached via `uno_app_get_runtime_info`. See `CLAUDE.md` → "Using the uno-app MCP" for lifecycle.
7. Screenshot each implemented screen via `uno_app_get_screenshot`; compare against `../Chefs-screenshots/` per variant → target mapping.
8. Iterate on any combo < 75% match until it clears.
9. Log results to `../results/test-1.md` and `../results/test-1.log`. Stop timer.

## Forbidden inputs

Do **not** read during the run:
- `../reference/PRD.md` (test 6's input)
- `../reference/DESIGN.md`, `../reference/DESIGN-NOTES.md` (test 3's input)
- `../reference/visual-skill-output/` (test 4's input)
- `../reference/figma-url.txt` (tests 2, 5)
- `../reference/forbidden/` (quarantined source-adjacent briefs)
- Any remote Uno Chefs source (repo, docs, rendered pages)

## Pass criteria

See `../SPEC.md` → Pass bar + Scoring model. Every target × viewport × theme combo must independently clear 75%; headline score is the average across combos, with per-combo scores preserved in `test-1.md`.

Before declaring the run done, complete every step in `../SPEC.md` §"Pre-handoff verification" and paste the per-page checklist (20 rows × 10 columns) into `../results/test-1.md`. A run without a completed checklist is incomplete regardless of build state. Avoid every entry in `../SPEC.md` §"Anti-patterns to avoid" — those are real failure modes from prior runs.

## Logging

In `../results/test-1.log` append: start + end timestamps, wall-clock duration, AI turn count, first-build-try result per platform, per-screen visual-match %, manual corrections (count + brief description).

## Notes

- Where screenshots are ambiguous, make a design decision and log it in `test-1.md`. That log becomes part of the writeup's "what the screenshot-only agent had to guess at" section.
- If you find yourself wanting to consult a non-visual source, stop and log the ambiguity instead — the whole point is to measure what pixels alone deliver.

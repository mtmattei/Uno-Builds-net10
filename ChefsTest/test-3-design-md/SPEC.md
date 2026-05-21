# Test 3 — Google Stitch DESIGN.md

**Parent spec:** `../SPEC.md` (read first for constants and pass bar)

**Skill-usage discipline:** see `../SPEC.md` §"Skill-usage discipline" — selection, invocation, and logging are mandatory in every measured run.

## Input

- `../reference/DESIGN.md` — Google Stitch `DESIGN.md` (image-upload mode, 2026-04-24). **Sole visual input.** Nothing else.
- `../reference/DESIGN-NOTES.md` — provenance + known gap list. May be read to understand what is deliberately missing; do **not** treat it as a design override.
- `../reference/API-CONTRACT.md` + `../reference/data/*.json` + `../reference/assets/` — shared data + bundled content layer (applies to all tests). Copy `../reference/assets/` contents into the scaffolded app's `Assets/` folder before first build so `ms-appx:///Assets/...` URIs resolve.

## Procedure

**Pre-session prep** must already be complete before opening Claude Code in this folder — see parent SPEC §"Per-run workflow → Phase 1". The expected pre-session state is: starter-kit files copied in, `dotnet new unoapp` scaffold present, `reference/assets/` copied into `ChefsTest3/Assets/`. If any of that is missing, abort and complete prep — do not scaffold inside the measured run (the `uno-app` MCP fails when a session opens in a folder with no `.sln`).

In the measured session:

1. Read `./SPEC.md` + `../SPEC.md`. Log start time to `../results/test-3.log`.
2. Read `../reference/DESIGN.md` in full. Read `../reference/DESIGN-NOTES.md` to understand documented gaps.
3. Implement **all 20 required pages from parent SPEC §"Required screens"** in the existing `ChefsTest3/` scaffold. `DESIGN.md` only describes a subset of the design system (5 screens fed to Stitch + the inversion pattern); pages it doesn't visually anchor must still be built using Material defaults, render fixture data, and be navigable. Where the design system *is* spelled out (tokens, inversion, primary CTA, search bar, form input), follow it verbatim. Where it's silent (see DESIGN-NOTES §"Gaps"), use Uno Toolkit / Uno Material defaults — do not invent brand-specific variants. **Do not re-run `dotnet new unoapp`.**
4. Build all five targets per parent SPEC.
5. Launch the app: `dotnet run -f net10.0-desktop` (background). Confirm `uno-app` MCP attached via `uno_app_get_runtime_info`. See `CLAUDE.md` → "Using the uno-app MCP" for lifecycle.
6. Screenshot each screen via `uno_app_get_screenshot`; compare against `../Chefs-screenshots/` per variant → target mapping. **Validation only — do not read screenshots before or during building.**
7. Iterate on failing combos until pass bar clears.
8. Log results to `../results/test-3.md` and `../results/test-3.log`. Stop timer.

## Forbidden inputs

Do **not** read during the run:
- `../Chefs-screenshots/` (visual reference — validation only)
- `../reference/PRD.md` (test 6's input)
- `../reference/visual-skill-output/` (test 4's input)
- `../reference/forbidden/` (quarantined source-adjacent briefs)
- Any remote Uno Chefs source (repo, docs, rendered pages)

## Pass criteria

See `../SPEC.md` → Pass bar + Scoring model. Every target × viewport × theme combo must independently clear 75%.

Before declaring the run done, complete every step in `../SPEC.md` §"Pre-handoff verification" and paste the per-page checklist into `../results/test-3.md`. A run without a completed checklist is incomplete regardless of build state. Avoid every entry in `../SPEC.md` §"Anti-patterns to avoid" — those are real failure modes from prior runs.

## Notes

- Log every place `DESIGN.md` was ambiguous, silent, or contradicted by the final visual match. This test doubles as an evaluation of the DESIGN.md format's fitness as a cross-stack handoff.
- The hex drift (`#FF1F5A` vs `#E91E63`), the 8px vs 4px base unit, and the missing chart triad are known input gaps — log their per-screen impact separately from Claude Code's implementation quality.
- When scoring, separate "DESIGN.md input loss" from "Claude Code implementation loss" in the per-screen notes if possible.

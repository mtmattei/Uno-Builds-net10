# Test 4 — Custom visual-breakdown skill

**Parent spec:** `../SPEC.md` (read first for constants, scoring model, and pass bar)

**Skill-usage discipline:** see `../SPEC.md` §"Skill-usage discipline" — selection, invocation, and logging are mandatory in every measured run.

## Input

- `../reference/visual-skill-output/DESIGN.md` — output of the `/analyze-design` skill run against `../Chefs-screenshots/` on 2026-04-24. **Sole visual input.** 1749-line design brief covering design system + 21 ASCII screen layouts + audit.
- `../reference/API-CONTRACT.md` + `../reference/data/*.json` + `../reference/assets/` — shared data + bundled content layer (applies to all tests). Copy `../reference/assets/` contents into the scaffolded app's `Assets/` folder before first build so `ms-appx:///Assets/...` URIs resolve.

## Procedure

**Pre-session prep** must already be complete before opening Claude Code in this folder — see parent SPEC §"Per-run workflow → Phase 1". The expected pre-session state is: starter-kit files copied in, `dotnet new unoapp` scaffold present, `reference/assets/` copied into `ChefsTest4/Assets/`. If any of that is missing, abort and complete prep — do not scaffold inside the measured run (the `uno-app` MCP fails when a session opens in a folder with no `.sln`).

In the measured session:

1. Read `./SPEC.md` + `../SPEC.md`. Log start time to `../results/test-4.log`.
2. Read `../reference/visual-skill-output/DESIGN.md` in full.
3. Implement **all 20 required pages from parent SPEC §"Required screens"** in the existing `ChefsTest4/` scaffold using only the skill's output. The visual-skill DESIGN.md covers 21 ASCII layouts and most pages; for any required page it's silent on, build it using Uno Toolkit / Material defaults and render the fixture data — the page must still exist and be navigable. Where the brief is specific (tokens, component specs, ASCII layouts), follow it verbatim. Where it flags open questions or estimates (`~` prefix), use its recommended defaults. **Do not re-run `dotnet new unoapp`.**
4. Build all five targets per parent SPEC.
5. Launch the app: `dotnet run -f net10.0-desktop` (background). Confirm `uno-app` MCP attached via `uno_app_get_runtime_info`. See `CLAUDE.md` → "Using the uno-app MCP" for lifecycle.
6. Screenshot via `uno_app_get_screenshot`; compare against `../Chefs-screenshots/` per variant → target mapping. **Validation only — do not read screenshots before or during building.**
7. Iterate until pass bar clears.
8. Log to `../results/test-4.md` and `../results/test-4.log`. Stop timer.

## Forbidden inputs

Do **not** read during the run:
- `../Chefs-screenshots/` (visual reference — validation only)
- `../reference/PRD.md` (test 6's input)
- `../reference/DESIGN.md`, `../reference/DESIGN-NOTES.md` (test 3's input)
- `../reference/figma-url.txt` (tests 2, 5)
- `../reference/forbidden/` (quarantined source-adjacent briefs)
- Any remote Uno Chefs source

## Pass criteria

See `../SPEC.md` → Pass bar + Scoring model. Every target × viewport × theme combo must independently clear 75%.

Before declaring the run done, complete every step in `../SPEC.md` §"Pre-handoff verification" and paste the per-page checklist into `../results/test-4.md`. A run without a completed checklist is incomplete regardless of build state. Avoid every entry in `../SPEC.md` §"Anti-patterns to avoid".

## Notes

- The skill's output is intentionally opinionated and includes an audit with flagged violations + open questions. The run's ability to follow through on the "recommended" defaults (vs. the "observed-but-flagged" state) is part of what's being measured.
- Log which sections the agent actually consulted most — that's a signal about which parts of a visual-skill output do the heaviest lifting.

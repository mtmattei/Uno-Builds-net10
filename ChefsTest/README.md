# ChefsFT-test

Observational benchmark: how effectively different AI-input methodologies can recreate the **Uno Chefs** sample app pixel-perfect, using only Uno Platform's AI-enabled dev stack (MCPs, skills, extensions, toolkit). Seven methodologies, same scope, reported side-by-side.

The full study design, constants, scope, and pass criteria live in [SPEC.md](./SPEC.md). Per-test kickoff prompts live in [KICKOFF-PROMPTS.md](./KICKOFF-PROMPTS.md).

## Folder map

### Top-level docs
| Path | What's there |
| --- | --- |
| `SPEC.md` | Master spec — goal, experiment design, constants, methodology list, scope, pass criteria, workflow. **Start here.** |
| `KICKOFF-PROMPTS.md` | Copy-paste session prompts, one per test. Used to launch each run in a fresh Claude Code session. |
| `.gitignore` | Excludes `bin/`, `obj/`, `.vs/`, `*.user`, `.env`, etc. Also excludes the local `reference/` folder (kept off the repo intentionally). |

### Shared assets (used by every test)
| Path | What's there |
| --- | --- |
| `claude-code-uno-starter-kit/` | Mandatory starter kit. `project-level/` is copied into each run's working folder before scaffolding (`.mcp.json`, `.claude/`, `CLAUDE.md`, `docs/` placeholders). `user-level/` is copied to `~/.claude/` once per machine. |
| `Chefs-screenshots/` | Design reference — mobile + tablet, light + dark — of the original Uno Chefs app. The sole input for Test 1; combined with Figma in Test 5. |
| `.claude/settings.local.json` | Local Claude Code permissions for this folder. |

### Test folders (one per methodology)
Each `test-N-*/` is the working folder for that methodology's run. Inside you'll find the scaffolded `ChefsTestN/` solution, plus the run's `CLAUDE.md`, `.mcp.json`, and any per-test `SPEC.md` overlay.

| Path | Methodology |
| --- | --- |
| `test-1-screenshots/` | **Test 1** — input is screenshots only (no Figma, no spec). |
| `test-2-figma-mcp/` | **Test 2** — input is the Figma file via the Figma MCP. |
| `test-3-design-md/` | **Test 3** — input is a Stitch-generated `DESIGN.md` spec. |
| `test-4-visual-skill/` | **Test 4** — input is the output of the visual-skill prompt. |
| `test-5-figma-plus-screenshots/` | **Test 5** — hybrid: Figma MCP + screenshots. |
| `test-6-blind-prd/` | **Test 6** — blind PRD-driven build (no visuals). |
| `test-7-visual-diff-loop/` | **Test 7** — visual diff iteration loop. |

### `.first-run` snapshots
Earlier attempts that hit a skill-discipline failure mode (see `KICKOFF-PROMPTS.md` §"Skill-discipline pre-flight"). Preserved verbatim so the re-run can be compared against the original. **Do not read these from inside an active re-run** — they're on the forbidden list per SPEC.

| Path | Snapshot of |
| --- | --- |
| `test-1-screenshots.first-run/` | Test 1, original run |
| `test-6-blind-prd.first-run/` | Test 6, original run |
| `test-7-visual-diff-loop.first-run/` | Test 7, original run |

### Results
| Path | What's there |
| --- | --- |
| `results/` | Per-test logs (`test-N.log`, `test-N.md`), `*-learnings.md` reflections, `master-comp-matrix.md` (cross-test comparison), `matrix-presentation.html` (rendered matrix), `screenshots/` (per-page captures from each run), `fixes/` (any post-run patches). Prior-run artifacts named `*.first-run.*` or `*.prior-run.*`. |

### Not in this repo
- `reference/` — the original Uno Chefs source code, data fixtures, and design notes. Excluded from this repo by design (study constraint: agents must not read the reference source). It remains in the local working folder only.

## Conventions

- **Stack constants** (.NET version, Uno.Sdk version, app pattern, target platforms, skills enabled, MCPs enabled) — see `SPEC.md` §"Constants across all runs". These are held constant; only input modality varies.
- **App naming** — each run scaffolds `ChefsTestN` so multiple running apps are distinguishable in the OS task switcher.
- **Skill-usage discipline** — non-trivial. Read `SPEC.md` §"Skill-usage discipline" and `KICKOFF-PROMPTS.md` §"Skill-discipline pre-flight" before opening any session.

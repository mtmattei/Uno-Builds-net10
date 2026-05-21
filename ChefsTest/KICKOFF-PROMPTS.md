# Kickoff Prompts

Copy-paste the relevant block into a fresh Claude Code session opened **in the test's folder**. The pre-session prep (Phase 1) must already be complete before the session opens — see `SPEC.md` §"Per-run workflow → Phase 1".

The prompt is identical for every test except the test number. No other edits required.

## Skill-discipline pre-flight (mandatory — read before starting any run)

Tests 1, 6, and 7 all hit one of two skill-discipline failure modes. **Both invalidate the run for public reporting.** Before starting any session, the agent must internalize these:

- **Failure mode #1:** never invoking the planned skills via the `Skill` tool — the run measures generic-agent reasoning, not the Uno AI stack.
- **Failure mode #2 (the trap that looks like compliance):** writing `SKILL-USE` log lines that *describe* where a skill's domain applied, without ever making a `Skill` tool call to that skill. The log lines and the skill-usage summary table are populated, but the underlying tool calls don't exist. **This is the failure tests 1, 6, and 7 all hit.**

**The rule, in one sentence:** the `Skill` tool call comes *first*, the code comes *second*, the `SKILL-USE` log line comes *third* with a `tool-call-summary` field that references what the skill actually returned. If you can't write the `tool-call-summary`, you didn't invoke the skill — log `Invocations: 0`.

The full discipline (definition, log line format, self-audit gate) is in parent `SPEC.md §"Skill-usage discipline"`. Read it before starting any run.

---

## Test 1 — Screenshots only *(re-run for skill-usage discipline; prior run preserved at ../results/test-1.first-run.{md,log})*

```
Read ./SPEC.md and ../SPEC.md — pay close attention to ../SPEC.md
§"Skill-usage discipline" including the two named failure modes and
the self-audit gate. This is a re-run because the prior run hit
failure mode #1 (no Skill tool calls). Do NOT repeat that, and do
NOT fall into failure mode #2 (writing SKILL-USE log lines without
matching Skill tool calls). The Skill tool call comes first, the
code comes second, the SKILL-USE log line comes third with a
tool-call-summary referencing what the skill returned.

Verify pre-session prep is complete (starter-kit files in place,
ChefsTest1/ scaffold exists, reference assets copied into
ChefsTest1/Assets/). Run this test end-to-end. Log to
../results/test-1.md and ../results/test-1.log. Do not read
anything in the forbidden lists of either SPEC, including the prior
run's outputs in ../results/test-1.first-run.{md,log} and the prior
ChefsTest1 codebase under ../test-1-screenshots.first-run/. Before
declaring done, complete every step in ../SPEC.md §"Pre-handoff
verification" AND the self-audit gate at workflow step 10.5; paste
the per-page checklist, fix-pass summary, skill-usage summary, and
codebase comparison matrix into the result file.
```

---

## Test 2 — Figma MCP *(deferred — Figma URLs pending)*

```
Read ./SPEC.md and ../SPEC.md. Verify pre-session prep is complete
(starter-kit files in place, ChefsTest2/ scaffold exists, reference
assets copied into ChefsTest2/Assets/). Run this test
end-to-end. Log to ../results/test-2.md and ../results/test-2.log.
Do not read anything in the forbidden lists of either SPEC. Before
declaring done, complete every step in ../SPEC.md §"Pre-handoff
verification" and paste the per-page checklist into the result file.
```

---

## Test 3 — Stitch DESIGN.md

```
Read ./SPEC.md and ../SPEC.md. Verify pre-session prep is complete
(starter-kit files in place, ChefsTest3/ scaffold exists, reference
assets copied into ChefsTest3/Assets/). Run this test
end-to-end. Log to ../results/test-3.md and ../results/test-3.log.
Do not read anything in the forbidden lists of either SPEC. Before
declaring done, complete every step in ../SPEC.md §"Pre-handoff
verification" and paste the per-page checklist into the result file.
```

---

## Test 4 — Visual-skill output

```
Read ./SPEC.md and ../SPEC.md. Verify pre-session prep is complete
(starter-kit files in place, ChefsTest4/ scaffold exists, reference
assets copied into ChefsTest4/Assets/). Run this test
end-to-end. Log to ../results/test-4.md and ../results/test-4.log.
Do not read anything in the forbidden lists of either SPEC. Before
declaring done, complete every step in ../SPEC.md §"Pre-handoff
verification" and paste the per-page checklist into the result file.
```

---

## Test 5 — Figma + Screenshots hybrid *(deferred — Figma URLs pending)*

```
Read ./SPEC.md and ../SPEC.md. Verify pre-session prep is complete
(starter-kit files in place, ChefsTest5/ scaffold exists, reference
assets copied into ChefsTest5/Assets/). Run this test
end-to-end. Log to ../results/test-5.md and ../results/test-5.log.
Do not read anything in the forbidden lists of either SPEC. Before
declaring done, complete every step in ../SPEC.md §"Pre-handoff
verification" and paste the per-page checklist into the result file.
```

---

## Test 6 — Blind PRD *(re-run for skill-usage discipline; prior run preserved at ../results/test-6.first-run.{md,log})*

```
Read ./SPEC.md and ../SPEC.md — pay close attention to ../SPEC.md
§"Skill-usage discipline" including the two named failure modes and
the self-audit gate. This is a re-run because the prior run hit
failure mode #1 (no Skill tool calls). Do NOT repeat that, and do
NOT fall into failure mode #2 (writing SKILL-USE log lines without
matching Skill tool calls). The Skill tool call comes first, the
code comes second, the SKILL-USE log line comes third with a
tool-call-summary referencing what the skill returned.

Verify pre-session prep is complete (starter-kit files in place,
ChefsTest6/ scaffold exists, reference assets copied into
ChefsTest6/Assets/). This is the blind-PRD negative control —
do not read any visual inputs (Chefs-screenshots/, reference/DESIGN.md,
reference/visual-skill-output/, reference/forbidden/). Run the test
end-to-end. Log to ../results/test-6.md and ../results/test-6.log.
Do not read anything in the forbidden lists of either SPEC, including
the prior run's outputs in ../results/test-6.first-run.{md,log} and
the prior ChefsTest6 codebase under ../test-6-blind-prd.first-run/.
Before declaring done, complete every step in ../SPEC.md
§"Pre-handoff verification" AND the self-audit gate at workflow
step 10.5; paste the per-page checklist, fix-pass summary,
skill-usage summary, and codebase comparison matrix into the result
file.
```

---

## Test 7 — Visual diff loop *(re-run required for skill-usage failure mode #2; before re-running, rename `../results/test-7.{md,log}` → `test-7.first-run.{md,log}` and `../test-7-visual-diff-loop/` → `test-7-visual-diff-loop.first-run/`, then redo Phase 1 prep)*

```
Read ./SPEC.md and ../SPEC.md — pay close attention to ../SPEC.md
§"Skill-usage discipline" including the two named failure modes and
the self-audit gate. The first run of this test hit failure mode #2:
SKILL-USE log lines and the skill-usage summary table were filled
in for 6 skills, but only the two MCP rule-pack tools actually ran —
zero per-task Skill tool calls were made. Do NOT repeat this. The
Skill tool call comes first, the code comes second, the SKILL-USE
log line comes third with a tool-call-summary referencing what the
skill returned.

Verify pre-session prep is complete (starter-kit files in place,
ChefsTest7/ scaffold exists, reference assets copied into
ChefsTest7/Assets/). Run this test end-to-end (iterative visual-diff
loop, up to 10 iterations). Log to ../results/test-7.md and
../results/test-7.log. Do not read anything in the forbidden lists
of either SPEC, including the prior run's outputs in
../results/test-7.first-run.{md,log} and the prior ChefsTest7
codebase under ../test-7-visual-diff-loop.first-run/. Before
declaring done, complete every step in ../SPEC.md §"Pre-handoff
verification" AND the self-audit gate at workflow step 10.5; paste
the per-page checklist, fix-pass summary, skill-usage summary, and
codebase comparison matrix into the result file.
```

---

## Test 8 — Paper.design *(deferred — population method pending)*

```
Read ./SPEC.md and ../SPEC.md. Verify pre-session prep is complete
(starter-kit files in place, ChefsTest8/ scaffold exists, reference
assets copied into ChefsTest8/Assets/). Run this test
end-to-end. Log to ../results/test-8.md and ../results/test-8.log.
Do not read anything in the forbidden lists of either SPEC. Before
declaring done, complete every step in ../SPEC.md §"Pre-handoff
verification" and paste the per-page checklist into the result file.
```

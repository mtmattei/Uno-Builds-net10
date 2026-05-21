# Results

One `test-N.md` and `test-N.log` per run. Final `summary.md` after all runs complete.

## Per-run file templates

### `test-N.md`
```
# Test N — <methodology>

- Start: <ISO timestamp>
- End: <ISO timestamp>
- Wall-clock: <minutes>
- AI turns: <count>
- First-build-try (Desktop / WASM / Android / iOS / Windows): <pass/fail> × 5
- Visual match average: <%>
- Per-screen scores: [table — every target × viewport × theme combo]
- Manual corrections: <count>
- Pass bar cleared: yes / no (criterion 1 / 2 / 3 individually)

## Per-page verification checklist (REQUIRED — fill from parent SPEC §"Pre-handoff verification")

[Paste the 20-row template from parent SPEC. ✅/❌/N-A per cell. Any ❌ = page failed criterion 2.]

## Observations
- ...

## Ambiguities in the input
- (How each was resolved.)

## Anti-patterns hit (and fixed) during the run
- (Reference master SPEC §"Anti-patterns to avoid". Note any that surfaced and how you closed them.)
```

### `test-N.log`
Append-only chronological log: timestamps, milestones, build results, iteration scores.

## `summary.md`
Produced after all tests. Comparison table, recommended methodology for the public stunt, justification.

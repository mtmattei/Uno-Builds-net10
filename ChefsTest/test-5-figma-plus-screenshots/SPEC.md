# Test 5 — Figma MCP + screenshots (hybrid)

**Parent spec:** `../SPEC.md`

**Skill-usage discipline:** see `../SPEC.md` §"Skill-usage discipline" — selection, invocation, and logging are mandatory in every measured run.

## Input

Both the Figma MCP (live pull) **and** `../Chefs-screenshots/`. Everything else forbidden.

## Preconditions

- Figma MCP authenticated.
- `../reference/figma-url.txt` populated.
- `../Chefs-screenshots/` populated (done).

## Procedure

1. Pull design system (colors, typography, components) from Figma MCP.
2. Use screenshots as ground truth for any Figma-vs-reality gaps (known drift is common).
3. Scaffold: `dotnet new unoapp` with MVUX, Uno.Sdk 6.5.31, .NET 10, Desktop/WASM/Android.
4. Implement app. When Figma and screenshots disagree, screenshots win; log every such case.
5. Build all three targets.
6. Screenshot via Uno App MCP; compare to reference.
7. Iterate until pass bar clears.
8. Log to `../results/test-5.md` and `../results/test-5.log`.

## Pass criteria

See `../SPEC.md` → Pass bar.

## Notes

- This is expected to produce the highest fidelity of the single-shot methods (1–5). Compare its score delta vs. test 2 to quantify the screenshot channel's contribution.

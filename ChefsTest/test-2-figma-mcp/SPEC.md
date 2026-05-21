# Test 2 — Figma MCP

**Parent spec:** `../SPEC.md`

**Skill-usage discipline:** see `../SPEC.md` §"Skill-usage discipline" — selection, invocation, and logging are mandatory in every measured run.

## Input

Uno Chefs Figma file via the `figma` MCP server. URL in `../reference/figma-url.txt`.

## Preconditions

- Figma MCP authenticated (`mcp__figma__authenticate`).
- `figma-url.txt` populated.

## Procedure

1. Authenticate Figma MCP if not already.
2. Pull frames, components, styles, and typography via Figma MCP tools.
3. Do **not** look at `../Chefs-screenshots/` or any other reference asset.
4. Scaffold: `dotnet new unoapp` with MVUX, Uno.Sdk 6.5.31, .NET 10, Desktop/WASM/Android.
5. Translate Figma styles → Uno Material / Toolkit styles. Translate frames → pages. Wire navigation per Figma flows.
6. Build all three targets.
7. Screenshot via Uno App MCP; compare to `../Chefs-screenshots/` per the variant → target mapping (validation time only, never for design decisions).
8. Iterate until pass bar clears.
9. Log to `../results/test-2.md` and `../results/test-2.log`.

## Pass criteria

See `../SPEC.md` → Pass bar.

## Notes

- Design decisions must be justifiable from Figma content alone.
- Log every place Figma was ambiguous or missing info.

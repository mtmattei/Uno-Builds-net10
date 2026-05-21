# Project Instructions

## Overview

<!-- Brief description of what this app does -->

## Architecture

- Pattern: **MVVM** (ObservableObject ViewModels, `INotifyPropertyChanged`, `RelayCommand` from `CommunityToolkit.Mvvm`)
- Navigation: Uno Navigation (regions-based) via `INavigator`
- DI: Microsoft.Extensions.DependencyInjection via Uno.Extensions.Hosting

## Project Structure

<!-- Update this to match your actual layout -->
- `src/` — Application source
- `src/ViewModels/` — MVVM ViewModels (one per page)
- `src/Models/` — Plain data records / DTOs
- `src/Presentation/` — Pages, UserControls, value converters
- `src/Services/` — Service interfaces and implementations
- `src/Strings/en/` — Localization resources

## Conventions

- New pages get a corresponding ViewModel in `ViewModels/`. ViewModels inherit from `ObservableObject` (CommunityToolkit.Mvvm) and expose `[ObservableProperty]` fields and `[RelayCommand]` methods.
- Use `INavigator` for navigation, never frame-based.
- Prefer Uno Toolkit controls (`NavigationBar`, `TabBar`) over raw WinUI equivalents.
- Keep XAML lean — use Lightweight Styling and theme resources over inline values.
- Search Uno Platform docs via MCP before assuming API usage or patterns.

## MCPs available in this session

Configured in `./.mcp.json` and auto-loaded when the session starts in this project root:

- **`uno`** (remote — `https://mcp.platform.uno/v1`) — Uno Platform docs, API reference, usage rules, troubleshooting guides. Search and fetch. Use **before** assuming any Uno / Uno.Extensions / Uno.Toolkit / Uno.Material API behavior or naming.
- **`uno-app`** (local — `dotnet dnx -y uno.devserver --mcp-app`) — Live control of a running Uno app: navigate, screenshot, interact, assert. Use during visual-validation and automated UI testing. **Lifecycle below — read it before invoking any `uno_app_*` tool.**

### Using the `uno-app` MCP — lifecycle

The `uno-app` MCP **attaches to a running app** — it does not launch one. There is no `uno_app_start`. The available Community tools include `uno_app_get_runtime_info`, `uno_app_get_screenshot`, `uno_app_pointer_click`, and others; all of them require a live app to attach to.

Correct order of operations:

1. Confirm the project compiles: `dotnet build -f net10.0-desktop`.
2. **Launch the app in the background** via Bash: `dotnet run -f net10.0-desktop` (use `run_in_background: true`). Wait a few seconds for it to be ready.
3. Call `uno_app_get_runtime_info` to confirm the MCP has attached.
4. Now `uno_app_get_screenshot`, `uno_app_pointer_click`, and the rest are usable.
5. When the validation pass finishes, terminate the running app process.

Failure modes to recognize:

- **No `uno_app_*` tools surfaced at session start.** The MCP didn't initialize. Almost always because the folder had no `.sln` / `.slnx` when the session opened ([Uno docs: "The uno-app MCP failed to start"](https://platform.uno/docs/articles/common-issues-ai-agents.html#the-uno-app-mcp-failed-to-start)). Resolution: close the session, scaffold the project, reopen Claude Code in the same folder.
- **`uno_app_get_runtime_info` returns no app / errors.** No running app process to attach to. Run step 2 first.
- **MCP ran fine yesterday, missing today.** Usually the `dotnet dnx -y uno.devserver --mcp-app` package fetch failed silently. Try running the command manually in Bash to surface the actual error.

If a Figma MCP is also configured (via user-level `~/.claude/.mcp.json`), it's available here too — use it for design-file queries.

## Skills available in this session

Use these specialized skills instead of general-purpose search when the task matches. Invoke a skill when the work fits its description — don't route domain questions through general Uno MCP search when a specialized skill already covers the answer.

- `uno-platform-agent` — general Uno Platform questions, Single Project architecture, cross-platform patterns, MVVM/MVUX overviews.
- `uno-navigation` — region-based navigation, route registration, NavigationView / TabBar / responsive shells, qualifiers, dialogs.
- `uno-toolkit` — AutoLayout, SafeArea, Card, Chip, TabBar, NavigationBar, DrawerControl, ShadowContainer, Toolkit extensions.
- `uno-material` — Material Design 3 theming, MD3 color system, typography, Material styles and control extensions.
- `uno-csharp-markup` — C# Markup fluent API, strongly-typed bindings, resources/styles/templates in code (when using code-first UI instead of XAML).
- `uno-extensions-services` — IHostBuilder, DI container, auth (MSAL/OIDC), HttpClient, configuration, logging, storage.
- `winui-xaml` — WinUI 3 / XAML layout, binding, async, collections, rendering, memory, accessibility, localization.
- `userinterface-wiki-uno` — UI/UX best practices adapted for WinUI 3 / XAML / Uno: animations, visual states, typography, layout animation.
- `uno-app-ui-testing` — Automates UI testing for Uno Platform apps via the Uno App MCP. Use for launching, interaction, visual validation, and end-to-end tests.
- `uno-app-test-assertions` — Assertion and validation patterns for UI tests — element properties, data-binding values, screenshot comparison.

For WPF migration work specifically, `wpf-migration-assessment` and `wpf-to-uno-migration` are also available.

## Key References

Before starting any new feature or architectural decision, read these first:

- `docs/ARCHITECTURE.md` — system architecture, layers, dependencies
- `docs/DESIGN-BRIEF.md` — design language, spacing, color tokens, component patterns
- `docs/INTERACTION-SPEC.md` — state model, user flows, component states, animation inventory

## Pre-Review Cleanup

Before submitting code for review, scan for and remove dead code:

- Remove commented-out code blocks that are no longer needed.
- Remove unreferenced methods/functions that are safe to delete.
- Remove obviously unreachable or orphaned code from prior refactors.
- Leave functional code, active comments, TODOs, and intentional extension points untouched.
- If usage is uncertain, do not delete — mark with `// REVIEW: possibly unused` instead.

## Verification

```bash
dotnet build
dotnet test
dotnet run -f net10.0-desktop  # or net10.0-browserwasm, etc.
```

Always run `dotnet build` after changes to confirm the project still compiles. Run tests when they exist.

# Project Kickoff Prompt

> Copy-paste this into your first Claude Code session for a new project.
> Edit the bracketed sections before pasting.

---

I'm starting a new Uno Platform project. Before writing any code, I need you to help me set up the foundation.

## The App

- **Name:** [App name]
- **One-liner:** [What it does in one sentence]
- **Target platforms:** [desktop / web / mobile / all]
- **Pattern:** MVVM

## What I Need From You

### 1. Scaffold the project

Use `dotnet new unoapp` with the latest stable .NET SDK and Uno.Sdk. Use MVVM (ObservableObject + RelayCommand from CommunityToolkit.Mvvm). After scaffolding, run `dotnet build` to confirm it compiles.

### 2. Set up the config files

Create the following in the project root:

- `.mcp.json` — with `uno` (remote at `https://mcp.platform.uno/v1`) and `uno-app` (local via `dotnet dnx -y uno.devserver --mcp-app`)
- `.claude/settings.json` — project-level permissions scoped to .NET file types, with a `PostToolUse` hook that runs `dotnet format` on every `.cs` write
- `.claude/settings.local.json` — empty template for personal overrides
- `CLAUDE.md` at project root — with sections for Overview, Architecture, Project Structure, Conventions, Key References, and Verification

### 3. Create the docs stubs

Create a `docs/` folder with these files:

- `docs/ARCHITECTURE.md` — leave it as a placeholder, I'll fill it in after we plan the architecture
- `docs/DESIGN-BRIEF.md` — same, placeholder for design language
- `docs/INTERACTION-SPEC.md` — same, placeholder for state model, flows, and animations

### 4. Interview me

Once the scaffolding is done, interview me about:

- The core user flows (what does someone actually do in this app?)
- The data model (what entities exist, where does data come from?)
- The navigation structure (how many pages, what's the hierarchy?)
- The visual direction (dark/light, density, reference apps or sites I like)

Use my answers to draft the three docs. Don't write them all at once — do one at a time, let me review each before moving to the next.

### 5. Commit the foundation

After I've approved the docs, commit everything with a descriptive message. Then we're ready to build.

---

> **Reminder:** Search the Uno Platform docs via MCP before making assumptions about APIs or patterns. Commit after each meaningful change. If a feature is complex, write a spec first and we'll start a fresh session to implement it.

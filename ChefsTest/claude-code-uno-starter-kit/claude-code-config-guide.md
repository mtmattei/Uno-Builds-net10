# Configuring Claude Code for Real .NET Projects

Claude Code works out of the box. It also asks permission for every `dotnet build`, has no idea your project uses MVUX, and will happily scaffold a project targeting .NET 8 when you wanted the latest.

The fix isn't a better prompt. It's configuration.

---

Claude Code's setup lives across a handful of files. Once you understand what each one does, the tool starts working the way you think. What follows is how I've set things up for Uno Platform projects, though the patterns apply to any .NET codebase.

---

# `settings.json`

The rules engine. It controls what Claude Code is allowed to do: which files it can read or write, which bash commands it can run, what gets denied.

For an Uno Platform project, this means pre-approving the `dotnet` CLI and `git`, while blocking access to `.env` files, signing keys, and destructive commands. You can also wire up hooks — automated commands that fire after specific actions. I have a `PostToolUse` hook that runs `dotnet format` every time Claude writes a `.cs` file. It keeps output consistent with my `.editorconfig` without me having to ask.

It lives at `.claude/settings.json` in your project directory for project-scoped rules, or `~/.claude/settings.json` for global defaults. Commit the project one. The team shares it.

---

# `settings.local.json`

Same format, same capabilities, never committed. Claude Code auto-gitignores it. This is where machine-specific environment variables, API keys, and experimental permissions go. Things you don't want to inflict on the rest of the team.

It wins in the precedence chain. If the shared config denies something and your local config allows it, local takes priority.[^1]

---

# `CLAUDE.md`

This is the one that changes everything.

`CLAUDE.md` is a free-form markdown file that Claude Code reads at the start of every session. No schema, no enforcement mechanism. Just guidance, written in plain language, that shapes how the agent thinks.

But it needs to stay short. Claude Code wraps your `CLAUDE.md` in a system reminder that tells the model to ignore instructions that aren't relevant to the current task. As instruction count grows, Claude doesn't just ignore the new ones — it starts ignoring all of them uniformly. General consensus is under 300 lines. Shorter is better.[^2]

That means: don't put code style rules in here. That's what `.editorconfig` and `dotnet format` are for. Claude is an in-context learner. If your codebase follows a pattern, it picks it up from reading your files. Focus instead on what Claude can't infer: your stack identity, scaffolding rules, framework-level decisions like `x:Bind` over `{Binding}`, workflow expectations.

A few patterns worth including:

- Commit after each meaningful change with descriptive messages. Makes reverting easy.
- For complex features, write a spec to a markdown file first. Start a fresh session to implement from the spec. Clean context.
- At the end of a session, have Claude summarize what was done and suggest improvements to the project docs. Continuous improvement loop.[^3]

For anything that isn't universally applicable — database schemas, component patterns, design tokens — don't inline it. Point to it. A `Key References` section that says "read `docs/ARCHITECTURE.md` before starting" gives Claude the map without bloating the instruction file.

progressive disclosure: tell Claude how to find information, not all the information itself

Placement matters. `~/.claude/CLAUDE.md` applies globally — scaffolding rules go here, because when you create a new project, the project directory doesn't have config files yet. `CLAUDE.md` at the repo root applies to that specific project. Project-level overrides user-level when they conflict.

---

# `.claude/`

Not a config file. Just the folder that holds `settings.json` and `settings.local.json`. Think `.vscode/` or `.vs/`. It exists at two levels: `~/.claude/` for user-scoped, `your-repo/.claude/` for project-scoped.

---

# `.mcp.json`

This is where you wire up external capabilities through the Model Context Protocol.

For Uno Platform, two servers matter. The remote server at `mcp.platform.uno/v1` gives Claude Code access to up-to-date documentation — docs search, docs fetch, agent rules, usage best practices. The local App MCP connects to your running application and gives the agent runtime visibility: screenshots, visual tree snapshots, pointer clicks, keyboard input, element data context inspection.

The remote MCP helps you write code that follows conventions. The App MCP confirms the code actually works at runtime. Design-time knowledge vs. runtime truth.

`.mcp.json` lives at the project root, not inside `.claude/`. Commit it. When teammates clone the repo and open Claude Code, they get prompted to approve the servers once. Then everything just works.

---

# The quick reference

Every file, where it goes, whether to commit it.

**User level — `~/.claude/`**

| File | Path | Commit? | Purpose |
|------|------|---------|---------|
| `settings.json` | `~/.claude/settings.json` | N/A | Baseline permissions, global deny rules |
| `CLAUDE.md` | `~/.claude/CLAUDE.md` | N/A | Global instructions, stack identity, session workflow |

**Project level — your repo root**

| File | Path | Commit? | Purpose |
|------|------|---------|---------|
| `.mcp.json` | `my-app/.mcp.json` | Yes | MCP server registry |
| `CLAUDE.md` | `my-app/CLAUDE.md` | Yes | Project instructions, architecture, doc references |
| `settings.json` | `my-app/.claude/settings.json` | Yes | Project permissions, hooks |
| `settings.local.json` | `my-app/.claude/settings.local.json` | No | Personal overrides |

**The full structure:**

```
~/
└── .claude/
    ├── settings.json
    └── CLAUDE.md

my-app/
├── .mcp.json
├── CLAUDE.md
├── .claude/
│   ├── settings.json
│   └── settings.local.json
├── docs/
│   ├── ARCHITECTURE.md
│   ├── DESIGN-BRIEF.md
│   └── INTERACTION-SPEC.md
├── src/
└── MyApp.sln
```

---

Six files. Two user-level, four project-level. The only one you don't commit is `settings.local.json`.

`settings.json` controls permissions. `CLAUDE.md` controls behavior. `.mcp.json` controls integrations. The separation is clean. Keep `CLAUDE.md` lean. Let hooks and linters handle formatting. Point to detailed docs instead of inlining them. Let the setup improve itself over time.

Set this up once and Claude Code stops being a generic assistant that needs hand-holding every session. It becomes a tool that knows your stack.

that's the whole point

[^1]: Managed enterprise policies still override everything.
[^2]: Anthropic's own teams learned this. The Data Infrastructure team keeps theirs ruthlessly focused and iterates on it after every work session.
[^3]: Anthropic's Data Infrastructure team does exactly this — they ask Claude to summarize completed sessions and suggest documentation improvements, creating a loop that makes subsequent sessions more effective.

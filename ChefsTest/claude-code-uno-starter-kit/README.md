# Claude Code Config Starter Kit for .NET / Uno Platform

## What's Inside

```
├── claude-code-config-guide.md        ← Blog post
├── KICKOFF.md                         ← Copy-paste prompt for first session
├── README.md                          ← You are here
│
├── user-level/                        ← Copy contents to ~/.claude/
│   └── .claude/
│       ├── settings.json              ← Global permissions
│       └── CLAUDE.md                  ← Global instructions
│
└── project-level/                     ← Copy contents to your repo root
    ├── .mcp.json                      ← Uno MCP + App MCP servers
    ├── CLAUDE.md                      ← Project instructions + doc references
    ├── .claude/
    │   ├── settings.json              ← Project permissions + hooks
    │   └── settings.local.json        ← Personal overrides (gitignored)
    └── docs/
        ├── ARCHITECTURE.md            ← Placeholder — fill in your architecture
        ├── DESIGN-BRIEF.md            ← Placeholder — fill in your design language
        └── INTERACTION-SPEC.md        ← Placeholder — fill in state model, flows, animations
```

## Setup

1. Copy `user-level/.claude/` to `~/.claude/`
2. Copy the contents of `project-level/` to your Uno Platform repo root
3. Fill in the placeholder docs in `docs/`
4. Customize `CLAUDE.md` files for your specific project

## Starting a New Project

1. Open Claude Code in an empty directory
2. Copy-paste the contents of `KICKOFF.md` into your first message
3. Edit the bracketed sections (app name, description, platforms) before sending
4. Follow the interview flow — Claude will scaffold, configure, and draft your docs

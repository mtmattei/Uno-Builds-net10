# Global Instructions

## Identity

- Stack: C# / WinUI 3 / XAML / .NET / Uno Platform
- OS: Windows
- Location: Montreal (bilingual EN/FR)

## Session Workflow

1. Ask me to **name the session** before doing anything else.
2. Ask **clarifying questions** before starting any task. Confirm scope, constraints, and intent first.
3. For complex features, write a spec to a markdown file first. Then start a fresh session to implement from the spec with clean context.
4. **Commit after each meaningful change** with a descriptive conventional commit message.
5. At the end of a session, summarize what was done and suggest any improvements to project CLAUDE.md or docs.

## Scaffolding

- Always use the **latest stable .NET SDK and Uno.Sdk version**. Check Uno Platform docs or NuGet before scaffolding. Never default to an older TFM.
- Use `dotnet new unoapp` with recommended defaults. Include MVVM (CommunityToolkit.Mvvm: ObservableObject + RelayCommand) unless specified.
- Run `dotnet build` after scaffolding to confirm it compiles.

## General Behavior

- Search the Uno Platform docs (via MCP) before assuming API patterns.
- Use `x:Bind` over `{Binding}` in XAML — this is a framework-level decision, not style.
- State assumptions explicitly. Challenge flawed premises.
- Don't duplicate what linters and `.editorconfig` already enforce. Follow existing code patterns.

# Codex / AI development instructions

These instructions apply to the entire repository. Read [README.md](README.md), [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md), and [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) before making significant changes. Also read the affected application's README and project file.

## Project character

This is a **just-for-fun, nice-to-have hobby collection** of independent classic-Windows-inspired utilities. Preserve recognizable original appearance and workflows where reasonable, with sensible modernization. **Do not chase pixel-perfect replicas at the expense of maintainability.** Keep scope, dependencies, and documentation small.

## Working rules

- Respect existing implementations: `NT-Clock` targets `net8.0-windows`; `NT-FileManager` targets `net48`. Do not silently migrate frameworks or introduce a shared UI architecture.
- `NT-Write`, `NT-Cardfile`, `NT-ProgramManager`, and `NT-Clipboard` are conceptual scaffolds, not completed products. Implement features there only when explicitly requested.
- Prefer small, focused changes. Preserve unrelated behavior and code; avoid speculative refactoring or broad redesigns.
- Write readable, maintainable C#. Explain non-obvious Windows/WinForms behavior and design intent in concise comments; use XML docs for important public interfaces. Do not add comments that merely restate code.
- Treat NT-FileManager operations as potentially destructive: do not run delete, move, overwrite, or drag-and-drop tests on user files. Use disposable fixtures and deliberate error handling; never weaken safeguards casually.
- Do not add copied proprietary Microsoft code or unlicensed historical assets. Respect repository and third-party licenses.
- Keep English as the default language for repository-wide technical documentation. When changing the root project overview, update both `README.md` and `README.de.md` consistently. Preserve application-specific language where appropriate.
- Never describe illustrative images as authentic screenshots; distinguish plans from working features.

## Validation and reporting

- Inspect the relevant solution and framework before selecting build commands. Prefer Windows with the relevant Visual Studio/.NET targeting packs; do not assume Linux/WSL builds or GUI tests prove Windows compatibility.
- Test the changed functionality when the environment supports it. Run `git diff --check` and review the final diff.
- Report what changed, what was actually tested, any tests that could not run, and outstanding risks or limitations. Do not invent passing tests or claim unverified behavior.
- Follow explicit user instructions about branches and commits; do not force-push or overwrite unrelated changes.

For detail, use the linked project docs rather than duplicating policies here.

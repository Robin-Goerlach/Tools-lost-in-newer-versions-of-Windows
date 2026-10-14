# Architecture

This document describes lightweight architectural principles for **Tools Lost in Newer Versions of Windows**. It is intentionally a guide, not a framework specification.

## Purpose and boundaries

The repository hosts independent, nostalgia-inspired Windows utilities. Preserve recognizable classic Windows appearance and familiar workflows; modernize where it improves safety, compatibility, accessibility, or everyday use. Pixel-perfect reproduction is **not** required if it adds disproportionate effort or awkward implementation.

This is a **just-for-fun, nice-to-have hobby project**. No fixed release schedule or commitment to implement every idea exists.

## Applications and current foundations

| Directory | State | Technology |
| --- | --- | --- |
| `NT-Clock/` | Existing implementation | C#, Windows Forms, `net8.0-windows` |
| `NT-FileManager/` | Existing implementation | C#, Windows Forms, `net48` (.NET Framework 4.8) |
| `NT-Write/` | Conceptual scaffold, not a functional editor | See its project files |
| `NT-Cardfile/` | Conceptual scaffold, not a functional organizer | See its project files |
| `NT-ProgramManager/` | Conceptual scaffold, not a functional launcher | See its project files |
| `NT-Clipboard/` | Conceptual scaffold, not a functional clipboard viewer | See its project files |

Each application owns its solution, dependencies, settings, documentation, and releases. Do not introduce cross-application dependencies or a repository-wide solution by default.

## Implementation guidance

- Prefer built-in WinForms and operating-system capabilities to custom rendering, unless a specific classic visual element makes it worthwhile.
- Keep UI concerns, state/persistence, and filesystem or OS interactions understandable; extract helpers only when this clarifies responsibilities.
- Retain each application's existing target framework and conventions. Changing frameworks or major dependencies requires an explicit, documented reason and compatibility review.
- Preserve recognizably classic UI details where feasible; prioritize readable text, reliable keyboard use, and sensible high-DPI behavior over exact historical pixels.
- Maintain independent application startup and build paths. Share code only after demonstrable duplication warrants the cost.
- Treat screenshots and generated illustrations as visual direction, not verified screenshots of running applications.

## Safety, especially NT-FileManager

File manipulation can delete or overwrite real user data. Validate paths and operation preconditions, account for errors and permission problems, and avoid surprising destructive behavior. Prefer clear confirmation for irreversible actions; never silently broaden a file operation's scope. Tests must operate only on dedicated disposable directories with synthetic data, never on personal or production files.

Launching external processes, working with clipboard data, or storing preferences should follow least-privilege and defensive input-handling principles. Do not embed secrets or user-specific absolute paths.

## Design decisions

Favor small, reviewable changes and direct solutions. Record a short explanation in the change or relevant project README when a decision affects compatibility, persisted data, file operations, or the recognizable user experience. Avoid architecture ceremonies or an ADR directory until genuine complexity demands them.

For contribution practices and validation see [DEVELOPMENT.md](DEVELOPMENT.md). For agent instructions see [../AGENTS.md](../AGENTS.md).

# Development guide

This guide is deliberately lightweight. **Tools Lost in Newer Versions of Windows** is a just-for-fun project: correctness matters, but deadlines and unnecessary process do not.

## Language and documentation

- English is the default language for repository-wide technical documentation and agent instructions.
- The root overview is available as [README.md](../README.md) and [README.de.md](../README.de.md). Keep these two summaries aligned when their content changes.
- Application documentation may retain its existing language; add a translation when useful, rather than multiplying files routinely.
- Prefer clear English identifiers and comments in new code, while retaining existing terminology unless changing it serves an actual purpose.
- Document **current behavior** separately from ideas, prototypes, and unverified claims.
- Update only the documentation affected by a change. Avoid boilerplate documents without a reader or use case.

## Build and run

Develop and test Windows Forms applications on Windows with Visual Studio 2022 and the appropriate .NET desktop tools and targeting packs.

| Application | Solution | Framework |
| --- | --- | --- |
| NT-Clock | `NT-Clock/NtClock.sln` | .NET 8 (`net8.0-windows`) |
| NT-FileManager | `NT-FileManager/NT-FileManager.sln` | .NET Framework 4.8 (`net48`) |

Open the relevant solution in Visual Studio and build it. A possible CLI build on a suitably configured Windows machine is `dotnet build NT-Clock/NtClock.sln` for the clock; building the .NET Framework project may require Visual Studio/MSBuild and the Framework 4.8 developer pack. Do not assume Linux or WSL alone can verify WinForms behavior or reliably build `net48`.

The other application directories are conceptual scaffolds and should not be described as finished utilities. Read their local READMEs before making assumptions.

## Change workflow

1. Inspect the affected application's README, project file, and relevant source.
2. Make the smallest useful change, preserving existing behavior unless the task explicitly changes it.
3. Add concise XML documentation for public/non-obvious APIs and focused comments explaining *why* tricky code exists. Avoid comment duplication and narration of self-evident statements.
4. Build and test the affected application where the Windows toolchain is available.
5. Check `git diff --check`, review the diff, and report what was verified versus what could not be verified.
6. Update affected user or developer documentation; adjust the changelog if the application maintains one and the change is noteworthy.

Use branches or direct commits as appropriate to the user's request and repository rules; never force-push or discard unrelated changes without explicit authorization.

## Testing priorities

For every changed application: test startup, key interaction paths, settings persistence where relevant, and behavior after errors. For UI changes, manually inspect the app at realistic display scaling.

For NT-FileManager, use only disposable test directories. Cover copies/moves, renames, deletes, path navigation, drag and drop, and error cases **as applicable to the changed feature**. Check that operations cannot inadvertently affect external files. Do not claim a passing build or manual GUI test unless actually performed.

No repository-wide CI or test suite is currently established; add automation incrementally only where it provides clear value. Never turn a missing Windows environment into a claimed successful test.

## Licensing and dependencies

Honor [LICENSE](../LICENSE) and any third-party licenses. Historical software is a source of inspiration, not a blanket right to copy code, icons, fonts, or other assets. Prefer dependencies that materially improve the utility; explain major additions or framework migrations.

Related: [ARCHITECTURE.md](ARCHITECTURE.md), [AGENTS.md](../AGENTS.md).

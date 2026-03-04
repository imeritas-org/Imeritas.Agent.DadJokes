# CLAUDE.md — Imeritas.Agent.DadJokes

## What This Is

Dad jokes agent plugin

## Project Structure

```
Imeritas.Agent.DadJokes/
├── src/
│   ├── Imeritas.Agent.DadJokes/
│   │   ├── Imeritas.Agent.DadJokes.csproj
│   │   ├── Plugin/
│   │   ├── Orchestrator/
│   │   ├── Services/
│   │   └── Models/
│   └── Imeritas.Agent.DadJokes.Tests/
│       └── Imeritas.Agent.DadJokes.Tests.csproj
├── docs/
│   ├── REQUIREMENTS.md
│   ├── TODO.md
│   ├── EVOLUTION.md
│   ├── GAP_ANALYSIS.md
│   ├── CONSIDERATIONS.md
│   ├── DEPLOYMENT.md
│   └── plan-schema.json
├── Imeritas.Agent.DadJokes.slnx
├── CLAUDE.md
├── agent.yml
├── README.md
├── nuget.config
└── .gitignore
```

## Build & Test

```
dotnet build Imeritas.Agent.DadJokes.slnx
dotnet test src/Imeritas.Agent.DadJokes.Tests
```

## Key Architecture

- Follow existing framework extension patterns (reference `HttpPlugin` for multi-instance singleton, `RunbookOrchestrator` for orchestrator lifecycle)
- Plugin is singleton; orchestrator is scoped per task
- Joke data is a static embedded collection (no external API, no database)
- Use `PluginHostContext` for logging and service resolution

## Dependencies

- `Imeritas.Agent.Contracts` (NuGet from GitHub Packages)
- `Imeritas.Agent.Contracts` (NuGet from GitHub Packages)
- No additional external dependencies — jokes are embedded in code

## Core Framework Reference (READ-ONLY)

The core Imeritas.Agent framework lives at `../Imeritas.Agent/`. You MUST read it
for context when implementing features. **Do NOT modify any files under
`../Imeritas.Agent/`.**

Key files to reference:
- `../Imeritas.Agent/CLAUDE.md`
- `../Imeritas.Agent/src/Imeritas.Agent.Contracts/`
- `../Imeritas.Agent/docs/SOLUTION_DEVELOPMENT.md`

## Best Practices

- **Unit testing**: All new public methods must have unit tests. Use xUnit + NSubstitute.
- **Manual regression tests**: For any user-facing behavior change, document manual
  regression test steps in the PR description.
- **Structured logging**: Use `ILogger<T>` with parameter placeholders throughout.
  No `Console.WriteLine`.
- **Error handling**: Return structured result objects from services. Use try-catch
  with logging in orchestrators. Return error strings from plugin functions (not
  exceptions) to let the AI recover.
- **Build validation**: Run `dotnet build` and `dotnet test` before marking any
  task complete. Both must pass with 0 errors, 0 warnings.
- **Code style**: C# 12, nullable enabled, file-scoped namespaces, expression-bodied
  members where natural.
- **Security**: No hardcoded secrets. Validate all external input. Follow path
  enforcement patterns.
- **Naming**: Follow framework conventions. XML documentation on public APIs.

## Configuration

Plugin settings configured via Imeritas.Agent admin UI per tenant.

## Common Gotchas

- **Contracts namespaces**: The NuGet package is `Imeritas.Agent.Contracts` but
  namespaces are `Imeritas.Agent.*`.
- **Singleton plugin, scoped orchestrator**: Plugin is singleton — resolve tenant
  config at call time. Orchestrator is scoped.
- - Plugin `Name` must be unique across all loaded plugins
- - `DirectlyInvocableFunctions` must return at least one entry for the plugin to appear in `/api/v1/plugins`
- - Orchestrator must persist task status via `_storage.SaveTaskAsync` (see framework convention)
- - Use `inputData["_queue_task_id"]` and `inputData["_queue_session_id"]` for task ID mapping

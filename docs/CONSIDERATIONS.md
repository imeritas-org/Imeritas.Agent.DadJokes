# Considerations — Imeritas.Agent.DadJokes

Design decisions, trade-offs, and forward-looking notes.

---

## 1. Architecture Decisions

### AD-1: Static JokeService with Embedded Data

**Decision:** Jokes are hardcoded in a static `IReadOnlyList<Joke>` initialized once at class load time. No database, no external API, no file I/O.

**Rationale:** The DadJokes plugin exists primarily as a pipeline validation tool — it proves the scaffold → plan → implement → test workflow works end-to-end. External data sources would add configuration complexity, failure modes, and tenant-specific state management that distract from this purpose. The static collection is deterministic (aside from random selection), testable without infrastructure, and deploys as a zero-dependency assembly.

**Trade-off:** Adding new jokes requires recompilation and redeployment. This is acceptable for the current scope (~20 jokes) but would not scale to a user-curated joke library.

### AD-2: PluginHostContext Constructor Pattern

**Decision:** `DadJokesPlugin` accepts `PluginHostContext` as its sole constructor parameter, using `host.LoggerFactory` to create a typed logger.

**Rationale:** The framework's `ExtensionLoader` uses a two-phase detection strategy: it first tries a `PluginHostContext` constructor, then falls back to parameterless. Since DadJokes is an external extension (loaded via `ExtensionLoader`, not registered in the host's DI container), it cannot use arbitrary constructor injection. `PluginHostContext` provides curated access to `ITenantContext`, `ILoggerFactory`, `IHttpClientFactory`, and `IConfiguration` — everything an extension plugin needs without coupling to the host's internal DI registrations.

**Trade-off:** Limited to the four services exposed by `PluginHostContext`. If the plugin needed additional services (e.g., `IFileStorageService`), it would need to resolve them through `ITenantContext` or request a framework enhancement.

### AD-3: Classification Contribution Split — Plugin as Contributor

**Decision:** `DadJokesPlugin` implements `IClassificationContributor` directly on the plugin class rather than registering a separate standalone contributor or relying solely on orchestrator-contributed classification.

**Rationale:** The framework's `ClassificationMetadataCollector` aggregates from three sources: standalone contributors, orchestrators, and plugins. Co-locating classification metadata on the plugin keeps the slash command (`/joke`), classification examples, and keyword hints next to the `tell_joke` function they route to. This follows the cohesion principle — a developer reading `DadJokesPlugin.cs` sees the complete picture: what tool is exposed, how it's classified, and how users invoke it.

**Trade-off:** If a future orchestrator also needs to contribute classification for `dad_joke` tasks, there could be duplicate or conflicting metadata. The current design accepts this risk since the plugin and orchestrator share the same `"dad_joke"` task type and the framework's collision detection (`ClassificationCollisionDetector`) would flag conflicts.

### AD-4: Orchestrator DI Lifetime — Scoped per Task

**Decision:** Per the framework convention, any future `DadJokeOrchestrator` would be registered as scoped (not singleton), receiving a fresh instance per task execution.

**Rationale:** Orchestrators are inherently stateful during task execution — they track progress, hold intermediate results, and manage task lifecycle. The scoped lifetime ensures no state leaks between tasks. This contrasts with the plugin's singleton lifetime, which is appropriate because plugins are stateless tool containers that resolve tenant config at call time.

**Trade-off:** Scoped instantiation has marginally higher overhead than singleton. Irrelevant for a joke orchestrator but important context for the pattern.

---

## 2. Key Design Principles

1. **CLAUDE.md is the contract.** It defines what the agent knows about the solution.
2. **agent.yml is the boundary.** Allowed paths, build commands, and context paths are declared upfront.
3. **Issues are units of work, not units of code.** Describe capabilities, not files.
4. **Gates are cheap, rework is expensive.** Review plans before implementation.
5. **Context flows forward.** Each completed issue enriches context for subsequent issues.
6. **Build and test are non-negotiable.** Every issue must leave the build green.

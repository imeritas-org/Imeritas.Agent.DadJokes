# Future Work

Known gaps, architectural debt, and planned features for Imeritas.Agent.DadJokes.

---

## Test Coverage

### Integration Tests Needed
- End-to-end test verifying `ExtensionLoader` discovers and instantiates `DadJokesPlugin` from a compiled assembly
- Verify `tell_joke` invocation through the Plugin Actions API (`POST /api/v1/plugins/DadJokes/functions/tell_joke/invoke`)
- Classification routing test: `/joke` slash command → `dad_joke` task type → correct orchestrator

### Unit Tests Needed
- `JokeService` standalone tests (currently only tested indirectly through `DadJokesPlugin`)
- Cross-category joke validation (verify jokes tagged with multiple categories appear in all relevant lookups)
- `DadJokesSettings` validation and default value tests

---

## Architectural Debt

- **Duplicate `Joke` type** — `Services/Joke.cs` and `Models/Joke.cs` define separate `Joke` records with slightly different shapes (`IReadOnlyList<string>` vs `string[]` categories, no `Id` vs `Id`). Consolidate to a single type or clarify the layering boundary.
- **No orchestrator implementation** — `REQUIREMENTS.md` specifies a `DadJokeOrchestrator` (`ITaskOrchestrator` for task type `"dad_joke"`), but it has not been implemented. Currently, joke tasks classified as `dad_joke` would fall through to the `GenericTaskOrchestrator` fallback. This works but loses the prescribed joke-selection behavior.
- **Static `Random` without thread safety** — `JokeService` uses `static readonly Random _random = new()`. In .NET 10 `Random` is thread-safe, but if targeting older runtimes, this should use `Random.Shared` instead.

---

## v1 Features

### Joke Rating System
Allow users to rate jokes (👍/👎) via a follow-up kernel function (`rate_joke`). Store ratings per-tenant using `IFileStorageService`. Use ratings to bias random selection toward higher-rated jokes over time. Would require adding joke IDs to the service layer and a `JokeRatingStore` abstraction.

### Expanded Category Taxonomy and More Jokes
Grow the joke collection from ~20 to 100+ jokes. Add categories like "sports", "music", "holidays", "math". Consider loading jokes from an embedded JSON resource file rather than a C# initializer to make contributions easier without code changes.

### Multi-Language Support
Add a `language` parameter to `tell_joke` and store jokes with language tags. Use `IConfiguration` or tenant settings to set a default language per tenant. Start with English and Spanish. Would require restructuring the `Joke` record to include a `Language` property and updating `JokeService` filtering.

### Joke-of-the-Day Scheduled Task
Integrate with the framework's scheduling system to deliver a daily joke via chat. Register a recurring task type `"dad_joke_daily"` that the `TaskQueueProcessor` executes on a cron schedule. Would require an orchestrator that posts the joke result back through `IResponseCallback`.

### Session Joke Limit Enforcement
Wire up `DadJokesSettings.MaxJokesPerSession` — currently defined but not enforced. Track joke count per session (via `ChatSessionManager` metadata or a lightweight in-memory counter) and return a friendly "You've had enough jokes for now!" message when the limit is reached.

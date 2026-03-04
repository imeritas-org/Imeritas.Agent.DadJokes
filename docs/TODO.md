# Future Work

Known gaps, architectural debt, and planned features for Imeritas.Agent.DadJokes.

---

## Test Coverage

### Integration Tests Needed
- End-to-end test with a running Imeritas.Agent host loading the DadJokes plugin
- Verify plugin registration via `/api/v1/plugins` endpoint
- Verify orchestrator handles `dad_joke` task type through the task queue

### Unit Tests — Complete
- `JokeCollectionServiceTests` — 13 tests covering category lookup, random selection, data integrity
- `DadJokesPluginTests` — 12 tests covering metadata, `tell_joke` function, classification contributor
- `DadJokeOrchestratorTests` — 13 tests + 4 theory cases covering routing, lifecycle, persistence

---

## Architectural Debt

- **`MaxJokesPerSession` not enforced**: The `DadJokesPluginSettings.MaxJokesPerSession` setting is defined with `[PluginSetting]` but not actively enforced in `TellJoke()`. Enforcement requires reading tenant config at call time via `PluginSettingsExtension.GetPluginSettingsAsync` and tracking per-session joke count. Low priority since the setting is auto-exposed in the admin UI.
- **Thread safety of `Random`**: `JokeCollectionService` uses a static `Random` instance. In .NET 10 `Random` is thread-safe, but for older targets consider `Random.Shared`.

---

## v1 Features

- **Joke-of-the-day scheduled task**: Add a `Schedule` intent slash command (e.g. `/joke-daily`) that delivers a daily joke via the task queue.
- **User joke history**: Track which jokes have been told to avoid repeats within a session. Requires integration with `IStorageService` session data.
- **Custom joke collections**: Allow tenants to add their own jokes via the admin UI settings. Could extend `DadJokesPluginSettings` with a `CustomJokes` collection.
- **Joke rating/feedback**: Accept user ratings on jokes to prioritize better jokes. Would need a new storage schema.

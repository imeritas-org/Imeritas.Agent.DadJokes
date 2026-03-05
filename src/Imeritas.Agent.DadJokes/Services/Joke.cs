namespace Imeritas.Agent.DadJokes.Services;

/// <summary>
/// Represents a single dad joke with setup, punchline, and category tags.
/// </summary>
public sealed record Joke(string Setup, string Punchline, IReadOnlyList<string> Categories);

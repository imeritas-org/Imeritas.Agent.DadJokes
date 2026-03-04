namespace Imeritas.Agent.DadJokes.Models;

/// <summary>
/// Represents a single dad joke with category tags.
/// </summary>
public sealed record Joke(
    string Id,
    string Setup,
    string Punchline,
    IReadOnlyList<string> Categories);

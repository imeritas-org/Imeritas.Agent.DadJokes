namespace Imeritas.Agent.DadJokes.Models;

/// <summary>
/// Represents a single dad joke with category tags.
/// </summary>
/// <param name="Id">Unique identifier for the joke.</param>
/// <param name="Setup">The setup line of the joke.</param>
/// <param name="Punchline">The punchline of the joke.</param>
/// <param name="Categories">One or more category tags (e.g., "tech", "food", "animals").</param>
public record DadJoke(int Id, string Setup, string Punchline, IReadOnlyList<string> Categories)
{
    /// <summary>
    /// Returns the joke formatted as "Setup — Punchline".
    /// </summary>
    public override string ToString() => $"{Setup} — {Punchline}";
}

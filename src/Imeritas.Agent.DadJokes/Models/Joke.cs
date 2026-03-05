namespace Imeritas.Agent.DadJokes.Models;

/// <summary>
/// Represents a single dad joke with setup/punchline structure and category tags.
/// </summary>
/// <param name="Id">Unique identifier for the joke.</param>
/// <param name="Setup">The setup line of the joke (the question or premise).</param>
/// <param name="Punchline">The punchline of the joke (the answer or payoff).</param>
/// <param name="Categories">Category tags for filtering and classification (e.g., "food", "animals").</param>
public record Joke(int Id, string Setup, string Punchline, string[] Categories);

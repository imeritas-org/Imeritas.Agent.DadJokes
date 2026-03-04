using Imeritas.Agent.DadJokes.Models;

namespace Imeritas.Agent.DadJokes.Services;

/// <summary>
/// In-memory joke repository. Stateless — safe to share across threads.
/// </summary>
public sealed class JokeCollectionService
{
    private static readonly Random Random = new();

    /// <summary>Returns all jokes matching the given category (case-insensitive). Empty list if none.</summary>
    public IReadOnlyList<Joke> GetByCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return [];

        return JokeData.AllJokes
            .Where(j => j.Categories.Any(c => c.Equals(category, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>Returns a random joke from the full collection.</summary>
    public Joke GetRandom()
        => JokeData.AllJokes[Random.Next(JokeData.AllJokes.Count)];

    /// <summary>Returns all distinct category tags.</summary>
    public IReadOnlySet<string> GetAllCategories()
        => JokeData.AllJokes
            .SelectMany(j => j.Categories)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets a joke by category (random within category), falling back to any random joke.</summary>
    public Joke GetJoke(string? category)
    {
        var matches = GetByCategory(category);
        return matches.Count > 0
            ? matches[Random.Next(matches.Count)]
            : GetRandom();
    }
}

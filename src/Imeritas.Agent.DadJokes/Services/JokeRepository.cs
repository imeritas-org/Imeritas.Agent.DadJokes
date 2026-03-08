using Imeritas.Agent.DadJokes.Models;

namespace Imeritas.Agent.DadJokes.Services;

/// <summary>
/// In-memory repository of dad jokes with category-based lookup and random selection.
/// All joke data is static and embedded — no external dependencies.
/// </summary>
public class JokeRepository
{
    private static readonly List<DadJoke> AllJokes = new()
    {
        new(1, "Why do programmers prefer dark mode?", "Because light attracts bugs.", new[] { "tech" }),
        new(2, "What do you call a fake noodle?", "An impasta.", new[] { "food" }),
        new(3, "Why don't eggs tell jokes?", "They'd crack each other up.", new[] { "food" }),
        new(4, "What do you call a bear with no teeth?", "A gummy bear.", new[] { "animals" }),
        new(5, "Why did the scarecrow win an award?", "He was outstanding in his field.", new[] { "work", "general" }),
        new(6, "I'm reading a book about anti-gravity.", "It's impossible to put down.", new[] { "science" }),
        new(7, "Why don't scientists trust atoms?", "Because they make up everything.", new[] { "science" }),
        new(8, "What do you call a dog that does magic?", "A Labracadabrador.", new[] { "animals" }),
        new(9, "Why did the coffee file a police report?", "It got mugged.", new[] { "food", "work" }),
        new(10, "How does a penguin build its house?", "Igloos it together.", new[] { "animals" }),
        new(11, "Why did the developer go broke?", "Because he used up all his cache.", new[] { "tech" }),
        new(12, "What do you call a sleeping dinosaur?", "A dino-snore.", new[] { "animals" }),
        new(13, "Why did the math book look sad?", "It had too many problems.", new[] { "science", "general" }),
        new(14, "What do you call cheese that isn't yours?", "Nacho cheese.", new[] { "food" }),
        new(15, "Why can't a bicycle stand on its own?", "Because it's two-tired.", new[] { "general" }),
        new(16, "What did the ocean say to the beach?", "Nothing, it just waved.", new[] { "general" }),
        new(17, "Why do Java developers wear glasses?", "Because they can't C#.", new[] { "tech" }),
        new(18, "What did the grape do when it got stepped on?", "Nothing, it just let out a little wine.", new[] { "food", "general" }),
        new(19, "Why did the IT guy go to the doctor?", "He had a virus.", new[] { "tech", "work" }),
        new(20, "What do you call a factory that makes okay products?", "A satisfactory.", new[] { "work", "general" }),
    };

    private static readonly IReadOnlyList<string> AllCategories = AllJokes
        .SelectMany(j => j.Categories)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
        .ToList();

    /// <summary>
    /// Gets all available joke category names.
    /// </summary>
    public IReadOnlyList<string> GetCategories() => AllCategories;

    /// <summary>
    /// Gets all jokes matching the specified category (case-insensitive).
    /// Returns an empty list if the category has no matches.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    public IReadOnlyList<DadJoke> GetByCategory(string category)
        => AllJokes
            .Where(j => j.Categories.Any(c => c.Equals(category, StringComparison.OrdinalIgnoreCase)))
            .ToList();

    /// <summary>
    /// Gets a random joke from the full collection.
    /// </summary>
    public DadJoke GetRandom() => AllJokes[Random.Shared.Next(AllJokes.Count)];

    /// <summary>
    /// Gets a random joke matching the specified category (case-insensitive).
    /// Falls back to a random joke from the full collection if the category has no matches.
    /// </summary>
    /// <param name="category">The category to filter by, or null/empty for a random joke.</param>
    public DadJoke GetRandomByCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return GetRandom();

        var matches = GetByCategory(category);
        return matches.Count > 0
            ? matches[Random.Shared.Next(matches.Count)]
            : GetRandom();
    }

    /// <summary>
    /// Gets the total number of jokes in the repository.
    /// </summary>
    public int Count => AllJokes.Count;
}

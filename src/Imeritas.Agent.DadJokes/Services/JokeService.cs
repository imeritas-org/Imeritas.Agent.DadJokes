namespace Imeritas.Agent.DadJokes.Services;

/// <summary>
/// In-memory joke repository containing embedded dad jokes tagged with categories.
/// Stateless — no constructor dependencies — can be instantiated directly.
/// </summary>
public sealed class JokeService
{
    private static readonly IReadOnlyList<Joke> _jokes = InitializeJokes();
    private static readonly Random _random = new();

    /// <summary>Total number of jokes in the collection.</summary>
    public int Count => _jokes.Count;

    /// <summary>
    /// Returns all jokes matching the given category (case-insensitive).
    /// Returns an empty list for null, empty, or unknown categories.
    /// </summary>
    public IReadOnlyList<Joke> GetByCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return [];

        return _jokes
            .Where(j => j.Categories.Any(c => c.Equals(category, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>
    /// Returns a random joke from the entire collection.
    /// </summary>
    public Joke GetRandom()
    {
        return _jokes[_random.Next(_jokes.Count)];
    }

    /// <summary>
    /// Returns a random joke from the given category.
    /// Falls back to a random joke from the entire collection if the category
    /// is null, empty, or has no matching jokes.
    /// </summary>
    public Joke GetRandomByCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return GetRandom();

        var matches = GetByCategory(category);
        return matches.Count > 0
            ? matches[_random.Next(matches.Count)]
            : GetRandom();
    }

    /// <summary>
    /// Returns a sorted, distinct list of all category names across all jokes.
    /// </summary>
    public IReadOnlyList<string> GetCategories()
    {
        return _jokes
            .SelectMany(j => j.Categories)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<Joke> InitializeJokes()
    {
        return
        [
            // Tech (4 jokes)
            new Joke("Why do programmers prefer dark mode?", "Because light attracts bugs.", ["tech"]),
            new Joke("What's a computer's favorite snack?", "Microchips.", ["tech", "food"]),
            new Joke("Why was the JavaScript developer sad?", "Because he didn't Node how to Express himself.", ["tech"]),
            new Joke("How do trees access the internet?", "They log in.", ["tech", "science"]),

            // Food (4 jokes)
            new Joke("Why did the coffee file a police report?", "It got mugged.", ["food"]),
            new Joke("What do you call a fake noodle?", "An impasta.", ["food"]),
            new Joke("Why don't eggs tell jokes?", "They'd crack each other up.", ["food"]),
            new Joke("What did the grape do when it got stepped on?", "Nothing, it just let out a little wine.", ["food"]),

            // Animals (4 jokes)
            new Joke("What do you call an alligator in a vest?", "An investigator.", ["animals"]),
            new Joke("Why don't oysters share their pearls?", "Because they're shellfish.", ["animals"]),
            new Joke("What do you call a bear with no teeth?", "A gummy bear.", ["animals"]),
            new Joke("Why do cows wear bells?", "Because their horns don't work.", ["animals"]),

            // Work (3 jokes)
            new Joke("Why did the scarecrow win an award?", "Because he was outstanding in his field.", ["work"]),
            new Joke("I used to hate facial hair, but then it grew on me.", "That's the spirit of perseverance at work.", ["work"]),
            new Joke("Why did the employee get fired from the calendar factory?", "He took a day off.", ["work"]),

            // Science (3 jokes)
            new Joke("Why can't you trust atoms?", "Because they make up everything.", ["science"]),
            new Joke("What did the ocean say to the beach?", "Nothing, it just waved.", ["science"]),
            new Joke("Why did the biologist break up with the physicist?", "There was no chemistry.", ["science"]),

            // General (2 jokes) — also cross-tagged
            new Joke("I'm reading a book about anti-gravity.", "It's impossible to put down.", ["general", "science"]),
            new Joke("What did the janitor say when he jumped out of the closet?", "Supplies!", ["general", "work"]),
        ];
    }
}

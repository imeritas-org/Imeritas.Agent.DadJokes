namespace Imeritas.Agent.DadJokes.Services;

using Imeritas.Agent.DadJokes.Models;

public class JokeService : IJokeService
{
    private readonly IReadOnlyList<DadJoke> _jokes = JokeData.All;
    private readonly Random _random = new();

    public List<DadJoke> GetByCategory(string category) =>
        _jokes.Where(j => j.Categories.Contains(category, StringComparer.OrdinalIgnoreCase))
              .ToList();

    public DadJoke GetRandom() =>
        _jokes[_random.Next(_jokes.Count)];

    public List<string> GetAllCategories() =>
        _jokes.SelectMany(j => j.Categories)
              .Distinct(StringComparer.OrdinalIgnoreCase)
              .Order(StringComparer.OrdinalIgnoreCase)
              .ToList();
}

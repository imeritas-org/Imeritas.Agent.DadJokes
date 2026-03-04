using Imeritas.Agent.DadJokes.Models;
using Imeritas.Agent.DadJokes.Services;
using Xunit;

namespace Imeritas.Agent.DadJokes.Tests.Services;

public class JokeCollectionServiceTests
{
    private readonly JokeCollectionService _sut = new();

    [Fact]
    public void GetByCategory_ValidCategory_ReturnsMatchingJokes()
    {
        var jokes = _sut.GetByCategory("tech");

        Assert.NotEmpty(jokes);
        Assert.All(jokes, j =>
            Assert.Contains("tech", j.Categories, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetByCategory_UnknownCategory_ReturnsEmptyList()
    {
        var jokes = _sut.GetByCategory("nonexistent");

        Assert.Empty(jokes);
    }

    [Theory]
    [InlineData("TECH")]
    [InlineData("Tech")]
    [InlineData("tEcH")]
    public void GetByCategory_IsCaseInsensitive(string category)
    {
        var jokes = _sut.GetByCategory(category);

        Assert.NotEmpty(jokes);
        Assert.All(jokes, j =>
            Assert.Contains("tech", j.Categories, StringComparer.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetByCategory_NullOrEmpty_ReturnsEmptyList(string? category)
    {
        var jokes = _sut.GetByCategory(category);

        Assert.Empty(jokes);
    }

    [Fact]
    public void GetRandom_ReturnsNonNullJoke()
    {
        var joke = _sut.GetRandom();

        Assert.NotNull(joke);
        Assert.False(string.IsNullOrWhiteSpace(joke.Setup));
        Assert.False(string.IsNullOrWhiteSpace(joke.Punchline));
    }

    [Fact]
    public void GetAllCategories_ReturnsAllDistinctCategories()
    {
        var categories = _sut.GetAllCategories();

        var expectedCategories = new[]
        {
            "tech", "food", "animals", "work",
            "science", "sports", "music", "general"
        };

        foreach (var expected in expectedCategories)
        {
            Assert.Contains(expected, categories, StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void GetJoke_WithValidCategory_ReturnsMatchingJoke()
    {
        var joke = _sut.GetJoke("food");

        Assert.NotNull(joke);
        Assert.Contains("food", joke.Categories, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetJoke_WithUnknownCategory_ReturnsFallbackJoke()
    {
        var joke = _sut.GetJoke("nonexistent");

        Assert.NotNull(joke);
        Assert.False(string.IsNullOrWhiteSpace(joke.Setup));
        Assert.False(string.IsNullOrWhiteSpace(joke.Punchline));
    }

    [Fact]
    public void GetJoke_WithNull_ReturnsFallbackJoke()
    {
        var joke = _sut.GetJoke(null);

        Assert.NotNull(joke);
        Assert.False(string.IsNullOrWhiteSpace(joke.Setup));
    }

    [Fact]
    public void JokeCollection_ContainsExpectedCount()
    {
        var allCategories = _sut.GetAllCategories();
        Assert.Equal(8, allCategories.Count);

        var allJokeIds = new HashSet<string>();
        foreach (var category in allCategories)
        {
            var jokes = _sut.GetByCategory(category);
            foreach (var joke in jokes)
            {
                allJokeIds.Add(joke.Id);
            }
        }

        Assert.Equal(20, allJokeIds.Count);
    }

    [Fact]
    public void AllJokes_HaveValidData()
    {
        var allCategories = _sut.GetAllCategories();
        var allJokes = new Dictionary<string, Joke>();

        foreach (var category in allCategories)
        {
            foreach (var joke in _sut.GetByCategory(category))
            {
                allJokes.TryAdd(joke.Id, joke);
            }
        }

        Assert.Equal(20, allJokes.Count);

        foreach (var joke in allJokes.Values)
        {
            Assert.False(string.IsNullOrWhiteSpace(joke.Id), $"Joke has empty Id");
            Assert.False(string.IsNullOrWhiteSpace(joke.Setup), $"Joke {joke.Id} has empty Setup");
            Assert.False(string.IsNullOrWhiteSpace(joke.Punchline), $"Joke {joke.Id} has empty Punchline");
            Assert.NotEmpty(joke.Categories);
            Assert.InRange(joke.Categories.Count, 1, 3);
        }
    }

    [Fact]
    public void EachCategory_HasAtLeastTwoJokes()
    {
        var allCategories = _sut.GetAllCategories();

        foreach (var category in allCategories)
        {
            var jokes = _sut.GetByCategory(category);
            Assert.True(jokes.Count >= 2,
                $"Category '{category}' has only {jokes.Count} joke(s), expected at least 2");
        }
    }
}

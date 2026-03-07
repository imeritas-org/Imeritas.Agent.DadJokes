using Imeritas.Agent.DadJokes.Models;
using Imeritas.Agent.DadJokes.Services;
using Xunit;

namespace Imeritas.Agent.DadJokes.Tests.Services;

public class JokeServiceTests
{
    // --- GetByCategory tests ---

    [Fact]
    public void GetByCategory_ValidCategory_ReturnsOnlyMatchingJokes()
    {
        var service = new JokeService();
        var knownCategory = JokeData.All.First().Categories.First();

        var results = service.GetByCategory(knownCategory);

        Assert.NotEmpty(results);
        Assert.All(results, joke =>
            Assert.Contains(knownCategory, joke.Categories, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetByCategory_DifferentCase_ReturnsSameResults()
    {
        var service = new JokeService();
        var category = JokeData.All.First().Categories.First();
        var upper = category.ToUpperInvariant();
        var lower = category.ToLowerInvariant();

        var upperResults = service.GetByCategory(upper);
        var lowerResults = service.GetByCategory(lower);

        Assert.NotEmpty(upperResults);
        Assert.Equal(upperResults.Count, lowerResults.Count);
        Assert.Equal(
            upperResults.Select(j => j.Id).OrderBy(id => id),
            lowerResults.Select(j => j.Id).OrderBy(id => id));
    }

    [Fact]
    public void GetByCategory_UnknownCategory_ReturnsEmptyList()
    {
        var service = new JokeService();

        var results = service.GetByCategory("nonexistent-category-xyz");

        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetByCategory_NullOrEmpty_ReturnsEmptyList(string? category)
    {
        var service = new JokeService();

        var results = service.GetByCategory(category!);

        Assert.NotNull(results);
        Assert.Empty(results);
    }

    // --- GetRandom tests ---

    [Fact]
    public void GetRandom_ReturnsNonNullJokeFromCollection()
    {
        var service = new JokeService();

        var joke = service.GetRandom();

        Assert.NotNull(joke);
        Assert.Contains(joke, JokeData.All);
    }

    // --- GetAllCategories tests ---

    [Fact]
    public void GetAllCategories_ReturnsDistinctSortedCategories()
    {
        var service = new JokeService();

        var categories = service.GetAllCategories();

        Assert.NotEmpty(categories);
        // Verify distinct
        Assert.Equal(categories.Count, categories.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        // Verify sorted
        var sorted = categories.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
        Assert.Equal(sorted, categories);
    }

    // --- JokeData integrity tests ---

    [Fact]
    public void JokeDataAll_ContainsAtLeast20Jokes()
    {
        Assert.True(JokeData.All.Count >= 20,
            $"Expected at least 20 jokes but found {JokeData.All.Count}");
    }

    [Fact]
    public void JokeDataAll_AllJokesHaveRequiredFields()
    {
        Assert.All(JokeData.All, joke =>
        {
            Assert.False(string.IsNullOrWhiteSpace(joke.Setup),
                $"Joke {joke.Id} has empty Setup");
            Assert.False(string.IsNullOrWhiteSpace(joke.Punchline),
                $"Joke {joke.Id} has empty Punchline");
            Assert.NotEmpty(joke.Categories);
        });
    }

    [Fact]
    public void JokeDataAll_AllJokesHaveUniqueIds()
    {
        var ids = JokeData.All.Select(j => j.Id).ToList();

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}

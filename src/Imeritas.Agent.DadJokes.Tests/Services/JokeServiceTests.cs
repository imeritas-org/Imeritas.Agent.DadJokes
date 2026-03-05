using Imeritas.Agent.DadJokes.Services;
using Xunit;

namespace Imeritas.Agent.DadJokes.Tests.Services;

/// <summary>
/// Unit tests for <see cref="JokeService"/> — a stateless, in-memory joke repository.
/// </summary>
public class JokeServiceTests
{
    private readonly JokeService _sut = new();

    // ── GetByCategory ─────────────────────────────────────────────────────

    [Fact]
    public void GetByCategory_ValidCategory_ReturnsMatchingJokes()
    {
        var result = _sut.GetByCategory("food");

        Assert.NotEmpty(result);
        Assert.All(result, joke =>
            Assert.Contains("food", joke.Categories, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetByCategory_UnknownCategory_ReturnsEmptyList()
    {
        var result = _sut.GetByCategory("nonexistent");

        Assert.Empty(result);
    }

    [Fact]
    public void GetByCategory_NullCategory_ReturnsEmptyList()
    {
        var result = _sut.GetByCategory(null!);

        Assert.Empty(result);
    }

    [Fact]
    public void GetByCategory_EmptyString_ReturnsEmptyList()
    {
        Assert.Empty(_sut.GetByCategory(""));
        Assert.Empty(_sut.GetByCategory("  "));
    }

    [Fact]
    public void GetByCategory_CaseInsensitive_ReturnsSameResults()
    {
        var upper = _sut.GetByCategory("TECH");
        var lower = _sut.GetByCategory("tech");

        Assert.Equal(upper.Count, lower.Count);
    }

    // ── GetRandom ─────────────────────────────────────────────────────────

    [Fact]
    public void GetRandom_ReturnsNonNullJoke()
    {
        var joke = _sut.GetRandom();

        Assert.NotNull(joke);
        Assert.False(string.IsNullOrWhiteSpace(joke.Setup));
    }

    // ── GetRandomByCategory ───────────────────────────────────────────────

    [Fact]
    public void GetRandomByCategory_ValidCategory_ReturnsJokeFromCategory()
    {
        var joke = _sut.GetRandomByCategory("animals");

        Assert.NotNull(joke);
        Assert.Contains("animals", joke.Categories, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetRandomByCategory_NullCategory_FallsBackToRandom()
    {
        var joke = _sut.GetRandomByCategory(null);

        Assert.NotNull(joke);
    }

    [Fact]
    public void GetRandomByCategory_UnknownCategory_FallsBackToRandom()
    {
        var joke = _sut.GetRandomByCategory("nonexistent");

        Assert.NotNull(joke);
    }

    // ── GetCategories ─────────────────────────────────────────────────────

    [Fact]
    public void GetCategories_ReturnsSortedDistinctList()
    {
        var result = _sut.GetCategories();

        Assert.True(result.Count >= 6, $"Expected at least 6 categories but got {result.Count}");
        Assert.Contains("animals", result, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("food", result, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("general", result, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("science", result, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("tech", result, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("work", result, StringComparer.OrdinalIgnoreCase);

        // Verify alphabetical sort order
        var sorted = result.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
        Assert.Equal(sorted, result);
    }

    // ── Count ─────────────────────────────────────────────────────────────

    [Fact]
    public void Count_ReturnsExpectedTotal()
    {
        Assert.Equal(20, _sut.Count);
    }
}

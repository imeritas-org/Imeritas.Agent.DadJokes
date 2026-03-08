using Imeritas.Agent.DadJokes.Services;
using Xunit;

namespace Imeritas.Agent.DadJokes.Tests.Services;

public class JokeRepositoryTests
{
    private readonly JokeRepository _sut = new();

    // ── GetRandom ──

    [Fact]
    public void GetRandom_ReturnsAJoke()
    {
        var joke = _sut.GetRandom();
        Assert.NotNull(joke);
        Assert.False(string.IsNullOrWhiteSpace(joke.Setup));
        Assert.False(string.IsNullOrWhiteSpace(joke.Punchline));
    }

    // ── GetByCategory ──

    [Theory]
    [InlineData("tech")]
    [InlineData("food")]
    [InlineData("animals")]
    [InlineData("work")]
    [InlineData("science")]
    [InlineData("general")]
    public void GetByCategory_KnownCategory_ReturnsNonEmptyList(string category)
    {
        var jokes = _sut.GetByCategory(category);
        Assert.NotEmpty(jokes);
        Assert.All(jokes, j =>
            Assert.Contains(j.Categories, c => c.Equals(category, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void GetByCategory_UnknownCategory_ReturnsEmptyList()
    {
        var jokes = _sut.GetByCategory("nonexistent");
        Assert.Empty(jokes);
    }

    [Fact]
    public void GetByCategory_IsCaseInsensitive()
    {
        var lower = _sut.GetByCategory("tech");
        var upper = _sut.GetByCategory("TECH");
        Assert.Equal(lower.Count, upper.Count);
    }

    // ── GetRandomByCategory ──

    [Fact]
    public void GetRandomByCategory_ValidCategory_ReturnsMatchingJoke()
    {
        var joke = _sut.GetRandomByCategory("tech");
        Assert.Contains(joke.Categories, c => c.Equals("tech", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetRandomByCategory_NullCategory_ReturnsFallbackJoke()
    {
        var joke = _sut.GetRandomByCategory(null);
        Assert.NotNull(joke);
    }

    [Fact]
    public void GetRandomByCategory_EmptyCategory_ReturnsFallbackJoke()
    {
        var joke = _sut.GetRandomByCategory("");
        Assert.NotNull(joke);
    }

    [Fact]
    public void GetRandomByCategory_UnknownCategory_ReturnsFallbackJoke()
    {
        var joke = _sut.GetRandomByCategory("nonexistent");
        Assert.NotNull(joke);
    }

    // ── GetCategories ──

    [Fact]
    public void GetCategories_ReturnsAllKnownCategories()
    {
        var categories = _sut.GetCategories();
        Assert.Contains("tech", categories);
        Assert.Contains("food", categories);
        Assert.Contains("animals", categories);
        Assert.Contains("work", categories);
        Assert.Contains("science", categories);
        Assert.Contains("general", categories);
    }

    // ── Count ──

    [Fact]
    public void Count_ReturnsAtLeast20()
    {
        Assert.True(_sut.Count >= 20);
    }

    // ── DadJoke.ToString ──

    [Fact]
    public void DadJoke_ToString_FormatsCorrectly()
    {
        var joke = _sut.GetRandom();
        var str = joke.ToString();
        Assert.Contains(joke.Setup, str);
        Assert.Contains(joke.Punchline, str);
        Assert.Contains("\u2014", str);
    }
}

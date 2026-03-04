using Imeritas.Agent.DadJokes.Plugin;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Imeritas.Agent.DadJokes.Tests.Plugin;

public class DadJokesPluginTests
{
    private readonly DadJokesPlugin _sut;

    public DadJokesPluginTests()
    {
        var logger = Substitute.For<ILogger<DadJokesPlugin>>();
        _sut = new DadJokesPlugin(logger);
    }

    [Fact]
    public void Name_ReturnsDadJokes()
    {
        Assert.Equal("DadJokes", _sut.Name);
    }

    [Fact]
    public void DisplayName_ReturnsDadJokes()
    {
        Assert.Equal("Dad Jokes", _sut.DisplayName);
    }

    [Fact]
    public void Description_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(_sut.Description));
    }

    [Fact]
    public void TellJoke_WithNullCategory_ReturnsFormattedJoke()
    {
        var result = _sut.TellJoke(null);

        Assert.NotNull(result);
        Assert.Contains("\n", result);
        var parts = result.Split('\n', 2);
        Assert.Equal(2, parts.Length);
        Assert.False(string.IsNullOrWhiteSpace(parts[0]));
        Assert.False(string.IsNullOrWhiteSpace(parts[1]));
    }

    [Fact]
    public void TellJoke_WithValidCategory_ReturnsJokeFromCategory()
    {
        var result = _sut.TellJoke("tech");

        Assert.NotNull(result);
        Assert.Contains("\n", result);
    }

    [Fact]
    public void TellJoke_WithUnknownCategory_ReturnsRandomJoke()
    {
        var result = _sut.TellJoke("nonexistent");

        Assert.NotNull(result);
        Assert.Contains("\n", result);
        var parts = result.Split('\n', 2);
        Assert.Equal(2, parts.Length);
        Assert.False(string.IsNullOrWhiteSpace(parts[0]));
    }

    [Fact]
    public void DirectlyInvocableFunctions_ContainsTellJoke()
    {
        Assert.Contains("tell_joke", _sut.DirectlyInvocableFunctions);
    }

    [Fact]
    public async Task GetSystemPromptContributionAsync_ReturnsNonNullNonEmpty()
    {
        var result = await _sut.GetSystemPromptContributionAsync("tenant1");

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void ClassificationExamples_IsNonEmpty()
    {
        Assert.NotEmpty(_sut.ClassificationExamples);
        Assert.True(_sut.ClassificationExamples.Count >= 2);
    }

    [Fact]
    public void ClassificationHints_TaskType_EqualsDadJoke()
    {
        Assert.Equal("dad_joke", _sut.ClassificationHints.TaskType);
    }

    [Fact]
    public void ClassificationHints_SourceName_EqualsDadJokes()
    {
        Assert.Equal("DadJokes", _sut.ClassificationHints.SourceName);
    }

    [Fact]
    public void SlashCommands_ContainsJokeCommand()
    {
        Assert.NotEmpty(_sut.SlashCommands);
        Assert.Contains(_sut.SlashCommands, cmd => cmd.Command == "/joke");
    }

    [Fact]
    public void SlashCommands_JokeCommand_MapsToTaskDadJoke()
    {
        var jokeCmd = Assert.Single(_sut.SlashCommands, cmd => cmd.Command == "/joke");

        Assert.Equal("dad_joke", jokeCmd.TaskType);
        Assert.Equal(Imeritas.Agent.Models.IntentType.Task, jokeCmd.Intent);
    }
}

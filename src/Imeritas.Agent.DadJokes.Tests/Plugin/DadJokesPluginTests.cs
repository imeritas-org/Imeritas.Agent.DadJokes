using Imeritas.Agent.DadJokes.Plugin;
using Imeritas.Agent.DadJokes.Services;
using Imeritas.Agent.Extensions;
using Imeritas.Agent.Models;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Imeritas.Agent.DadJokes.Tests.Plugin;

public class DadJokesPluginTests
{
    private readonly DadJokesPlugin _sut;

    public DadJokesPluginTests()
    {
        var tenantContext = Substitute.For<ITenantContext>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var configuration = Substitute.For<IConfiguration>();

        var host = new PluginHostContext(tenantContext, loggerFactory, httpClientFactory, configuration);
        _sut = new DadJokesPlugin(host);
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
    public void DirectlyInvocableFunctions_ContainsTellJoke()
    {
        Assert.Contains("tell_joke", _sut.DirectlyInvocableFunctions);
    }

    [Fact]
    public async Task TellJokeAsync_NoCategory_ReturnsJoke()
    {
        var result = await _sut.TellJokeAsync();
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.Contains("\u2014", result); // joke format: Setup — Punchline
    }

    [Fact]
    public async Task TellJokeAsync_WithCategory_ReturnsJoke()
    {
        var result = await _sut.TellJokeAsync("tech");
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.Contains("\u2014", result);
    }

    [Fact]
    public async Task TellJokeAsync_UnknownCategory_ReturnsFallbackJoke()
    {
        var result = await _sut.TellJokeAsync("nonexistent");
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public async Task GetSystemPromptContributionAsync_ReturnsNonNullPrompt()
    {
        var result = await _sut.GetSystemPromptContributionAsync("tenant1");
        Assert.NotNull(result);
        Assert.Contains("Dad Jokes", result);
        Assert.Contains("tell_joke", result);
    }

    // ── IClassificationContributor ──

    [Fact]
    public void SlashCommands_ContainsJokeCommand()
    {
        Assert.Single(_sut.SlashCommands);
        Assert.Equal("/joke", _sut.SlashCommands[0].Command);
        Assert.Equal(IntentType.Task, _sut.SlashCommands[0].Intent);
        Assert.Equal("dad_joke", _sut.SlashCommands[0].TaskType);
    }

    [Fact]
    public void ClassificationExamples_ReturnsExamples()
    {
        Assert.NotEmpty(_sut.ClassificationExamples);
        Assert.All(_sut.ClassificationExamples, ex =>
            Assert.Equal("[task:dad_joke]", ex.ClassificationPrefix));
    }

    [Fact]
    public void ClassificationHints_TargetsDadJokeTaskType()
    {
        Assert.Equal("dad_joke", _sut.ClassificationHints.TaskType);
        Assert.Contains("joke", _sut.ClassificationHints.TaskTypeKeywords);
    }
}

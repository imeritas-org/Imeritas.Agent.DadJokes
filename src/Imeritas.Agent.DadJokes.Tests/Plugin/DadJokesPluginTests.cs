using System.Reflection;
using Imeritas.Agent.DadJokes.Plugin;
using Imeritas.Agent.Extensions;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Imeritas.Agent.DadJokes.Tests.Plugin;

/// <summary>
/// Unit tests for <see cref="DadJokesPlugin"/> covering identity properties,
/// kernel function behavior, system prompt contribution, and classification metadata.
/// </summary>
public class DadJokesPluginTests
{
    private readonly DadJokesPlugin _plugin;

    public DadJokesPluginTests()
    {
        var tenantContext = Substitute.For<ITenantContext>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var configuration = Substitute.For<IConfiguration>();

        var host = new PluginHostContext(tenantContext, loggerFactory, httpClientFactory, configuration);
        _plugin = new DadJokesPlugin(host);
    }

    // ── Plugin Identity ───────────────────────────────────────────────────

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        Assert.Equal("DadJokes", _plugin.Name);
        Assert.Equal("DadJokes", _plugin.PluginKey);
        Assert.Equal("DadJokes", _plugin.KernelPluginName);
        Assert.Null(_plugin.InstanceName);
        Assert.Equal("Dad Jokes", _plugin.DisplayName);
        Assert.False(string.IsNullOrEmpty(_plugin.Description));
    }

    // ── TellJokeAsync: happy path ─────────────────────────────────────────

    [Fact]
    public async Task TellJokeAsync_NoCategory_ReturnsSetupPunchlineFormat()
    {
        var result = await _plugin.TellJokeAsync();

        Assert.Contains("\n\n", result);
        var parts = result.Split("\n\n", 2);
        Assert.Equal(2, parts.Length);
        Assert.False(string.IsNullOrWhiteSpace(parts[0]), "Setup should not be empty");
        Assert.False(string.IsNullOrWhiteSpace(parts[1]), "Punchline should not be empty");
    }

    [Fact]
    public async Task TellJokeAsync_ValidCategory_ReturnsJoke()
    {
        var result = await _plugin.TellJokeAsync("food");

        Assert.Contains("\n\n", result);
        Assert.DoesNotContain("Error", result);
        Assert.DoesNotContain("No jokes found", result);
    }

    [Fact]
    public async Task TellJokeAsync_InvalidCategory_ReturnsNoJokesFoundMessage()
    {
        var result = await _plugin.TellJokeAsync("nonexistent");

        Assert.Contains("No jokes found in category 'nonexistent'", result);
        Assert.Contains("Try a different category!", result);
    }

    // ── TellJokeAsync: exception path ─────────────────────────────────────

    [Fact]
    public async Task TellJokeAsync_OnException_ReturnsErrorMessage_DoesNotThrow()
    {
        // Arrange: force an exception by nulling out the joke service via reflection
        var field = typeof(DadJokesPlugin).GetField(
            "_jokeService",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field!.SetValue(_plugin, null);

        // Act — should not throw
        var result = await _plugin.TellJokeAsync();

        // Assert
        Assert.StartsWith("Error telling joke:", result);
    }

    // ── GetSystemPromptContributionAsync ───────────────────────────────────

    [Fact]
    public async Task GetSystemPromptContributionAsync_ReturnsNonNullStringWithCategories()
    {
        var result = await _plugin.GetSystemPromptContributionAsync("test-tenant");

        Assert.NotNull(result);
        Assert.Contains("animals", result);
        Assert.Contains("food", result);
        Assert.Contains("tech", result);
        Assert.Contains("work", result);
        Assert.Contains("science", result);
        Assert.Contains("general", result);
    }

    // ── DirectlyInvocableFunctions ─────────────────────────────────────────

    [Fact]
    public void DirectlyInvocableFunctions_ContainsTellJoke()
    {
        Assert.Contains("tell_joke", _plugin.DirectlyInvocableFunctions);
    }

    // ── SlashCommands ──────────────────────────────────────────────────────

    [Fact]
    public void SlashCommands_ContainsJokeCommand()
    {
        Assert.Single(_plugin.SlashCommands);
        Assert.Equal("/joke", _plugin.SlashCommands[0].Command);
        Assert.Equal("dad_joke", _plugin.SlashCommands[0].TaskType);
    }

    // ── ClassificationExamples ─────────────────────────────────────────────

    [Fact]
    public void ClassificationExamples_ReturnsTwoExamplesForDadJokeTask()
    {
        Assert.Equal(2, _plugin.ClassificationExamples.Count);
        Assert.All(_plugin.ClassificationExamples, ex =>
        {
            Assert.Equal("[TASK:dad_joke]", ex.ClassificationPrefix);
            Assert.Equal("DadJokes", ex.SourceName);
        });
    }
}

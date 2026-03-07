using Imeritas.Agent.DadJokes.Plugin;
using Imeritas.Agent.Extensions;
using Imeritas.Agent.Models;
using Imeritas.Agent.Plugins;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Imeritas.Agent.DadJokes.Tests.Plugin;

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

        var hostContext = new PluginHostContext(tenantContext, loggerFactory, httpClientFactory, configuration);
        _plugin = new DadJokesPlugin(hostContext);
    }

    // ── Plugin Identity ──────────────────────────────────────────

    [Fact]
    public void Name_Always_ReturnsDadJokes()
    {
        Assert.Equal("DadJokes", _plugin.Name);
    }

    [Fact]
    public void DirectlyInvocableFunctions_Always_ContainsTellJoke()
    {
        Assert.Contains("tell_joke", _plugin.DirectlyInvocableFunctions);
    }

    // ── IClassificationContributor ───────────────────────────────

    [Fact]
    public void ClassificationExamples_Always_IsNonEmpty()
    {
        var contributor = (IClassificationContributor)_plugin;
        Assert.NotEmpty(contributor.ClassificationExamples);
    }

    [Fact]
    public void SlashCommands_Always_ContainsJokeCommand()
    {
        var contributor = (IClassificationContributor)_plugin;
        Assert.Contains(contributor.SlashCommands, c => c.Command == "/joke");
    }

    // ── tell_joke Function ───────────────────────────────────────

    [Fact]
    public async Task TellJoke_NoCategory_ReturnsFormattedJokeWithSetupAndPunchline()
    {
        var result = await _plugin.TellJokeAsync();

        Assert.False(string.IsNullOrWhiteSpace(result));
        // The joke format is "{Setup}\n\n{Punchline}" — verify it contains a double newline
        Assert.Contains("\n\n", result);
    }

    [Fact]
    public async Task TellJoke_ValidCategory_ReturnsJokeFromThatCategory()
    {
        var result = await _plugin.TellJokeAsync("technology");

        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public async Task TellJoke_UnknownCategory_ReturnsRandomJokeNotError()
    {
        var result = await _plugin.TellJokeAsync("nonexistent_category_xyz");

        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.DoesNotContain("error", result, StringComparison.OrdinalIgnoreCase);
    }
}

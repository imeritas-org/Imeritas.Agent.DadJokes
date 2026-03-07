using System.ComponentModel;
using Imeritas.Agent.Extensions;
using Imeritas.Agent.Models;
using Imeritas.Agent.Plugins;
using Imeritas.Agent.Plugins.Configuration;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Imeritas.Agent.DadJokes.Plugin;

/// <summary>
/// Extension plugin that delivers dad jokes through chat and direct invocation.
/// Implements the singleton extension plugin pattern with PluginHostContext constructor,
/// attribute-driven settings, and classification contributor for intent routing.
/// </summary>
public class DadJokesPlugin : IAgentPlugin, IConfigurablePlugin<DadJokesSettings>, IClassificationContributor
{
    private readonly PluginHostContext _host;
    private readonly ILogger _logger;

    private static readonly Random Rng = new();

    // ── Constructor ──────────────────────────────────────────────────

    /// <summary>
    /// Extension plugin constructor — called by ExtensionLoader during Phase 2.
    /// ExtensionLoader detects the PluginHostContext parameter and injects it from DI.
    /// </summary>
    public DadJokesPlugin(PluginHostContext host)
    {
        _host = host;
        _logger = host.LoggerFactory.CreateLogger<DadJokesPlugin>();
    }

    // ── Plugin Identity ──────────────────────────────────────────────

    public string Name => "DadJokes";
    public string DisplayName => "Dad Jokes";
    public string Description => "Tells dad jokes with optional category filtering to brighten conversations";

    // ── System Prompt ────────────────────────────────────────────────

    public Task<string?> GetSystemPromptContributionAsync(string tenantId, string? taskType = null)
    {
        var prompt = """
            ## Dad Jokes
            You have access to a dad joke capability via the DadJokes-tell_joke function.
            When the user asks for a joke, wants to be cheered up, or requests humor, use this function.
            You can optionally pass a category like "animals", "food", "technology", or "general".
            Present the joke naturally — deliver the setup, then the punchline.
            """;
        return Task.FromResult<string?>(prompt);
    }

    // ── DirectlyInvocableFunctions ───────────────────────────────────

    /// <summary>
    /// Exposes tell_joke for direct REST invocation via /api/v1/plugins/{key}/functions/{name}/invoke.
    /// Including a function here makes the plugin appear in GET /api/v1/plugins.
    /// </summary>
    public IReadOnlySet<string> DirectlyInvocableFunctions => new HashSet<string> { "tell_joke" };

    // ── Kernel Function ──────────────────────────────────────────────

    [KernelFunction("tell_joke")]
    [Description("Tells a random dad joke. Optionally filter by category (e.g., animals, food, technology, general).")]
    public Task<string> TellJokeAsync(
        [Description("Optional joke category: animals, food, technology, or general")] string? category = null)
    {
        try
        {
            var jokes = string.IsNullOrWhiteSpace(category)
                ? AllJokes
                : Jokes.TryGetValue(category.Trim().ToLowerInvariant(), out var categoryJokes)
                    ? categoryJokes
                    : AllJokes;

            if (jokes.Count == 0)
                return Task.FromResult("I'm all out of jokes right now. Try again later!");

            var joke = jokes[Rng.Next(jokes.Count)];

            _logger.LogInformation("Telling joke from category {Category}", category ?? "random");

            return Task.FromResult($"{joke.Setup}\n\n{joke.Punchline}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error telling joke for category {Category}", category);
            return Task.FromResult($"Error telling joke: {ex.Message}");
        }
    }

    // ── IClassificationContributor ───────────────────────────────────

    public IReadOnlyList<SlashCommandMapping> SlashCommands => new[]
    {
        new SlashCommandMapping(
            Command: "/joke",
            Intent: IntentType.Task,
            TaskType: "dad_joke",
            ScheduleFrequency: null,
            ResponseText: "Here comes a dad joke!",
            SourceName: Name)
    };

    public IReadOnlyList<ClassificationExample> ClassificationExamples => new[]
    {
        new ClassificationExample(
            UserMessage: "Tell me a dad joke",
            ClassificationPrefix: "[task:dad_joke]",
            ResponseText: "Here comes a dad joke!",
            SourceName: Name),
        new ClassificationExample(
            UserMessage: "I need a funny joke to cheer me up",
            ClassificationPrefix: "[task:dad_joke]",
            ResponseText: "Let me find a joke for you!",
            SourceName: Name)
    };

    public ClassificationHints ClassificationHints => new()
    {
        SourceName = Name,
        TaskType = "dad_joke",
        TaskTypeKeywords = new[] { "joke", "dad joke", "funny", "humor", "punchline", "laugh" }
    };

    // ── Embedded Joke Data ───────────────────────────────────────────

    private record DadJoke(string Setup, string Punchline);

    private static readonly Dictionary<string, List<DadJoke>> Jokes = new()
    {
        ["general"] = new()
        {
            new("Why don't scientists trust atoms?", "Because they make up everything!"),
            new("I'm reading a book about anti-gravity.", "It's impossible to put down!"),
            new("Why did the scarecrow win an award?", "Because he was outstanding in his field!"),
            new("I used to hate facial hair...", "But then it grew on me."),
            new("What do you call a fake noodle?", "An impasta!")
        },
        ["animals"] = new()
        {
            new("What do you call a bear with no teeth?", "A gummy bear!"),
            new("Why do cows wear bells?", "Because their horns don't work!"),
            new("What do you call a fish without eyes?", "A fsh!"),
            new("Why don't oysters share?", "Because they're shellfish!"),
            new("What do you call a sleeping dinosaur?", "A dino-snore!")
        },
        ["food"] = new()
        {
            new("What did the grape do when it got stepped on?", "Nothing, it just let out a little wine!"),
            new("Why did the coffee file a police report?", "It got mugged!"),
            new("What do you call cheese that isn't yours?", "Nacho cheese!"),
            new("Why did the tomato turn red?", "Because it saw the salad dressing!"),
            new("What do you call a sad strawberry?", "A blueberry!")
        },
        ["technology"] = new()
        {
            new("Why do programmers prefer dark mode?", "Because light attracts bugs!"),
            new("What's a computer's favorite snack?", "Microchips!"),
            new("Why was the computer cold?", "It left its Windows open!"),
            new("What did the router say to the doctor?", "It hurts when IP!"),
            new("Why did the developer go broke?", "Because he used up all his cache!")
        }
    };

    private static readonly List<DadJoke> AllJokes = Jokes.Values.SelectMany(j => j).ToList();
}

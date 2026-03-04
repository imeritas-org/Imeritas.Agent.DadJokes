using System.ComponentModel;
using Imeritas.Agent.DadJokes.Services;
using Imeritas.Agent.Extensions;
using Imeritas.Agent.Models;
using Imeritas.Agent.Plugins;
using Imeritas.Agent.Plugins.Configuration;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Imeritas.Agent.DadJokes.Plugin;

/// <summary>
/// Singleton extension plugin that delivers dad jokes through a directly invocable function.
/// </summary>
public class DadJokesPlugin : IAgentPlugin, IClassificationContributor, IConfigurablePlugin<DadJokesPluginSettings>
{
    private readonly ILogger _logger;
    private readonly JokeCollectionService _jokeService = new();

    /// <summary>
    /// Extension plugin constructor — called by the framework plugin loader.
    /// </summary>
    public DadJokesPlugin(PluginHostContext host)
    {
        _logger = host.LoggerFactory.CreateLogger<DadJokesPlugin>();
    }

    /// <summary>
    /// Test-friendly constructor.
    /// </summary>
    internal DadJokesPlugin(ILogger<DadJokesPlugin> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "DadJokes";

    /// <inheritdoc />
    public string DisplayName => "Dad Jokes";

    /// <inheritdoc />
    public string Description => "Tells dad jokes by category or at random";

    /// <inheritdoc />
    public IReadOnlySet<string> DirectlyInvocableFunctions =>
        new HashSet<string> { "tell_joke" };

    /// <summary>
    /// Tells a dad joke, optionally filtered by category.
    /// </summary>
    [KernelFunction("tell_joke")]
    [Description("Tells a dad joke. Optionally filtered by category.")]
    public string TellJoke(
        [Description("Optional joke category (e.g. tech, food, animals, work, science, sports, music, general)")]
        string? category = null)
    {
        try
        {
            var joke = _jokeService.GetJoke(category);
            _logger.LogInformation("Told joke {JokeId} for category {Category}", joke.Id, category ?? "random");
            return $"{joke.Setup}\n{joke.Punchline}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error telling joke for category {Category}", category);
            return $"Error telling joke: {ex.Message}";
        }
    }

    /// <inheritdoc />
    public Task<string?> GetSystemPromptContributionAsync(string tenantId, string? taskType = null)
        => Task.FromResult<string?>(
            "You can tell dad jokes using the DadJokes plugin. Use the tell_joke function with an optional category parameter (tech, food, animals, work, science, sports, music, general).");

    // IClassificationContributor

    /// <inheritdoc />
    public IReadOnlyList<SlashCommandMapping> SlashCommands =>
    [
        new SlashCommandMapping("/joke", IntentType.Task, "dad_joke", "",
            "I'll tell you a dad joke!", "DadJokes")
    ];

    /// <inheritdoc />
    public IReadOnlyList<ClassificationExample> ClassificationExamples =>
    [
        new ClassificationExample("Tell me a dad joke", "[TASK:dad_joke]",
            "Here comes a dad joke!", "DadJokes"),
        new ClassificationExample("I need a tech joke", "[TASK:dad_joke]",
            "Let me find a tech joke for you!", "DadJokes"),
        new ClassificationExample("Make me laugh with a food joke", "[TASK:dad_joke]",
            "One food joke coming right up!", "DadJokes")
    ];

    /// <inheritdoc />
    public ClassificationHints ClassificationHints => new()
    {
        SourceName = "DadJokes",
        TaskType = "dad_joke",
        TaskTypeKeywords = ["joke", "dad joke", "funny", "humor", "pun"]
    };
}

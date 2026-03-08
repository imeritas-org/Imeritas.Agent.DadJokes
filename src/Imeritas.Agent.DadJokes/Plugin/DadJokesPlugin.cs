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
/// A singleton plugin that delivers dad jokes through AI chat and direct REST invocation.
/// Implements <see cref="IAgentPlugin"/>, <see cref="IConfigurablePlugin{DadJokesSettings}"/>
/// (marker interface — the framework auto-generates config schema from <see cref="PluginSettingAttribute"/>
/// attributes on <see cref="DadJokesSettings"/>),
/// and <see cref="IClassificationContributor"/> for classification-driven routing.
/// </summary>
public class DadJokesPlugin : IAgentPlugin, IConfigurablePlugin<DadJokesSettings>, IClassificationContributor
{
    private readonly ILogger _logger;
    private readonly JokeRepository _repository;

    /// <summary>
    /// Creates a new instance of the DadJokes plugin.
    /// Called by the framework's ExtensionLoader during Phase 2 (instantiation).
    /// </summary>
    /// <param name="host">Curated service access provided by the framework.</param>
    public DadJokesPlugin(PluginHostContext host)
    {
        _logger = host.LoggerFactory.CreateLogger<DadJokesPlugin>();
        _repository = new JokeRepository();
    }

    /// <summary>
    /// Internal constructor for unit testing — allows injection of a custom <see cref="JokeRepository"/>.
    /// Not used by the framework.
    /// </summary>
    /// <param name="host">Curated service access provided by the framework.</param>
    /// <param name="repository">The joke repository to use.</param>
    internal DadJokesPlugin(PluginHostContext host, JokeRepository repository)
    {
        _logger = host.LoggerFactory.CreateLogger<DadJokesPlugin>();
        _repository = repository;
    }

    /// <inheritdoc />
    public string Name => "DadJokes";

    /// <inheritdoc />
    public string DisplayName => "Dad Jokes";

    /// <inheritdoc />
    public string Description => "Delivers contextually relevant dad jokes through chat and invocable functions.";

    /// <inheritdoc />
    public Task<string?> GetSystemPromptContributionAsync(string tenantId, string? taskType = null)
    {
        var categories = string.Join(", ", _repository.GetCategories());
        var prompt = $"## Dad Jokes\n- Use the DadJokes-tell_joke function to tell dad jokes.\n- Available categories: {categories}\n- Pass an optional category parameter to get a themed joke.";
        return Task.FromResult<string?>(prompt);
    }

    /// <inheritdoc />
    public IReadOnlySet<string> DirectlyInvocableFunctions => new HashSet<string> { "tell_joke" };

    /// <summary>
    /// Tells a dad joke, optionally filtered by category.
    /// Returns a formatted joke string. If the category has no matches, returns a random joke.
    /// </summary>
    /// <param name="category">Optional joke category (e.g., "tech", "food", "animals"). Leave empty for a random joke.</param>
    /// <returns>A formatted dad joke string.</returns>
    [KernelFunction("tell_joke")]
    [Description("Tells a dad joke, optionally filtered by category (tech, food, animals, work, science, general)")]
    public Task<string> TellJokeAsync(
        [Description("Optional joke category: tech, food, animals, work, science, general. Leave empty for random.")] string? category = null)
    {
        try
        {
            var joke = _repository.GetRandomByCategory(category);
            _logger.LogInformation("Told joke {JokeId} (category filter: {Category})", joke.Id, category ?? "none");
            return Task.FromResult(joke.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error telling joke with category {Category}", category);
            return Task.FromResult($"Error telling joke: {ex.Message}");
        }
    }

    // ── IClassificationContributor ──

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IReadOnlyList<ClassificationExample> ClassificationExamples => new[]
    {
        new ClassificationExample(
            UserMessage: "Tell me a dad joke",
            ClassificationPrefix: "[task:dad_joke]",
            ResponseText: "Let me find a good one!",
            SourceName: Name),
        new ClassificationExample(
            UserMessage: "Give me a tech joke",
            ClassificationPrefix: "[task:dad_joke]",
            ResponseText: "Here's a tech dad joke!",
            SourceName: Name)
    };

    /// <inheritdoc />
    public ClassificationHints ClassificationHints => new()
    {
        SourceName = Name,
        TaskType = "dad_joke",
        TaskTypeKeywords = new[] { "joke", "dad joke", "funny", "pun", "humor" }
    };
}

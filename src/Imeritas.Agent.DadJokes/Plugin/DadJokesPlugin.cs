using System.ComponentModel;
using System.Text;
using Imeritas.Agent.DadJokes.Models;
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
/// Dad Jokes plugin — exposes a <c>tell_joke</c> kernel function that delivers
/// setup/punchline dad jokes optionally filtered by category.
/// Contributes classification metadata so the router can direct joke-related
/// messages to the correct task type.
/// </summary>
public class DadJokesPlugin : IAgentPlugin, IClassificationContributor, IConfigurablePlugin<DadJokesSettings>
{
    // ── Fields ───────────────────────────────────────────────────────────
    private readonly ILogger _logger;
    private readonly JokeService _jokeService = new();

    // ── Constructor ──────────────────────────────────────────────────────
    /// <summary>
    /// Extension plugin constructor — <c>ExtensionLoader</c> detects the
    /// <see cref="PluginHostContext"/> parameter and injects it from DI during
    /// Phase 2 instantiation.
    /// </summary>
    public DadJokesPlugin(PluginHostContext host)
    {
        _logger = host.LoggerFactory.CreateLogger<DadJokesPlugin>();
    }

    // ── Plugin Identity ──────────────────────────────────────────────────
    /// <inheritdoc />
    public string Name => "DadJokes";

    /// <inheritdoc />
    public string? InstanceName => null;

    /// <inheritdoc />
    public string PluginKey => Name;

    /// <inheritdoc />
    public string KernelPluginName => Name;

    /// <inheritdoc />
    public string DisplayName => "Dad Jokes";

    /// <inheritdoc />
    public string Description => "Tells dad jokes by category with setup and punchline format";

    // ── Kernel Registration ──────────────────────────────────────────────
    /// <inheritdoc />
    public void RegisterOnKernel(Kernel kernel)
        => kernel.Plugins.AddFromObject(this, KernelPluginName);

    // ── System Prompt ────────────────────────────────────────────────────
    /// <inheritdoc />
    public Task<string?> GetSystemPromptContributionAsync(string tenantId, string? taskType = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Dad Jokes Plugin");
        sb.AppendLine("- You can tell dad jokes using the DadJokes-tell_joke function");
        sb.AppendLine("- Optionally specify a category for themed jokes");
        sb.Append("- Available categories: ");
        sb.AppendLine(string.Join(", ", _jokeService.GetCategories()));
        sb.AppendLine("- Jokes are returned in setup/punchline format");
        return Task.FromResult<string?>(sb.ToString());
    }

    // ── KernelFunction: tell_joke ────────────────────────────────────────
    /// <summary>
    /// Tells a dad joke, optionally filtered by category.
    /// Returns the joke in "Setup\n\nPunchline" format.
    /// Returns an error string on failure — never throws.
    /// </summary>
    [KernelFunction("tell_joke")]
    [Description("Tells a dad joke, optionally filtered by category")]
    public Task<string> TellJokeAsync(
        [Description("Optional joke category (e.g., 'programming', 'food'). Leave empty for any category.")]
        string? category = null)
    {
        try
        {
            // If a category was requested but has no matches, return feedback
            if (!string.IsNullOrWhiteSpace(category) && _jokeService.GetByCategory(category).Count == 0)
                return Task.FromResult($"No jokes found in category '{category}'. Try a different category!");

            // Get joke from service (falls back to random if category is null/empty)
            var joke = _jokeService.GetRandomByCategory(category);

            // Return in 'Setup\n\nPunchline' format per AC
            return Task.FromResult($"{joke.Setup}\n\n{joke.Punchline}");
        }
        catch (Exception ex)
        {
            // Return error strings, never throw (AC + framework convention)
            _logger.LogError(ex, "Error telling joke for category {Category}", category);
            return Task.FromResult($"Error telling joke: {ex.Message}");
        }
    }

    // ── Approval Gate ────────────────────────────────────────────────────
    /// <inheritdoc />
    public IReadOnlySet<string> ApprovalRequiredFunctions => new HashSet<string>();

    // ── Directly Invocable Functions ─────────────────────────────────────
    /// <inheritdoc />
    public IReadOnlySet<string> DirectlyInvocableFunctions => new HashSet<string> { "tell_joke" };

    // ── IClassificationContributor ───────────────────────────────────────
    /// <inheritdoc />
    public IReadOnlyList<SlashCommandMapping> SlashCommands => new[]
    {
        new SlashCommandMapping(
            Command: "/joke",
            Intent: IntentType.Task,
            TaskType: "dad_joke",
            ScheduleFrequency: null,
            ResponseText: "Let me tell you a dad joke!",
            SourceName: Name)
    };

    /// <inheritdoc />
    public IReadOnlyList<ClassificationExample> ClassificationExamples => new[]
    {
        new ClassificationExample(
            UserMessage: "Tell me a dad joke",
            ClassificationPrefix: "[TASK:dad_joke]",
            ResponseText: "Let me tell you a dad joke!",
            SourceName: Name),
        new ClassificationExample(
            UserMessage: "I need a programming joke",
            ClassificationPrefix: "[TASK:dad_joke]",
            ResponseText: "Here comes a programming dad joke!",
            SourceName: Name)
    };

    /// <inheritdoc />
    public ClassificationHints ClassificationHints => new()
    {
        SourceName = Name,
        TaskType = "dad_joke",
        TaskTypeKeywords = new[] { "dad joke", "joke", "funny", "punchline", "humor", "laugh" }
    };
}

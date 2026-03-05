using Imeritas.Agent.DadJokes.Services;
using Imeritas.Agent.Models;
using Imeritas.Agent.Orchestrators;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging;
using AgentTaskStatus = Imeritas.Agent.Models.TaskStatus;

namespace Imeritas.Agent.DadJokes.Orchestrator;

/// <summary>
/// Orchestrator for dad joke tasks. Handles the 'dad_joke' task type by
/// fetching a joke from <see cref="JokeService"/> (optionally filtered by
/// category) and persisting the result via <see cref="IStorageService"/>.
/// </summary>
public class DadJokeOrchestrator : ITaskOrchestrator, IClassificationContributor
{
    private readonly IStorageService _storage;
    private readonly ILogger<DadJokeOrchestrator> _logger;
    private readonly JokeService _jokeService = new();

    public DadJokeOrchestrator(
        IStorageService storage,
        ILogger<DadJokeOrchestrator> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    // ── ITaskOrchestrator Identity ────────────────────────────────────

    /// <inheritdoc />
    public string Name => "DadJoke";

    /// <inheritdoc />
    public int Priority => 50;

    /// <inheritdoc />
    public bool CanHandle(string tenantId, string? taskType, string prompt)
        => taskType == "dad_joke";

    // ── Execution ─────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<AgentTask> ExecuteAsync(
        string tenantId,
        string userId,
        string prompt,
        string? taskType = null,
        Dictionary<string, object>? inputData = null,
        string? parentTaskId = null,
        CancellationToken cancellationToken = default,
        ITaskProgressCallback? progress = null)
    {
        var task = new AgentTask
        {
            UserId = userId,
            Title = "Dad Joke",
            Description = prompt,
            Type = TaskType.Manual,
            Status = AgentTaskStatus.Running,
            StartedAt = DateTime.UtcNow,
            ParentTaskId = parentTaskId
        };

        try
        {
            // Extract optional category from inputData
            string? category = null;
            if (inputData?.TryGetValue("category", out var categoryObj) == true)
                category = categoryObj?.ToString();

            _logger.LogInformation(
                "Executing dad joke task for user {UserId}, category {Category}",
                userId, category ?? "random");

            // Get joke — falls back to random when category is null/empty/unknown
            var joke = _jokeService.GetRandomByCategory(category);
            var jokeText = $"{joke.Setup}\n\n{joke.Punchline}";

            task.OutputData["result"] = jokeText;

            if (progress != null)
                await progress.ReportAsync("Here's a dad joke for you!");

            task.Status = AgentTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing dad joke task for user {UserId}", userId);
            task.Status = AgentTaskStatus.Failed;
            task.ErrorMessage = ex.Message;
        }
        finally
        {
            task.LastUpdatedAt = DateTime.UtcNow;
            await _storage.SaveTaskAsync(tenantId, task);
        }

        return task;
    }

    // ── IClassificationContributor ────────────────────────────────────

    /// <inheritdoc />
    public ClassificationHints ClassificationHints => new()
    {
        TaskType = "dad_joke",
        TaskTypeKeywords = new[] { "dad joke", "joke", "funny", "punchline" }
    };
}

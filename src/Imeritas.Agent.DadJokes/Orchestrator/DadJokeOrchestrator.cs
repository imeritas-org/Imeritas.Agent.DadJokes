using Imeritas.Agent.DadJokes.Services;
using Imeritas.Agent.Models;
using Imeritas.Agent.Orchestrators;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging;
using AgentTaskStatus = Imeritas.Agent.Models.TaskStatus;

namespace Imeritas.Agent.DadJokes.Orchestrator;

/// <summary>
/// Scoped orchestrator for the "dad_joke" task type.
/// </summary>
public class DadJokeOrchestrator : ITaskOrchestrator
{
    private readonly IStorageService _storage;
    private readonly ILogger<DadJokeOrchestrator> _logger;
    private readonly JokeCollectionService _jokeService = new();

    public DadJokeOrchestrator(
        IStorageService storage,
        ILogger<DadJokeOrchestrator> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "DadJoke";

    /// <inheritdoc />
    public int Priority => 50;

    /// <inheritdoc />
    public bool CanHandle(string tenantId, string? taskType, string prompt)
        => string.Equals(taskType, "dad_joke", StringComparison.OrdinalIgnoreCase);

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
            Status = AgentTaskStatus.Running,
            StartedAt = DateTime.UtcNow,
            ParentTaskId = parentTaskId
        };

        // Map queue task/session IDs if present
        if (inputData?.TryGetValue("_queue_task_id", out var qid) == true)
            task.TaskId = qid.ToString()!;
        if (inputData?.TryGetValue("_queue_session_id", out var sid) == true)
            task.SessionId = sid.ToString();

        try
        {
            var category = ExtractCategory(inputData);

            _logger.LogInformation(
                "Executing dad joke task for tenant {TenantId}, category {Category}",
                tenantId, category ?? "random");

            var joke = _jokeService.GetJoke(category);
            var formatted = $"{joke.Setup}\n{joke.Punchline}";

            task.Status = AgentTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.OutputData["result"] = formatted;
            task.OutputData["category"] = category ?? "random";
            task.OutputData["joke_id"] = joke.Id;
            task.ExecutionLog.Add($"Selected joke {joke.Id} for category '{category ?? "random"}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing dad joke task");
            task.Status = AgentTaskStatus.Failed;
            task.ErrorMessage = ex.Message;
            task.OutputData["result"] = $"Error: {ex.Message}";
        }

        await _storage.SaveTaskAsync(tenantId, task);
        return task;
    }

    private static string? ExtractCategory(Dictionary<string, object>? inputData)
    {
        if (inputData?.TryGetValue("category", out var cat) == true)
        {
            var catStr = cat?.ToString();
            if (!string.IsNullOrWhiteSpace(catStr))
                return catStr;
        }

        return null;
    }
}

using Imeritas.Agent.DadJokes.Models;
using Imeritas.Agent.DadJokes.Services;
using Imeritas.Agent.Models;
using Imeritas.Agent.Orchestrators;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging;
using TaskStatus = Imeritas.Agent.Models.TaskStatus;

namespace Imeritas.Agent.DadJokes.Orchestrator;

public class DadJokeOrchestrator : ITaskOrchestrator
{
    private readonly JokeService _jokeService;
    private readonly IStorageService _storage;
    private readonly ILogger<DadJokeOrchestrator> _logger;

    public DadJokeOrchestrator(
        JokeService jokeService,
        IStorageService storage,
        ILogger<DadJokeOrchestrator> logger)
    {
        _jokeService = jokeService;
        _storage = storage;
        _logger = logger;
    }

    public string Name => "DadJoke";

    public int Priority => 50;

    public bool CanHandle(string tenantId, string? taskType, string prompt)
        => taskType == "dad_joke";

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
        // 1. Create AgentTask with queue-provided IDs (universal framework pattern)
        var task = new AgentTask
        {
            TaskId = inputData?.TryGetValue("_queue_task_id", out var qtid) == true
                && qtid is string qts && !string.IsNullOrEmpty(qts)
                    ? qts : Guid.NewGuid().ToString(),
            SessionId = inputData?.TryGetValue("_queue_session_id", out var qsid) == true
                ? qsid?.ToString() : null,
            UserId = userId,
            Title = "Dad Joke",
            Description = prompt,
            Status = TaskStatus.Running,
            StartedAt = DateTime.UtcNow,
            OrchestratorName = Name,
            InputData = inputData ?? new Dictionary<string, object>(),
            ParentTaskId = parentTaskId
        };

        _logger.LogInformation("DadJoke orchestrator starting task {TaskId} for user {UserId}", task.TaskId, userId);

        try
        {
            // 2. Read optional category from inputData
            string? category = null;
            if (inputData?.TryGetValue("category", out var catObj) == true)
            {
                category = catObj?.ToString();
            }

            // 3. Get joke from JokeService
            DadJoke? joke = null;
            if (!string.IsNullOrEmpty(category))
            {
                _logger.LogDebug("Looking up joke for category {Category}", category);
                var jokes = _jokeService.GetByCategory(category);
                if (jokes.Count > 0)
                {
                    joke = jokes[Random.Shared.Next(jokes.Count)];
                }
            }

            // 4. Fall back to random joke if no category match or no category
            if (joke is null)
            {
                joke = _jokeService.GetRandom();
                _logger.LogDebug("Using random joke (category={Category})", category ?? "(none)");
            }

            // 5. Set result and mark completed
            task.OutputData["result"] = $"{joke.Setup} {joke.Punchline}";
            task.Status = TaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation("DadJoke task {TaskId} completed successfully", task.TaskId);
        }
        catch (Exception ex)
        {
            // 6. On exception, set Failed status with error details
            _logger.LogError(ex, "DadJoke task {TaskId} failed", task.TaskId);
            task.Status = TaskStatus.Failed;
            task.ErrorMessage = ex.Message;
            task.OutputData["result"] = $"Error: {ex.Message}";
        }
        finally
        {
            // 7. Always save task before returning (framework convention)
            task.LastUpdatedAt = DateTime.UtcNow;
            await _storage.SaveTaskAsync(tenantId, task);
        }

        return task;
    }
}

using Imeritas.Agent.DadJokes.Services;
using Imeritas.Agent.Models;
using Imeritas.Agent.Orchestrators;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging;
using TaskStatus = Imeritas.Agent.Models.TaskStatus;

namespace Imeritas.Agent.DadJokes.Orchestrator;

/// <summary>
/// A simple task orchestrator for the "dad_joke" task type.
/// Selects a joke by category from the repository and returns it as the task result.
/// Falls back to a random joke when no category match is found.
/// </summary>
public class DadJokeOrchestrator : ITaskOrchestrator
{
    private readonly IStorageService _storage;
    private readonly ILogger<DadJokeOrchestrator> _logger;
    private readonly JokeRepository _repository;

    /// <summary>
    /// Creates a new instance of the DadJoke orchestrator.
    /// Dependencies are resolved via standard DI (orchestrators are scoped).
    /// </summary>
    /// <param name="storage">Storage service for persisting task state.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public DadJokeOrchestrator(IStorageService storage, ILogger<DadJokeOrchestrator> logger)
    {
        _storage = storage;
        _logger = logger;
        _repository = new JokeRepository();
    }

    /// <summary>
    /// Internal constructor for unit testing — allows injection of a custom <see cref="JokeRepository"/>.
    /// </summary>
    /// <param name="storage">Storage service for persisting task state.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="repository">The joke repository to use.</param>
    internal DadJokeOrchestrator(IStorageService storage, ILogger<DadJokeOrchestrator> logger, JokeRepository repository)
    {
        _storage = storage;
        _logger = logger;
        _repository = repository;
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
            Status = TaskStatus.Running,
            StartedAt = DateTime.UtcNow,
            OrchestratorName = Name,
            ParentTaskId = parentTaskId
        };

        if (inputData != null)
        {
            foreach (var kvp in inputData)
                task.InputData[kvp.Key] = kvp.Value;
        }

        task.ExecutionLog.Add($"DadJokeOrchestrator started for tenant {tenantId}");

        try
        {
            // Extract category from input data or prompt
            var category = ExtractCategory(inputData, prompt);

            if (progress != null)
                await progress.ReportAsync("Finding the perfect dad joke...");

            var joke = _repository.GetRandomByCategory(category);

            _logger.LogInformation(
                "Selected joke {JokeId} for tenant {TenantId} (category: {Category})",
                joke.Id, tenantId, category ?? "random");

            task.OutputData["result"] = joke.ToString();
            task.OutputData["jokeId"] = joke.Id;
            task.OutputData["categories"] = string.Join(", ", joke.Categories);
            task.Status = TaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.ExecutionLog.Add($"Delivered joke {joke.Id}: {joke.Setup}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing dad joke task for tenant {TenantId}", tenantId);
            task.Status = TaskStatus.Failed;
            task.ErrorMessage = ex.Message;
            task.CompletedAt = DateTime.UtcNow;
            task.ExecutionLog.Add($"Failed: {ex.Message}");
        }

        await _storage.SaveTaskAsync(tenantId, task);
        return task;
    }

    /// <summary>
    /// Extracts a joke category from the input data dictionary or the prompt text.
    /// Checks inputData["category"] first, then scans the prompt for known category keywords.
    /// </summary>
    private string? ExtractCategory(Dictionary<string, object>? inputData, string prompt)
    {
        // Check inputData first
        if (inputData?.TryGetValue("category", out var categoryObj) == true)
        {
            var cat = categoryObj?.ToString();
            if (!string.IsNullOrWhiteSpace(cat))
                return cat;
        }

        // Scan prompt for known categories
        var knownCategories = _repository.GetCategories();
        var lowerPrompt = prompt.ToLowerInvariant();
        foreach (var cat in knownCategories)
        {
            if (lowerPrompt.Contains(cat, StringComparison.OrdinalIgnoreCase))
                return cat;
        }

        return null;
    }
}

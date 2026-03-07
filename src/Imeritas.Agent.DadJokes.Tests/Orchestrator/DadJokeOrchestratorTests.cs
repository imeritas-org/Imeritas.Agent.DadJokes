using Imeritas.Agent.DadJokes.Orchestrator;
using Imeritas.Agent.DadJokes.Services;
using Imeritas.Agent.Models;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using TaskStatus = Imeritas.Agent.Models.TaskStatus;

namespace Imeritas.Agent.DadJokes.Tests.Orchestrator;

public class DadJokeOrchestratorTests
{
    private readonly IStorageService _storage;
    private readonly DadJokeOrchestrator _orchestrator;

    public DadJokeOrchestratorTests()
    {
        _storage = Substitute.For<IStorageService>();
        _storage.SaveTaskAsync(Arg.Any<string>(), Arg.Any<AgentTask>()).Returns(Task.CompletedTask);

        var jokeService = new JokeService();
        var logger = NullLogger<DadJokeOrchestrator>.Instance;
        _orchestrator = new DadJokeOrchestrator(jokeService, _storage, logger);
    }

    // ── CanHandle ────────────────────────────────────────────────

    [Fact]
    public void CanHandle_DadJokeTaskType_ReturnsTrue()
    {
        Assert.True(_orchestrator.CanHandle("t1", "dad_joke", "any"));
    }

    [Fact]
    public void CanHandle_OtherTaskType_ReturnsFalse()
    {
        Assert.False(_orchestrator.CanHandle("t1", "email", "any"));
    }

    [Fact]
    public void CanHandle_NullTaskType_ReturnsFalse()
    {
        Assert.False(_orchestrator.CanHandle("t1", null, "any"));
    }

    // ── ExecuteAsync — Task ID Mapping ───────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithQueueTaskId_SetsTaskIdToQueueValue()
    {
        var inputData = new Dictionary<string, object>
        {
            ["_queue_task_id"] = "my-task-123"
        };

        var result = await _orchestrator.ExecuteAsync("t1", "u1", "tell me a joke",
            taskType: "dad_joke", inputData: inputData);

        Assert.Equal("my-task-123", result.TaskId);
    }

    [Fact]
    public async Task ExecuteAsync_WithQueueSessionId_SetsSessionIdToQueueValue()
    {
        var inputData = new Dictionary<string, object>
        {
            ["_queue_session_id"] = "sess-456"
        };

        var result = await _orchestrator.ExecuteAsync("t1", "u1", "tell me a joke",
            taskType: "dad_joke", inputData: inputData);

        Assert.Equal("sess-456", result.SessionId);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutQueueTaskId_GeneratesNewGuid()
    {
        var result = await _orchestrator.ExecuteAsync("t1", "u1", "tell me a joke",
            taskType: "dad_joke");

        Assert.True(Guid.TryParse(result.TaskId, out _));
    }

    // ── ExecuteAsync — Joke Content ──────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithCategory_ReturnsMatchingJoke()
    {
        var inputData = new Dictionary<string, object>
        {
            ["category"] = "tech"
        };

        var result = await _orchestrator.ExecuteAsync("t1", "u1", "tell me a tech joke",
            taskType: "dad_joke", inputData: inputData);

        Assert.True(result.OutputData.ContainsKey("result"));
        var jokeText = result.OutputData["result"]?.ToString();
        Assert.False(string.IsNullOrWhiteSpace(jokeText));
    }

    [Fact]
    public async Task ExecuteAsync_WithoutCategory_ReturnsRandomJoke()
    {
        var result = await _orchestrator.ExecuteAsync("t1", "u1", "tell me a joke",
            taskType: "dad_joke");

        Assert.True(result.OutputData.ContainsKey("result"));
        var jokeText = result.OutputData["result"]?.ToString();
        Assert.False(string.IsNullOrWhiteSpace(jokeText));
    }

    // ── ExecuteAsync — Task Status & Persistence ─────────────────

    [Fact]
    public async Task ExecuteAsync_Success_SetsStatusToCompleted()
    {
        var result = await _orchestrator.ExecuteAsync("t1", "u1", "tell me a joke",
            taskType: "dad_joke");

        Assert.Equal(TaskStatus.Completed, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_Success_SavesTaskViaStorageService()
    {
        await _orchestrator.ExecuteAsync("t1", "u1", "tell me a joke",
            taskType: "dad_joke");

        await _storage.Received().SaveTaskAsync("t1", Arg.Any<AgentTask>());
    }
}

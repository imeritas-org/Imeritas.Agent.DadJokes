using Imeritas.Agent.DadJokes.Orchestrator;
using Imeritas.Agent.Models;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using AgentTaskStatus = Imeritas.Agent.Models.TaskStatus;

namespace Imeritas.Agent.DadJokes.Tests.Orchestrator;

public class DadJokeOrchestratorTests
{
    private readonly IStorageService _storage;
    private readonly DadJokeOrchestrator _sut;

    public DadJokeOrchestratorTests()
    {
        _storage = Substitute.For<IStorageService>();
        var logger = Substitute.For<ILogger<DadJokeOrchestrator>>();
        _sut = new DadJokeOrchestrator(_storage, logger);
    }

    [Fact]
    public void Name_ReturnsDadJoke()
    {
        Assert.Equal("DadJoke", _sut.Name);
    }

    [Fact]
    public void Priority_Returns50()
    {
        Assert.Equal(50, _sut.Priority);
    }

    [Fact]
    public void CanHandle_DadJokeTaskType_ReturnsTrue()
    {
        Assert.True(_sut.CanHandle("tenant1", "dad_joke", "tell me a joke"));
    }

    [Fact]
    public void CanHandle_DadJokeTaskType_IsCaseInsensitive()
    {
        Assert.True(_sut.CanHandle("tenant1", "DAD_JOKE", "tell me a joke"));
        Assert.True(_sut.CanHandle("tenant1", "Dad_Joke", "tell me a joke"));
    }

    [Fact]
    public void CanHandle_NullTaskType_ReturnsFalse()
    {
        Assert.False(_sut.CanHandle("tenant1", null, "tell me a joke"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("chat")]
    [InlineData("runbook")]
    [InlineData("other_task")]
    public void CanHandle_OtherTaskTypes_ReturnsFalse(string taskType)
    {
        Assert.False(_sut.CanHandle("tenant1", taskType, "tell me a joke"));
    }

    [Fact]
    public async Task ExecuteAsync_WithCategory_ReturnsMatchingCategoryJoke()
    {
        var inputData = new Dictionary<string, object> { ["category"] = "tech" };

        var result = await _sut.ExecuteAsync(
            "tenant1", "user1", "tell me a tech joke",
            taskType: "dad_joke", inputData: inputData);

        Assert.Equal(AgentTaskStatus.Completed, result.Status);
        Assert.True(result.OutputData.ContainsKey("result"));
        var jokeText = result.OutputData["result"].ToString();
        Assert.NotNull(jokeText);
        Assert.Contains("\n", jokeText);
        Assert.Equal("tech", result.OutputData["category"]!.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WithNoCategory_ReturnsRandomJoke()
    {
        var result = await _sut.ExecuteAsync(
            "tenant1", "user1", "tell me a joke",
            taskType: "dad_joke");

        Assert.Equal(AgentTaskStatus.Completed, result.Status);
        Assert.True(result.OutputData.ContainsKey("result"));
        var jokeText = result.OutputData["result"].ToString();
        Assert.NotNull(jokeText);
        Assert.Contains("\n", jokeText);
        Assert.Equal("random", result.OutputData["category"]!.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_SetsTaskStatusToCompleted()
    {
        var result = await _sut.ExecuteAsync(
            "tenant1", "user1", "joke please",
            taskType: "dad_joke");

        Assert.Equal(AgentTaskStatus.Completed, result.Status);
        Assert.NotNull(result.CompletedAt);
        Assert.NotNull(result.StartedAt);
    }

    [Fact]
    public async Task ExecuteAsync_SetsOutputDataResult()
    {
        var result = await _sut.ExecuteAsync(
            "tenant1", "user1", "joke please",
            taskType: "dad_joke");

        Assert.True(result.OutputData.ContainsKey("result"));
        Assert.True(result.OutputData.ContainsKey("category"));
        Assert.True(result.OutputData.ContainsKey("joke_id"));
    }

    [Fact]
    public async Task ExecuteAsync_CallsSaveTaskAsync()
    {
        await _sut.ExecuteAsync(
            "tenant1", "user1", "joke please",
            taskType: "dad_joke");

        await _storage.Received(1).SaveTaskAsync("tenant1", Arg.Any<AgentTask>());
    }

    [Fact]
    public async Task ExecuteAsync_SetsUserIdAndDescription()
    {
        var result = await _sut.ExecuteAsync(
            "tenant1", "user1", "I want a joke",
            taskType: "dad_joke");

        Assert.Equal("user1", result.UserId);
        Assert.Equal("I want a joke", result.Description);
        Assert.Equal("Dad Joke", result.Title);
    }

    [Fact]
    public async Task ExecuteAsync_WithQueueIds_MapsTaskAndSessionIds()
    {
        var inputData = new Dictionary<string, object>
        {
            ["_queue_task_id"] = "qt-123",
            ["_queue_session_id"] = "qs-456"
        };

        var result = await _sut.ExecuteAsync(
            "tenant1", "user1", "joke",
            taskType: "dad_joke", inputData: inputData);

        Assert.Equal("qt-123", result.TaskId);
        Assert.Equal("qs-456", result.SessionId);
    }

    [Fact]
    public async Task ExecuteAsync_AddsExecutionLogEntry()
    {
        var result = await _sut.ExecuteAsync(
            "tenant1", "user1", "joke",
            taskType: "dad_joke");

        Assert.NotEmpty(result.ExecutionLog);
        Assert.Contains(result.ExecutionLog, log => log.Contains("Selected joke"));
    }
}

using Imeritas.Agent.DadJokes.Orchestrator;
using Imeritas.Agent.Models;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using AgentTaskStatus = Imeritas.Agent.Models.TaskStatus;

namespace Imeritas.Agent.DadJokes.Tests.Orchestrator;

/// <summary>
/// Unit tests for <see cref="DadJokeOrchestrator"/> covering routing (CanHandle),
/// execution (ExecuteAsync), persistence, and task metadata.
/// </summary>
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

    // ── CanHandle ─────────────────────────────────────────────────────────

    [Fact]
    public void CanHandle_DadJokeTaskType_ReturnsTrue()
    {
        Assert.True(_sut.CanHandle("t1", "dad_joke", "tell me a joke"));
    }

    [Fact]
    public void CanHandle_OtherTaskType_ReturnsFalse()
    {
        Assert.False(_sut.CanHandle("t1", "email", "send email"));
    }

    [Fact]
    public void CanHandle_NullTaskType_ReturnsFalse()
    {
        Assert.False(_sut.CanHandle("t1", null, "hello"));
    }

    // ── ExecuteAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithCategory_ReturnsCompletedTask()
    {
        var inputData = new Dictionary<string, object> { ["category"] = "food" };

        var task = await _sut.ExecuteAsync(
            tenantId: "t1",
            userId: "u1",
            prompt: "tell me a joke",
            taskType: "dad_joke",
            inputData: inputData);

        Assert.Equal(AgentTaskStatus.Completed, task.Status);
        Assert.True(task.OutputData.ContainsKey("result"));
        var result = task.OutputData["result"]?.ToString();
        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public async Task ExecuteAsync_WithoutCategory_ReturnsCompletedTask()
    {
        var task = await _sut.ExecuteAsync(
            tenantId: "t1",
            userId: "u1",
            prompt: "tell me a joke",
            taskType: "dad_joke",
            inputData: new Dictionary<string, object>());

        Assert.Equal(AgentTaskStatus.Completed, task.Status);
        Assert.True(task.OutputData.ContainsKey("result"));
        var result = task.OutputData["result"]?.ToString();
        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public async Task ExecuteAsync_NullInputData_ReturnsCompletedTask()
    {
        var task = await _sut.ExecuteAsync(
            tenantId: "t1",
            userId: "u1",
            prompt: "tell me a joke",
            taskType: "dad_joke",
            inputData: null);

        Assert.Equal(AgentTaskStatus.Completed, task.Status);
        Assert.True(task.OutputData.ContainsKey("result"));
    }

    [Fact]
    public async Task ExecuteAsync_CompletedTask_SetsTimestamps()
    {
        var task = await _sut.ExecuteAsync(
            tenantId: "t1",
            userId: "u1",
            prompt: "tell me a joke",
            taskType: "dad_joke",
            inputData: new Dictionary<string, object>());

        Assert.NotNull(task.StartedAt);
        Assert.NotNull(task.CompletedAt);
        Assert.Equal(AgentTaskStatus.Completed, task.Status);
    }

    [Fact]
    public async Task ExecuteAsync_CallsSaveTaskAsync()
    {
        await _sut.ExecuteAsync(
            tenantId: "t1",
            userId: "u1",
            prompt: "tell me a joke",
            taskType: "dad_joke",
            inputData: new Dictionary<string, object>());

        await _storage.Received().SaveTaskAsync("t1", Arg.Any<AgentTask>());
    }

    [Fact]
    public async Task ExecuteAsync_SetsOutputDataResult()
    {
        var task = await _sut.ExecuteAsync(
            tenantId: "t1",
            userId: "u1",
            prompt: "tell me a joke",
            taskType: "dad_joke",
            inputData: new Dictionary<string, object>());

        var result = task.OutputData["result"]?.ToString();
        Assert.NotNull(result);
        Assert.Contains("\n\n", result);
    }
}

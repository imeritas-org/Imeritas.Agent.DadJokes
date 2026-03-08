using Imeritas.Agent.DadJokes.Orchestrator;
using Imeritas.Agent.DadJokes.Services;
using Imeritas.Agent.Models;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskStatus = Imeritas.Agent.Models.TaskStatus;
using Xunit;

namespace Imeritas.Agent.DadJokes.Tests.Orchestrator;

public class DadJokeOrchestratorTests
{
    private readonly DadJokeOrchestrator _sut;
    private readonly IStorageService _storage;

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
    public void Priority_IsLessThanMaxValue()
    {
        Assert.True(_sut.Priority < int.MaxValue);
    }

    [Fact]
    public void CanHandle_DadJokeTaskType_ReturnsTrue()
    {
        Assert.True(_sut.CanHandle("tenant1", "dad_joke", "tell me a joke"));
    }

    [Fact]
    public void CanHandle_OtherTaskType_ReturnsFalse()
    {
        Assert.False(_sut.CanHandle("tenant1", "email", "tell me a joke"));
    }

    [Fact]
    public void CanHandle_NullTaskType_ReturnsFalse()
    {
        Assert.False(_sut.CanHandle("tenant1", null, "tell me a joke"));
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        Assert.True(_sut.CanHandle("tenant1", "DAD_JOKE", "anything"));
    }

    [Fact]
    public async Task ExecuteAsync_NoCategory_ReturnsCompletedTaskWithJoke()
    {
        var result = await _sut.ExecuteAsync("tenant1", "user1", "tell me a joke", "dad_joke");

        Assert.Equal(TaskStatus.Completed, result.Status);
        Assert.True(result.OutputData.ContainsKey("result"));
        Assert.NotNull(result.OutputData["result"]);
        Assert.Contains("\u2014", result.OutputData["result"].ToString()!);
        await _storage.Received(1).SaveTaskAsync("tenant1", result);
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryInInputData_ReturnsCompletedTask()
    {
        var inputData = new Dictionary<string, object> { ["category"] = "tech" };
        var result = await _sut.ExecuteAsync("tenant1", "user1", "joke please", "dad_joke", inputData);

        Assert.Equal(TaskStatus.Completed, result.Status);
        Assert.True(result.OutputData.ContainsKey("result"));
        await _storage.Received(1).SaveTaskAsync("tenant1", result);
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryInPrompt_ReturnsCompletedTask()
    {
        var result = await _sut.ExecuteAsync("tenant1", "user1", "tell me a tech joke", "dad_joke");

        Assert.Equal(TaskStatus.Completed, result.Status);
        Assert.True(result.OutputData.ContainsKey("result"));
    }

    [Fact]
    public async Task ExecuteAsync_SetsTaskMetadata()
    {
        var result = await _sut.ExecuteAsync("tenant1", "user1", "joke", "dad_joke");

        Assert.Equal("user1", result.UserId);
        Assert.Equal("Dad Joke", result.Title);
        Assert.Equal("DadJoke", result.OrchestratorName);
        Assert.NotNull(result.StartedAt);
        Assert.NotNull(result.CompletedAt);
        Assert.NotEmpty(result.ExecutionLog);
    }

    [Fact]
    public async Task ExecuteAsync_WithParentTaskId_SetsParent()
    {
        var result = await _sut.ExecuteAsync("tenant1", "user1", "joke", "dad_joke",
            parentTaskId: "parent-123");
        Assert.Equal("parent-123", result.ParentTaskId);
    }

    [Fact]
    public async Task ExecuteAsync_PersistsTask()
    {
        var result = await _sut.ExecuteAsync("tenant1", "user1", "joke", "dad_joke");
        await _storage.Received(1).SaveTaskAsync("tenant1", result);
    }

    [Fact]
    public async Task ExecuteAsync_OutputDataContainsJokeIdAndCategories()
    {
        var result = await _sut.ExecuteAsync("tenant1", "user1", "joke", "dad_joke");

        Assert.True(result.OutputData.ContainsKey("jokeId"));
        Assert.True(result.OutputData.ContainsKey("categories"));
    }
}

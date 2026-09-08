using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tasks.Api.Clients;
using Tasks.Api.Controllers;
using Tasks.Api.Data;
using Tasks.Api.Models;
using Xunit;

namespace Tasks.Api.Tests;

public class CreateTaskTests
{
    private TasksDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TasksDbContext(options);
    }

    [Fact]
    public async Task CreateTask_WithValidData_Returns201Created()
    {
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var projectId = Guid.NewGuid();

        mockClient.Setup(c => c.GetProjectByIdAsync(projectId))
            .ReturnsAsync(new ProjectDto { Id = projectId.ToString(), IsArchived = false });

        var controller = new TasksController(mockClient.Object, context);
        var request = new CreateTaskRequest { Title = "New Task" };

        var result = await controller.CreateTask(projectId, request);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var task = Assert.IsType<TaskItem>(createdResult.Value);
        Assert.Equal("New Task", task.Title);
        Assert.Equal("ToDo", task.Status);
    }

    [Fact]
    public async Task CreateTask_WhenProjectNotFound_Returns404NotFound()
    {
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var projectId = Guid.NewGuid();

        mockClient.Setup(c => c.GetProjectByIdAsync(projectId))
            .ReturnsAsync((ProjectDto?)null);

        var controller = new TasksController(mockClient.Object, context);

        var result = await controller.CreateTask(projectId, new CreateTaskRequest { Title = "Test" });

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, ((ProblemDetails)notFoundResult.Value!).Status);
    }

    [Fact]
    public async Task CreateTask_WhenProjectIsArchived_Returns409Conflict()
    {
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var projectId = Guid.NewGuid();

        mockClient.Setup(c => c.GetProjectByIdAsync(projectId))
            .ReturnsAsync(new ProjectDto { Id = projectId.ToString(), IsArchived = true });

        var controller = new TasksController(mockClient.Object, context);

        var result = await controller.CreateTask(projectId, new CreateTaskRequest { Title = "Test" });

        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, ((ProblemDetails)conflictResult.Value!).Status);
    }

    [Fact]
    public async Task CreateTask_WhenProjectsApiUnavailable_Returns502BadGateway()
    {
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var projectId = Guid.NewGuid();

        mockClient.Setup(c => c.GetProjectByIdAsync(projectId))
            .ThrowsAsync(new InvalidOperationException());

        var controller = new TasksController(mockClient.Object, context);

        var result = await controller.CreateTask(projectId, new CreateTaskRequest { Title = "Test" });

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status502BadGateway, statusCodeResult.StatusCode);
    }
}
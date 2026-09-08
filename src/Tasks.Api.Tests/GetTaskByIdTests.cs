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

public class GetTaskByIdTests
{
    private TasksDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TasksDbContext(options);
    }

    [Fact]
    public async Task GetTaskById_WithValidProjectAndTask_Returns200OkWithTask()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var expectedTask = new TaskItem
        {
            Id = taskId.ToString(),
            ProjectId = projectId.ToString(),
            Title = "Valid Task"
        };
        context.Tasks.Add(expectedTask);
        await context.SaveChangesAsync();

        var controller = new TasksController(mockClient.Object, context);

        // Act
        var result = await controller.GetTaskById(projectId, taskId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTask = Assert.IsType<TaskItem>(okResult.Value);

        // Verify it returns 200 OK with the full task object[cite: 4]
        Assert.Equal(taskId.ToString(), returnedTask.Id);
        Assert.Equal(projectId.ToString(), returnedTask.ProjectId);
    }

    [Fact]
    public async Task GetTaskById_WhenTaskDoesNotExist_Returns404NotFound()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var controller = new TasksController(mockClient.Object, context);

        // Act
        var result = await controller.GetTaskById(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        // Verify it returns 404 Not Found for non-existent IDs[cite: 4]
        Assert.Equal(StatusCodes.Status404NotFound, ((ProblemDetails)notFoundResult.Value!).Status);
    }

    [Fact]
    public async Task GetTaskById_WhenTaskBelongsToDifferentProject_Returns404NotFound()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();

        var actualProjectId = Guid.NewGuid();
        var requestedProjectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var task = new TaskItem
        {
            Id = taskId.ToString(),
            ProjectId = actualProjectId.ToString(),
            Title = "Cross-Project Task"
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var controller = new TasksController(mockClient.Object, context);

        // Act - Requesting the task ID but using the WRONG project ID
        var result = await controller.GetTaskById(requestedProjectId, taskId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        // Verify it returns 404 to prevent cross-project access[cite: 4]
        Assert.Equal(StatusCodes.Status404NotFound, ((ProblemDetails)notFoundResult.Value!).Status);
    }
}
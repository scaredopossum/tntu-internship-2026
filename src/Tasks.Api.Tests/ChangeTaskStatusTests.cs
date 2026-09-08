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

public class ChangeTaskStatusTests
{
    private TasksDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TasksDbContext(options);
    }

    [Theory]
    [InlineData("ToDo", "InProgress", StatusCodes.Status200OK)]
    [InlineData("InProgress", "Done", StatusCodes.Status200OK)]
    [InlineData("ToDo", "Done", StatusCodes.Status409Conflict)]
    [InlineData("InProgress", "ToDo", StatusCodes.Status409Conflict)]
    [InlineData("Done", "InProgress", StatusCodes.Status409Conflict)]
    public async Task ChangeTaskStatus_EnforcesStateTransitions(string initialStatus, string targetStatus, int expectedStatusCode)
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var task = new TaskItem
        {
            Id = taskId.ToString(),
            ProjectId = projectId.ToString(),
            Title = "Lifecycle Test Task",
            Status = initialStatus
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var controller = new TasksController(mockClient.Object, context);
        var request = new ChangeTaskStatusRequest { Status = targetStatus };

        // Act
        var result = await controller.ChangeTaskStatus(projectId, taskId, request);

        // Assert
        if (expectedStatusCode == StatusCodes.Status200OK)
        {
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedTask = Assert.IsType<TaskItem>(okResult.Value);
            Assert.Equal(targetStatus, updatedTask.Status);
        }
        else
        {
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(expectedStatusCode, ((ProblemDetails)conflictResult.Value!).Status);
        }
    }

    [Fact]
    public async Task ChangeTaskStatus_WhenTaskNotFound_Returns404NotFound()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var controller = new TasksController(mockClient.Object, context);
        var request = new ChangeTaskStatusRequest { Status = "InProgress" };

        // Act
        var result = await controller.ChangeTaskStatus(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, ((ProblemDetails)notFoundResult.Value!).Status);
    }
}
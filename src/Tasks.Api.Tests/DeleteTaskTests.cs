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

public class DeleteTaskTests
{
    private TasksDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TasksDbContext(options);
    }

    [Fact]
    public async Task DeleteTask_WhenTaskExists_Returns204NoContentAndRemovesTask()
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
            Title = "Task to Delete"
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var controller = new TasksController(mockClient.Object, context);

        // Act
        var result = await controller.DeleteTask(projectId, taskId);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify task is permanently removed[cite: 7]
        var deletedTask = await context.Tasks.FindAsync(taskId.ToString());
        Assert.Null(deletedTask);
    }

    [Fact]
    public async Task DeleteTask_WhenTaskDoesNotExist_Returns404NotFound()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var controller = new TasksController(mockClient.Object, context);

        // Act
        var result = await controller.DeleteTask(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, ((ProblemDetails)notFoundResult.Value!).Status);
    }
}
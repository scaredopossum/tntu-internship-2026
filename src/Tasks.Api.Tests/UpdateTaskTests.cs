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

public class UpdateTaskTests
{
    private TasksDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TasksDbContext(options);
    }

    [Fact]
    public async Task UpdateTask_WithValidData_Returns200OkAndUpdatesFields()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var existingTask = new TaskItem
        {
            Id = taskId.ToString(),
            ProjectId = projectId.ToString(),
            Title = "Old Title",
            Status = "ToDo",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        context.Tasks.Add(existingTask);
        await context.SaveChangesAsync();

        var controller = new TasksController(mockClient.Object, context);
        var request = new UpdateTaskRequest { Title = "Updated Title", Description = "New description" };

        // Act
        var result = await controller.UpdateTask(projectId, taskId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedTask = Assert.IsType<TaskItem>(okResult.Value);

        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal("New description", updatedTask.Description);
        Assert.Equal("ToDo", updatedTask.Status); // Status unchanged[cite: 5]
    }

    [Fact]
    public async Task UpdateTask_WhenTaskDoesNotExist_Returns404NotFound()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var controller = new TasksController(mockClient.Object, context);
        var request = new UpdateTaskRequest { Title = "Updated Title" };

        // Act
        var result = await controller.UpdateTask(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, ((ProblemDetails)notFoundResult.Value!).Status);
    }
}
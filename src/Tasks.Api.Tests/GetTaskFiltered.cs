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

public class GetTaskFilteredTests
{
    private TasksDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TasksDbContext(options);
    }

    [Fact]
    public async Task GetTasksByProject_WithStatusFilter_ReturnsFilteredTasks()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var projectId = Guid.NewGuid();

        mockClient.Setup(c => c.GetProjectByIdAsync(projectId))
            .ReturnsAsync(new ProjectDto { Id = projectId.ToString(), IsArchived = false });

        context.Tasks.AddRange(
            new TaskItem { ProjectId = projectId.ToString(), Title = "Task 1", Status = "ToDo", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new TaskItem { ProjectId = projectId.ToString(), Title = "Task 2", Status = "InProgress", CreatedAt = DateTime.UtcNow.AddDays(-1) }
        );
        await context.SaveChangesAsync();

        var controller = new TasksController(mockClient.Object, context);

        // Act
        var result = await controller.GetTasksByProject(projectId, "InProgress");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value).ToList();

        Assert.Single(tasks);
        Assert.Equal("InProgress", tasks[0].Status);
        Assert.Equal("Task 2", tasks[0].Title);
    }

    [Fact]
    public async Task GetTasksByProject_WithInvalidStatus_Returns400BadRequest()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var projectId = Guid.NewGuid();

        mockClient.Setup(c => c.GetProjectByIdAsync(projectId))
            .ReturnsAsync(new ProjectDto { Id = projectId.ToString(), IsArchived = false });

        var controller = new TasksController(mockClient.Object, context);

        // Act
        var result = await controller.GetTasksByProject(projectId, "InvalidStatus");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
    }
}
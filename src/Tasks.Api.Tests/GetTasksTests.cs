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

public class GetTasksTests
{
    private TasksDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TasksDbContext(options);
    }

    [Fact]
    public async Task GetTasksByProject_WithTasks_Returns200OkOrderedDescending()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var projectId = Guid.NewGuid();

        mockClient.Setup(c => c.GetProjectByIdAsync(projectId))
            .ReturnsAsync(new ProjectDto { Id = projectId.ToString(), IsArchived = false });

        // Add tasks out of order to verify sorting
        var taskOld = new TaskItem { ProjectId = projectId.ToString(), Title = "Old Task", CreatedAt = DateTime.UtcNow.AddDays(-2) };
        var taskNew = new TaskItem { ProjectId = projectId.ToString(), Title = "New Task", CreatedAt = DateTime.UtcNow.AddDays(-1) };
        context.Tasks.AddRange(taskOld, taskNew);
        await context.SaveChangesAsync();

        var controller = new TasksController(mockClient.Object, context);

        // Act
        var result = await controller.GetTasksByProject(projectId, status: null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value).ToList();

        Assert.Equal(2, tasks.Count);
        // Verify tasks are ordered by createdAt descending[cite: 3]
        Assert.Equal("New Task", tasks[0].Title);
        Assert.Equal("Old Task", tasks[1].Title);
    }

    [Fact]
    public async Task GetTasksByProject_WithNoTasks_Returns200OkEmptyArray()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var projectId = Guid.NewGuid();

        mockClient.Setup(c => c.GetProjectByIdAsync(projectId))
            .ReturnsAsync(new ProjectDto { Id = projectId.ToString(), IsArchived = false });

        var controller = new TasksController(mockClient.Object, context);

        // Act
        var result = await controller.GetTasksByProject(projectId, status: null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value);

        // Verify an empty array is returned for projects with no tasks[cite: 3]
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task GetTasksByProject_WhenProjectNotFound_Returns404NotFound()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockClient = new Mock<IProjectsApiClient>();
        var projectId = Guid.NewGuid();

        mockClient.Setup(c => c.GetProjectByIdAsync(projectId))
            .ReturnsAsync((ProjectDto?)null);

        var controller = new TasksController(mockClient.Object, context);

        // Act
        var result = await controller.GetTasksByProject(projectId, status: null);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        // Verify 404 is returned when the project does not exist in Projects.Api[cite: 3]
        Assert.Equal(StatusCodes.Status404NotFound, ((ProblemDetails)notFoundResult.Value!).Status);
    }
}
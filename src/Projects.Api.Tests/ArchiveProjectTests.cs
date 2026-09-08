using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Projects.Api.Controllers;
using Projects.Api.Data;
using Projects.Api.Models;
using Xunit;

namespace Projects.Api.Tests;

public class ArchiveProjectTests
{
    private ProjectsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ProjectsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ProjectsDbContext(options);
    }

    [Fact]
    public async Task ArchiveProject_WhenProjectExists_SetsIsArchivedTrueAndReturns200Ok()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var projectId = Guid.NewGuid();
        var project = new Project { Id = projectId.ToString(), Name = "Active Project", IsArchived = false };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var controller = new ProjectsController(context);

        // Act
        var result = await controller.ArchiveProject(projectId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedProject = Assert.IsType<Project>(okResult.Value);
        Assert.True(updatedProject.IsArchived);
    }

    [Fact]
    public async Task ArchiveProject_WhenProjectDoesNotExist_Returns404NotFound()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new ProjectsController(context);

        // Act
        var result = await controller.ArchiveProject(Guid.NewGuid());

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
    }

    [Fact]
    public async Task ArchiveProject_WhenProjectAlreadyArchived_Returns200OkIdempotent()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var projectId = Guid.NewGuid();
        var project = new Project { Id = projectId.ToString(), Name = "Already Archived", IsArchived = true };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var controller = new ProjectsController(context);

        // Act
        var result = await controller.ArchiveProject(projectId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedProject = Assert.IsType<Project>(okResult.Value);
        Assert.True(updatedProject.IsArchived);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Api_1.Controllers;
using Api_1.Data;
using Api_1.Models;
using Xunit;

namespace Projects.Api.Tests;

public class UpdateProjectTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task UpdateProject_WhenProjectExists_Returns200OkWithUpdatedProject()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var projectId = Guid.NewGuid();
        var project = new Project { Id = projectId.ToString(), Name = "Old Name", IsArchived = false };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var controller = new ProjectsController(context);
        var request = new UpdateProjectRequest { Name = "New Name", Description = "Updated Desc" };

        // Act
        var result = await controller.UpdateProject(projectId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedProject = Assert.IsType<Project>(okResult.Value);
        Assert.Equal("New Name", updatedProject.Name);
        Assert.Equal("Updated Desc", updatedProject.Description);
    }

    [Fact]
    public async Task UpdateProject_WhenProjectDoesNotExist_Returns404NotFound()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new ProjectsController(context);
        var request = new UpdateProjectRequest { Name = "Valid Name" };

        // Act
        var result = await controller.UpdateProject(Guid.NewGuid(), request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
    }

    [Fact]
    public async Task UpdateProject_WhenProjectIsArchived_Returns409Conflict()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var projectId = Guid.NewGuid();
        var archivedProject = new Project { Id = projectId.ToString(), Name = "Archived", IsArchived = true };
        context.Projects.Add(archivedProject);
        await context.SaveChangesAsync();

        var controller = new ProjectsController(context);
        var request = new UpdateProjectRequest { Name = "Valid Name" };

        // Act
        var result = await controller.UpdateProject(projectId, request);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(conflictResult.Value);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
    }
}
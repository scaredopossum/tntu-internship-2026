using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Projects.Api.Controllers;
using Projects.Api.Data;
using Projects.Api.Models;
using Xunit;

namespace Projects.Api.Tests;

public class GetProjectByIdTests
{
    private ProjectsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ProjectsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ProjectsDbContext(options);
    }

    [Fact]
    public async Task GetProjectById_WhenProjectExists_Returns200OkWithProject()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var projectId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId.ToString(),
            Name = "Existing Project",
            IsArchived = true, // Must return the project even if it is archived
            CreatedAt = DateTime.UtcNow
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var controller = new ProjectsController(context);

        // Act
        var result = await controller.GetProjectById(projectId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProject = Assert.IsType<Project>(okResult.Value);

        Assert.Equal(projectId.ToString(), returnedProject.Id);
        Assert.True(returnedProject.IsArchived);
    }

    [Fact]
    public async Task GetProjectById_WhenProjectDoesNotExist_Returns404NotFoundWithProblemDetails()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new ProjectsController(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await controller.GetProjectById(nonExistentId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);

        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("Project not found", problemDetails.Title);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projects.Api.Controllers;
using Projects.Api.Data;
using Projects.Api.Models;
using Xunit;

namespace Projects.Api.Tests;

public class ProjectQueryTests
{
    private ProjectsDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ProjectsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ProjectsDbContext(options);
    }

    [Fact]
    public async Task GetProjects_WhenNoProjectsExist_ReturnsEmptyArray()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var controller = new ProjectsController(context);

        // Act
        var result = await controller.GetProjects();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var projects = Assert.IsAssignableFrom<IEnumerable<Project>>(okResult.Value);
        Assert.Empty(projects);
    }

    [Fact]
    public async Task GetProjects_ReturnsOnlyNonArchivedProjects_OrderedByNewest()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var oldProject = new Project { Name = "Old", IsArchived = false, CreatedAt = DateTime.UtcNow.AddDays(-2) };
        var newProject = new Project { Name = "New", IsArchived = false, CreatedAt = DateTime.UtcNow };
        var archivedProject = new Project { Name = "Archived", IsArchived = true, CreatedAt = DateTime.UtcNow.AddDays(-1) };

        context.Projects.AddRange(oldProject, newProject, archivedProject);
        await context.SaveChangesAsync();

        var controller = new ProjectsController(context);

        // Act
        var result = await controller.GetProjects();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProjects = Assert.IsAssignableFrom<IEnumerable<Project>>(okResult.Value).ToList();

        Assert.Equal(2, returnedProjects.Count); // Archived project is excluded
        Assert.Equal("New", returnedProjects[0].Name); // Newest is first
        Assert.Equal("Old", returnedProjects[1].Name);
    }
}
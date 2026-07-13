using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_1.Controllers;
using Api_1.Data;
using Api_1.Models;
using Xunit;

namespace Api_1.Tests;

public class ProjectFilteringTests
{
    // Допоміжний метод для ініціалізації ізольованого контексту бази даних у пам'яті
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetProjects_WhenArchivedProjectsExist_ShouldExcludeThemFromResponse()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        var activeProject1 = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Active Project A",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };
        var archivedProject = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Archived Project B",
            IsArchived = true,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };
        var activeProject2 = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Active Project C",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        context.Projects.AddRange(activeProject1, archivedProject, activeProject2);
        await context.SaveChangesAsync();

        var controller = new ProjectsController(context);

        // Act
        var result = await controller.GetProjects();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var projects = Assert.IsAssignableFrom<IEnumerable<Project>>(okResult.Value).ToList();

        // Перевірка кількості: повернуто лише 2 проєкти замість 3
        Assert.Equal(2, projects.Count);

        // Перевірка відсутності архівованого проєкту за назвою або ідентифікатором
        Assert.DoesNotContain(projects, p => p.IsArchived);
        Assert.DoesNotContain(projects, p => p.Name == "Archived Project B");
    }

    [Fact]
    public async Task GetProjects_WhenMultipleActiveProjectsExist_ShouldOrderByCreatedAtDescending()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        var oldestProject = new Project { Id = Guid.NewGuid().ToString(), Name = "Oldest", IsArchived = false, CreatedAt = DateTime.UtcNow.AddDays(-5) };
        var newestProject = new Project { Id = Guid.NewGuid().ToString(), Name = "Newest", IsArchived = false, CreatedAt = DateTime.UtcNow };
        var midProject = new Project { Id = Guid.NewGuid().ToString(), Name = "Mid", IsArchived = false, CreatedAt = DateTime.UtcNow.AddDays(-2) };

        context.Projects.AddRange(oldestProject, newestProject, midProject);
        await context.SaveChangesAsync();

        var controller = new ProjectsController(context);

        // Act
        var result = await controller.GetProjects();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var projects = Assert.IsAssignableFrom<IEnumerable<Project>>(okResult.Value).ToList();

        // Перевірка сортування: перший елемент має бути найновішим, останній — найстарішим
        Assert.Equal("Newest", projects[0].Name);
        Assert.Equal("Mid", projects[1].Name);
        Assert.Equal("Oldest", projects[2].Name);
    }
}
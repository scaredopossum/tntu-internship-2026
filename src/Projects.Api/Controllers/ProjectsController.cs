using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projects.Api.Data;
using Projects.Api.Models;

namespace Projects.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")] // Відповідає шляху /api/v1/projects
public class ProjectsController : ControllerBase
{
    private readonly ProjectsDbContext _context;

    public ProjectsController(ProjectsDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Project), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        try
        {
            // Ініціалізація нового проєкту
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            // Збереження в базу даних
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Повертає 201 Created разом із заголовком Location та створеним об'єктом
            return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating project: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    // Допоміжний метод для формування заголовка Location (URI нового ресурсу)
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProjectById(Guid id)
    {
        // Convert the Guid back to string since the Cosmos DB Id property is a string
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id.ToString());

        if (project == null)
        {
            // Returns 404 Not Found formatted as Problem Details
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
                Detail = $"A project with the ID '{id}' could not be found."
            });
        }

        // Returns 200 OK with the full project object (including archived projects)
        return Ok(project);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Project>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjects()
    {
        // Query filters out archived projects and orders by newest first
        var projects = await _context.Projects
            .Where(p => !p.IsArchived)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(projects);
    }
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id.ToString());

        if (project == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
                Detail = $"A project with the ID '{id}' could not be found."
            });
        }

        if (project.IsArchived)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Project is archived",
                Detail = "Archived projects cannot be updated."
            });
        }

        project.Name = request.Name;
        project.Description = request.Description;

        await _context.SaveChangesAsync();

        return Ok(project);
    }

    [HttpPatch("{id:guid}/archive")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveProject(Guid id)
    {
        var project = await _context.Projects.FindAsync(id.ToString());

        if (project == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
                Detail = $"A project with the ID '{id}' could not be found."
            });
        }

        if (!project.IsArchived)
        {
            project.IsArchived = true;
            await _context.SaveChangesAsync();
        }

        return Ok(project);
    }
}
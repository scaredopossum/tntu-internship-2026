using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_1.Data;
using Api_1.Models;

namespace Api_1.Controllers;

[ApiController]
[Route("api/v1/[controller]")] // Відповідає шляху /api/v1/projects
public class ProjectsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProjectsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Project), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
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

    // Допоміжний метод для формування заголовка Location (URI нового ресурсу)
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProjectById(Guid id)
    {
        // Convert the Guid back to string since the Cosmos DB Id property is a string
        var project = await _context.Projects.FindAsync(id.ToString());

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
}
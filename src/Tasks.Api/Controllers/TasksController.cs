using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tasks.Api.Clients;
using Tasks.Api.Data;
using Tasks.Api.Models;

namespace Tasks.Api.Controllers;

[ApiController]
[Route("api/v1/projects/{projectId:guid}/tasks")]
public class TasksController : ControllerBase
{
    private readonly IProjectsApiClient _projectsClient;
    private readonly TasksDbContext _context;

    public TasksController(IProjectsApiClient projectsClient, TasksDbContext context)
    {
        _projectsClient = projectsClient;
        _context = context;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CreateTask(Guid projectId, [FromBody] CreateTaskRequest request)
    {
        ProjectDto? project;
        try
        {
            project = await _projectsClient.GetProjectByIdAsync(projectId);
        }
        catch (InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = "Bad Gateway",
                Detail = "The Projects service is currently unavailable."
            });
        }

        if (project == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
                Detail = $"A project with the ID '{projectId}' could not be found."
            });
        }

        if (project.IsArchived)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Project is archived",
                Detail = "Cannot create tasks for an archived project."
            });
        }

        var task = new TaskItem
        {
            ProjectId = projectId.ToString(),
            Title = request.Title,
            Description = request.Description,
            Assignee = request.Assignee,
            DueDate = request.DueDate,
            Status = "ToDo",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return Created($"/api/v1/projects/{projectId}/tasks/{task.Id}", task);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetTasksByProject(Guid projectId, [FromQuery] string? status)
    {
        // Validate optional status query parameter if provided
        if (!string.IsNullOrEmpty(status))
        {
            var allowedStatuses = new[] { "ToDo", "InProgress", "Done" };
            if (!allowedStatuses.Contains(status))
            {
                return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                { "status", new[] { "Status must be ToDo, InProgress, or Done." } }
            })
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid status filter",
                    Detail = $"The status '{status}' is not valid. Allowed values are: ToDo, InProgress, Done."
                });
            }
        }

        ProjectDto? project;
        try
        {
            project = await _projectsClient.GetProjectByIdAsync(projectId);
        }
        catch (InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = "Bad Gateway",
                Detail = "The Projects service is currently unavailable."
            });
        }

        // Validate project exists before returning tasks[cite: 3]
        if (project == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
                Detail = $"A project with the ID '{projectId}' could not be found."
            });
        }

        var query = _context.Tasks
            .Where(t => t.ProjectId == projectId.ToString());

        // Apply status filter if provided (US-016)[cite: 12]
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status == status);
        }

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{taskId:guid}")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(Guid projectId, Guid taskId)
    {
        // Efficient Cosmos query using document id and partition key[cite: 4]
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId.ToString() && t.ProjectId == projectId.ToString());

        // Handles missing tasks, mismatched projects, and non-existent project IDs uniformly[cite: 4]
        if (task == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Task not found",
                Detail = $"Task '{taskId}' was not found in project '{projectId}'."
            });
        }

        // Returns the full task object[cite: 4]
        return Ok(task);
    }

    [HttpPut("{taskId:guid}")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(Guid projectId, Guid taskId, [FromBody] UpdateTaskRequest request)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId.ToString() && t.ProjectId == projectId.ToString());

        if (task == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Task not found",
                Detail = $"Task '{taskId}' was not found in project '{projectId}'."
            });
        }

        // Update allowed fields; status, id, projectId, and createdAt remain untouched
        task.Title = request.Title;
        task.Description = request.Description;
        task.Assignee = request.Assignee;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow; // Set to current UTC on every update[cite: 5]

        await _context.SaveChangesAsync();

        return Ok(task);
    }

    [HttpPatch("{taskId:guid}/status")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeTaskStatus(Guid projectId, Guid taskId, [FromBody] ChangeTaskStatusRequest request)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId.ToString() && t.ProjectId == projectId.ToString());

        if (task == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Task not found",
                Detail = $"Task '{taskId}' was not found in project '{projectId}'."
            });
        }

        // Enforce allowed state transitions: ToDo -> InProgress, InProgress -> Done
        bool isValidTransition = (task.Status, request.Status) switch
        {
            ("ToDo", "InProgress") => true,
            ("InProgress", "Done") => true,
            _ => false
        };

        if (!isValidTransition)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Invalid status transition",
                Detail = $"Transitioning status from '{task.Status}' to '{request.Status}' is not allowed."
            });
        }

        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow; // UpdatedAt is refreshed on a successful status change[cite: 6]

        await _context.SaveChangesAsync();

        return Ok(task);
    }

    [HttpDelete("{taskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid projectId, Guid taskId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId.ToString() && t.ProjectId == projectId.ToString());

        if (task == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Task not found",
                Detail = $"Task '{taskId}' was not found in project '{projectId}'."
            });
        }

        // Permanently remove the task from Cosmos DB (domain rule BR-T08)
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
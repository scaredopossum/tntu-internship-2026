using Microsoft.AspNetCore.Mvc;
using Tasks.Api.Data;

namespace Tasks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly TasksDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(TasksDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("db")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CheckDatabase()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();
            _logger.LogInformation("Database connection check successful");
            return Ok(new { status = "healthy", message = "Database connection successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection check failed");

            // Format the 503 response as an RFC 7807 Problem Details object
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Service Unavailable",
                Detail = ex.Message
            });
        }
    }
}
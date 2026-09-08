using Microsoft.AspNetCore.Mvc;
using Tasks.Api.Data;

namespace Tasks.Api.Controllers
{
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
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                // Cosmos doesn't support CanConnectAsync, so we attempt a simple query
                await _context.Database.EnsureCreatedAsync();
                _logger.LogInformation("Database connection check successful");
                return Ok(new { status = "healthy", message = "Database connection successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection check failed");
                return StatusCode(503, new { status = "unhealthy", message = ex.Message });
            }
        }
    }
}

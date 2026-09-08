using Microsoft.AspNetCore.Mvc;
using Api_1.Data;

namespace Api_1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("db")]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                // Cosmos doesn't support CanConnectAsync, so we attempt to ensure database is created
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

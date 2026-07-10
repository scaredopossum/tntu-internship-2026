using Microsoft.AspNetCore.Mvc;
using Api_1;
using Api_1.Data; // Переконайтеся, що це простір імен вашого проекту

[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    // Впровадження залежності (Dependency Injection) через конструктор
    public DatabaseController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("test-connection")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();

            return Ok("Successfully connected to Cosmos DB.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Connection failed: {ex.Message}");
        }
    }
}
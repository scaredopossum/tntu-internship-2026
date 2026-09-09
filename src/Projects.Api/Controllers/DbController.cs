using Microsoft.AspNetCore.Mvc;
using Projects.Api;
using Projects.Api.Data; // Переконайтеся, що це простір імен вашого проекту

[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly ProjectsDbContext _context;

    // Впровадження залежності (Dependency Injection) через конструктор
    public DatabaseController(ProjectsDbContext context)
    {
        _context = context;
    }

    //[HttpGet("test-connection")]
    //public async Task<IActionResult> TestConnection()
    //{
    //    try
    //    {
    //        await _context.Database.EnsureCreatedAsync();

    //        return Ok("Successfully connected to Cosmos DB.");
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, $"Connection failed: {ex.Message}");
    //    }
    //}
}
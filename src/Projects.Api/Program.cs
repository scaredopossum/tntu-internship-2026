using Azure.Identity;

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Projects.Api;
using Projects.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add user secrets BEFORE reading configuration
//if (builder.Environment.IsDevelopment())
//{
    builder.Configuration.AddUserSecrets<Program>();
//}

string endpoint = builder.Configuration["CosmosDb:AccountEndpoint"] ?? throw new InvalidOperationException("CosmosDb:AccountEndpoint is not configured");
string key = builder.Configuration["CosmosDb:AccountKey"] ?? throw new InvalidOperationException("CosmosDb:AccountKey is not configured");
string databaseName = "TaskBoard";

// Validate configuration
if (string.IsNullOrWhiteSpace(endpoint))
    throw new InvalidOperationException("CosmosDb:AccountEndpoint cannot be empty");
if (string.IsNullOrWhiteSpace(key))
    throw new InvalidOperationException("CosmosDb:AccountKey cannot be empty");

// Normalize endpoint - remove trailing slash if present
endpoint = endpoint.TrimEnd('/');

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProjectsDbContext>(options =>
    options.UseCosmos(endpoint, key, databaseName, cosmosOptions =>
    {
        cosmosOptions.ConnectionMode(Microsoft.Azure.Cosmos.ConnectionMode.Gateway);
    })
);

builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Enable Swagger across all environments (Development & Production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Projects API V1");
    // Optional: Sets Swagger as the root landing page (e.g., https://your-app.azurewebsites.net/)
    c.RoutePrefix = string.Empty;
});

// Fix HTTPS redirect for Swagger in non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();

// Log configuration for debugging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Cosmos DB Endpoint: {Endpoint}", endpoint);

// Initialize database with error handling
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<Projects.Api.Data.ProjectsDbContext>();
        await context.Database.EnsureCreatedAsync(); // Створює базу та контейнер, якщо вони відсутні
        logger.LogInformation("Database initialization successful");
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while initializing the database. Endpoint: {Endpoint}, Message: {Message}, InnerException: {InnerException}", endpoint, ex.Message, ex.InnerException?.Message);
    // Don't throw in development - allow API to run for debugging
    if (app.Environment.IsProduction())
    {
        throw;
    }
}



app.Run();

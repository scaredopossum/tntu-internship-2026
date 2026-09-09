using Microsoft.EntityFrameworkCore;
using Tasks.Api.Clients;
using Tasks.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add user secrets BEFORE reading configuration
builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the typed HttpClient
builder.Services.AddHttpClient<IProjectsApiClient, ProjectsApiClient>(client =>
{
    var baseUrl = builder.Configuration["Services:ProjectsApiUrl"];
    if (string.IsNullOrEmpty(baseUrl))
    {
        throw new InvalidOperationException("ProjectsApiUrl is not configured.");
    }

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register TasksDbContext
string endpoint = builder.Configuration["CosmosDb:AccountEndpoint"] ?? throw new InvalidOperationException("CosmosDb:AccountEndpoint is not configured");
string key = builder.Configuration["CosmosDb:AccountKey"] ?? throw new InvalidOperationException("CosmosDb:AccountKey is not configured");
string databaseName = builder.Configuration["CosmosDb:DatabaseName"] ?? "TaskBoard";

// Validate configuration
if (string.IsNullOrWhiteSpace(endpoint))
    throw new InvalidOperationException("CosmosDb:AccountEndpoint cannot be empty");
if (string.IsNullOrWhiteSpace(key))
    throw new InvalidOperationException("CosmosDb:AccountKey cannot be empty");

endpoint = endpoint.TrimEnd('/');

builder.Services.AddDbContext<TasksDbContext>(options =>
    options.UseCosmos(endpoint, key, databaseName, cosmosOptions =>
    {
        cosmosOptions.ConnectionMode(Microsoft.Azure.Cosmos.ConnectionMode.Gateway);
    })
);

// Enable standard RFC 7807 Problem Details for all unhandled errors and framework validations[cite: 9]
builder.Services.AddProblemDetails();

var app = builder.Build();

// Enable Swagger across all environments (Development & Production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tasks API V1");
    // Sets Swagger as the root landing page
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
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
        var context = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        await context.Database.EnsureCreatedAsync();
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
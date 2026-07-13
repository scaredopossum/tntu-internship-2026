using Azure.Identity;

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Api_1;
using Api_1.Data;

var builder = WebApplication.CreateBuilder(args);

string endpoint = builder.Configuration["CosmosDb:AccountEndpoint"];
string key = builder.Configuration["CosmosDb:AccountKey"];
string databaseName = "TaskBoard";

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseCosmos(endpoint, key, databaseName)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

Console.WriteLine($"Endpoint: {endpoint}");
Console.WriteLine($"Key: {key}");

// Initialize database with error handling
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<Api_1.Data.ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(); // Створює базу та контейнер, якщо вони відсутні
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while initializing the database.");
    throw;
}

app.Run();

using Projects.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Projects.Api.Data;

public class ProjectsDbContext : DbContext
{
    public ProjectsDbContext(DbContextOptions<ProjectsDbContext> options)
        : base(options)
    {
    }
    public DbSet<Project> Projects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Project entity for Cosmos DB
        modelBuilder.Entity<Project>()
            .ToContainer("projects")
            .HasPartitionKey(p => p.Id)
            .HasNoDiscriminator(); // Disable EF Core's default discriminator
    }
}
using Api_1.Models;
using Microsoft.EntityFrameworkCore;

namespace Api_1.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Project> Projects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Project>()
            .ToContainer("projects") 
            .HasPartitionKey(p => p.Id); 
    }
}
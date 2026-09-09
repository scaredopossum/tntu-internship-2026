using Microsoft.EntityFrameworkCore;
using Tasks.Api.Models;

namespace Tasks.Api.Data;

public class TasksDbContext : DbContext
{
    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>()
            .ToContainer("tasks")
            .HasPartitionKey(t => t.ProjectId);

        // Явне зіставлення властивості C# з назвою поля JSON у Cosmos DB
        modelBuilder.Entity<TaskItem>()
            .Property(t => t.ProjectId)
            .ToJsonProperty("projectId");

        // Рекомендується також явно вказати мапінг для Id, 
        // оскільки Cosmos DB вимагає, щоб ідентифікатор документа був виключно у нижньому регістрі "id"
        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Id)
            .ToJsonProperty("id");
    }
}
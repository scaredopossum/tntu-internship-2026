using System.ComponentModel.DataAnnotations;

namespace Api_1.Models;

public class Project
{
    [Key] // Вказує, що це первинний ключ
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
using System.ComponentModel.DataAnnotations;

namespace Tasks.Api.Models;

public class CreateTaskRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Assignee { get; set; }
    public DateTime? DueDate { get; set; }
}
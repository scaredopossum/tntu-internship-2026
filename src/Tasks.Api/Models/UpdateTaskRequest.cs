using System.ComponentModel.DataAnnotations;

namespace Tasks.Api.Models;

public class UpdateTaskRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    [MaxLength(200, ErrorMessage = "Assignee cannot exceed 200 characters.")]
    public string? Assignee { get; set; }

    public DateTime? DueDate { get; set; }
}
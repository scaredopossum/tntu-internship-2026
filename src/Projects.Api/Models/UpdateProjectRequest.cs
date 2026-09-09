using System.ComponentModel.DataAnnotations;

namespace Projects.Api.Models;

public class UpdateProjectRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required and cannot be empty.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Projects.Api.Models;

public class CreateProjectRequest
{
    [Required(ErrorMessage = "Name is required")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or consist only of whitespace")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}
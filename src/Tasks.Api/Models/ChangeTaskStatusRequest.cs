using System.ComponentModel.DataAnnotations;

namespace Tasks.Api.Models;

public class ChangeTaskStatusRequest
{
    [Required]
    [RegularExpression("^(ToDo|InProgress|Done)$", ErrorMessage = "Status must be ToDo, InProgress, or Done.")]
    public string Status { get; set; } = string.Empty;
}
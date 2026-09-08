namespace Tasks.Api.Clients;

public class ProjectDto
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public bool IsArchived { get; set; }
}
namespace Tasks.Api.Clients;

public interface IProjectsApiClient
{
    Task<ProjectDto?> GetProjectByIdAsync(Guid id);
}
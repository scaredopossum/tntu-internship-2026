using System.Net;

namespace Tasks.Api.Clients;

public class ProjectsApiClient : IProjectsApiClient
{
    private readonly HttpClient _httpClient;

    public ProjectsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/projects/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProjectDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Projects API is unavailable.", ex);
        }
    }
}
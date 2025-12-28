using App.Domain.Entities;

namespace App.Application.Interfaces.Services;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllProjectsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default);
    Task<Project?> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Project?> GetProjectWithTasksAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Project> CreateProjectAsync(Project project, CancellationToken cancellationToken = default);
    Task UpdateProjectAsync(Project project, CancellationToken cancellationToken = default);
    Task DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ValidateProjectAsync(Project project);
    Task<bool> IsKeyUniqueAsync(string key, Guid? excludeProjectId = null, CancellationToken cancellationToken = default);
}

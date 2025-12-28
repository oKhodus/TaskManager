using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace App.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IProjectRepository projectRepository,
        ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _projectRepository.ListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _projectRepository.GetActiveProjectsAsync(cancellationToken);
    }

    public async Task<Project?> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _projectRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Project?> GetProjectWithTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _projectRepository.GetWithTasksAsync(id, cancellationToken);
    }

    public async Task<Project> CreateProjectAsync(Project project, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating project: Name={Name}, Key={Key}", project.Name, project.Key);

        // Business logic: Set creation timestamp
        project.CreatedAt = DateTime.UtcNow;
        project.IsActive = true;

        var result = await _projectRepository.AddAsync(project, cancellationToken);
        _logger.LogInformation("Project created successfully: Id={ProjectId}, Name={Name}", result.Id, result.Name);
        return result;
    }

    public async Task UpdateProjectAsync(Project project, CancellationToken cancellationToken = default)
    {
        // Business logic: Set update timestamp
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project, cancellationToken);
    }

    public async Task DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting project (soft delete): Id={ProjectId}", id);
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project != null)
        {
            var originalKey = project.Key;

            // Business logic: Soft delete + release Key for reuse
            project.IsActive = false;
            project.UpdatedAt = DateTime.UtcNow;

            // Append timestamp to Key to free it for new projects (avoids UNIQUE constraint issues)
            project.Key = $"{project.Key}_deleted_{DateTime.UtcNow:yyyyMMddHHmmss}";

            await _projectRepository.UpdateAsync(project, cancellationToken);
            _logger.LogInformation("Project soft-deleted successfully: Id={ProjectId}, Name={Name}, OriginalKey={OriginalKey}, NewKey={NewKey}",
                id, project.Name, originalKey, project.Key);
        }
        else
        {
            _logger.LogWarning("Project not found for deletion: Id={ProjectId}", id);
        }
    }

    public Task<bool> ValidateProjectAsync(Project project)
    {
        // Business logic: Validate project
        var isValid = !string.IsNullOrWhiteSpace(project.Name) &&
                     !string.IsNullOrWhiteSpace(project.Key) &&
                     project.Key.Length <= 10;

        return Task.FromResult(isValid);
    }

    public async Task<bool> IsKeyUniqueAsync(string key, Guid? excludeProjectId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Validating project key uniqueness: Key={ProjectKey}, ExcludeProjectId={ExcludeProjectId}", key, excludeProjectId);

        var isUnique = await _projectRepository.IsKeyUniqueAsync(key, excludeProjectId, cancellationToken);

        if (isUnique)
            _logger.LogDebug("Project key {ProjectKey} validation passed - key is unique", key);
        else
            _logger.LogInformation("Project key {ProjectKey} validation failed - key already exists", key);

        return isUnique;
    }
}

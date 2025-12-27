using App.Application.Interfaces.Repositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Repositories;

public class ProjectRepository : RepositoryBase<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context, ILogger<ProjectRepository> logger) : base(context, logger)
    {
    }

    public async Task<Project?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching project with key {ProjectKey}", key);

        var project = await _dbSet
            .FirstOrDefaultAsync(p => p.Key == key, cancellationToken);

        if (project != null)
            _logger.LogDebug("Retrieved project with key {ProjectKey} and ID {ProjectId}", key, project.Id);
        else
            _logger.LogDebug("Project with key {ProjectKey} not found", key);

        return project;
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching active projects");

        var projects = await _dbSet
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {ProjectCount} active projects", projects.Count);
        return projects;
    }

    public async Task<Project?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching project with tasks for project ID {ProjectId}", id);

        var project = await _dbSet
            .Include(p => p.Tasks)
            .ThenInclude(t => t.AssignedTo)
            .Include(p => p.Tasks)
            .ThenInclude(t => t.Tags)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project != null)
            _logger.LogDebug("Retrieved project with {TaskCount} tasks for project ID {ProjectId}", project.Tasks?.Count ?? 0, id);
        else
            _logger.LogDebug("Project with ID {ProjectId} not found", id);

        return project;
    }

    public async Task<Project?> GetWithSprintsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching project with sprints for project ID {ProjectId}", id);

        var project = await _dbSet
            .Include(p => p.Sprints)
            .ThenInclude(s => s.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project != null)
            _logger.LogDebug("Retrieved project with {SprintCount} sprints for project ID {ProjectId}", project.Sprints?.Count ?? 0, id);
        else
            _logger.LogDebug("Project with ID {ProjectId} not found", id);

        return project;
    }
}

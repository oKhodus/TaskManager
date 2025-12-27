using App.Application.Interfaces.Repositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Repositories;

public class SprintRepository : RepositoryBase<Sprint>, ISprintRepository
{
    public SprintRepository(ApplicationDbContext context, ILogger<SprintRepository> logger) : base(context, logger)
    {
    }

    public async Task<IEnumerable<Sprint>> GetActiveSprintsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching active sprints");

        var sprints = await _dbSet
            .Include(s => s.Project)
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {SprintCount} active sprints", sprints.Count);
        return sprints;
    }

    public async Task<IEnumerable<Sprint>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching sprints for project ID {ProjectId}", projectId);

        var sprints = await _dbSet
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {SprintCount} sprints for project ID {ProjectId}", sprints.Count, projectId);
        return sprints;
    }

    public async Task<Sprint?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching sprint with tasks for sprint ID {SprintId}", id);

        var sprint = await _dbSet
            .Include(s => s.Tasks)
            .ThenInclude(t => t.AssignedTo)
            .Include(s => s.Tasks)
            .ThenInclude(t => t.Tags)
            .Include(s => s.Project)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (sprint != null)
            _logger.LogDebug("Retrieved sprint with {TaskCount} tasks for sprint ID {SprintId}", sprint.Tasks?.Count ?? 0, id);
        else
            _logger.LogDebug("Sprint with ID {SprintId} not found", id);

        return sprint;
    }

    public async Task<Sprint?> GetCurrentSprintForProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching current sprint for project ID {ProjectId}", projectId);

        var now = DateTime.UtcNow;
        var sprint = await _dbSet
            .Include(s => s.Tasks)
            .Where(s => s.ProjectId == projectId &&
                       s.IsActive &&
                       s.StartDate <= now &&
                       s.EndDate >= now)
            .FirstOrDefaultAsync(cancellationToken);

        if (sprint != null)
            _logger.LogDebug("Retrieved current sprint with ID {SprintId} for project ID {ProjectId}", sprint.Id, projectId);
        else
            _logger.LogDebug("No current sprint found for project ID {ProjectId}", projectId);

        return sprint;
    }
}

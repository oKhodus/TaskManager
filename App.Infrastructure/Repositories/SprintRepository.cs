using App.Application.Interfaces.Repositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class SprintRepository : RepositoryBase<Sprint>, ISprintRepository
{
    public SprintRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Sprint>> GetActiveSprintsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Project)
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Sprint>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sprint?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Tasks)
            .ThenInclude(t => t.AssignedTo)
            .Include(s => s.Tasks)
            .ThenInclude(t => t.Tags)
            .Include(s => s.Project)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sprint?> GetCurrentSprintForProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(s => s.Tasks)
            .Where(s => s.ProjectId == projectId &&
                       s.IsActive &&
                       s.StartDate <= now &&
                       s.EndDate >= now)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

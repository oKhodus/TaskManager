using App.Application.Interfaces.Repositories;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TaskStatus = App.Domain.Enums.TaskStatus;

namespace App.Infrastructure.Repositories;

public class TaskRepository : RepositoryBase<TaskBase>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TaskBase>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Tags)
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskBase>> GetBySprintIdAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Tags)
            .Where(t => t.SprintId == sprintId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskBase>> GetByAssignedUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Sprint)
            .Include(t => t.Tags)
            .Where(t => t.AssignedToId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskBase>> GetByStatusAsync(TaskStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.AssignedTo)
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskBase>> GetByPriorityAsync(TaskPriority priority, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.AssignedTo)
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .Where(t => t.Priority == priority)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskBase>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.AssignedTo)
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .Where(t => t.Title.Contains(searchTerm) ||
                       t.Description.Contains(searchTerm))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskBase?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Sprint)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
}

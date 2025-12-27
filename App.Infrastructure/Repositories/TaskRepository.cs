using App.Application.Interfaces.Repositories;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskStatus = App.Domain.Enums.TaskStatus;

namespace App.Infrastructure.Repositories;

public class TaskRepository : RepositoryBase<TaskBase>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context, ILogger<TaskRepository> logger) : base(context, logger)
    {
    }

    public async Task<IEnumerable<TaskBase>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching tasks for project with ID {ProjectId}", projectId);

        var tasks = await _dbSet
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Tags)
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {TaskCount} tasks for project with ID {ProjectId}", tasks.Count, projectId);
        return tasks;
    }

    public async Task<IEnumerable<TaskBase>> GetBySprintIdAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching tasks for sprint with ID {SprintId}", sprintId);

        var tasks = await _dbSet
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Tags)
            .Where(t => t.SprintId == sprintId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {TaskCount} tasks for sprint with ID {SprintId}", tasks.Count, sprintId);
        return tasks;
    }

    public async Task<IEnumerable<TaskBase>> GetByAssignedUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching tasks assigned to user with ID {UserId}", userId);

        var tasks = await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Sprint)
            .Include(t => t.Tags)
            .Where(t => t.AssignedToId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {TaskCount} tasks assigned to user with ID {UserId}", tasks.Count, userId);
        return tasks;
    }

    public async Task<IEnumerable<TaskBase>> GetByStatusAsync(TaskStatus status, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching tasks with status {Status}", status);

        var tasks = await _dbSet
            .Include(t => t.AssignedTo)
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {TaskCount} tasks with status {Status}", tasks.Count, status);
        return tasks;
    }

    public async Task<IEnumerable<TaskBase>> GetByPriorityAsync(TaskPriority priority, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching tasks with priority {Priority}", priority);

        var tasks = await _dbSet
            .Include(t => t.AssignedTo)
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .Where(t => t.Priority == priority)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {TaskCount} tasks with priority {Priority}", tasks.Count, priority);
        return tasks;
    }

    public async Task<IEnumerable<TaskBase>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Searching tasks with term {SearchTerm}", searchTerm);

        var tasks = await _dbSet
            .Include(t => t.AssignedTo)
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .Where(t => t.Title.Contains(searchTerm) ||
                       t.Description.Contains(searchTerm))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {TaskCount} tasks matching search term {SearchTerm}", tasks.Count, searchTerm);
        return tasks;
    }

    public async Task<TaskBase?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching task details for task with ID {TaskId}", id);

        var task = await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Sprint)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task != null)
            _logger.LogDebug("Retrieved task details for task with ID {TaskId}", id);
        else
            _logger.LogDebug("Task with ID {TaskId} not found", id);

        return task;
    }
}

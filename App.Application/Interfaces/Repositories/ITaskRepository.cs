using App.Domain.Entities;
using App.Domain.Enums;
using TaskStatus = App.Domain.Enums.TaskStatus;

namespace App.Application.Interfaces.Repositories;

public interface ITaskRepository : IRepository<TaskBase>
{
    Task<IEnumerable<TaskBase>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskBase>> GetBySprintIdAsync(Guid sprintId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskBase>> GetByAssignedUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskBase>> GetByStatusAsync(TaskStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskBase>> GetByPriorityAsync(TaskPriority priority, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskBase>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<TaskBase?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}

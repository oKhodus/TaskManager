using App.Domain.Entities;
using App.Domain.Enums;
using TaskStatus = App.Domain.Enums.TaskStatus;

namespace App.Application.Interfaces.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskBase>> GetAllTasksAsync(CancellationToken cancellationToken = default);
    Task<TaskBase?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskBase>> GetTasksByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskBase>> GetTasksBySprintIdAsync(Guid sprintId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskBase>> GetTasksByStatusAsync(TaskStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskBase>> GetTasksByPriorityAsync(TaskPriority priority, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskBase>> SearchTasksAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<TaskBase> CreateTaskAsync(TaskBase task, CancellationToken cancellationToken = default);
    Task UpdateTaskAsync(TaskBase task, CancellationToken cancellationToken = default);
    Task DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ValidateTaskAsync(TaskBase task);
    Task AssignTaskToUserAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
    Task ChangeTaskStatusAsync(Guid taskId, TaskStatus newStatus, CancellationToken cancellationToken = default);
}

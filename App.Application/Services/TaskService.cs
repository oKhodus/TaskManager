using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using Microsoft.Extensions.Logging;
using TaskStatus = App.Domain.Enums.TaskStatus;

namespace App.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository taskRepository,
        IUserRepository userRepository,
        ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskBase>> GetAllTasksAsync(CancellationToken cancellationToken = default)
    {
        return await _taskRepository.ListAsync(cancellationToken);
    }

    public async Task<TaskBase?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _taskRepository.GetWithDetailsAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<TaskBase>> GetTasksByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _taskRepository.GetByProjectIdAsync(projectId, cancellationToken);
    }

    public async Task<IEnumerable<TaskBase>> GetTasksBySprintIdAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        return await _taskRepository.GetBySprintIdAsync(sprintId, cancellationToken);
    }

    public async Task<IEnumerable<TaskBase>> GetTasksByStatusAsync(TaskStatus status, CancellationToken cancellationToken = default)
    {
        return await _taskRepository.GetByStatusAsync(status, cancellationToken);
    }

    public async Task<IEnumerable<TaskBase>> GetTasksByPriorityAsync(TaskPriority priority, CancellationToken cancellationToken = default)
    {
        return await _taskRepository.GetByPriorityAsync(priority, cancellationToken);
    }

    public async Task<IEnumerable<TaskBase>> SearchTasksAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllTasksAsync(cancellationToken);
        }

        return await _taskRepository.SearchAsync(searchTerm, cancellationToken);
    }

    public async Task<TaskBase> CreateTaskAsync(TaskBase task, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating task: {Title}, ProjectId={ProjectId}, Type={TaskType}",
            task.Title, task.ProjectId, task.GetType().Name);

        // Business logic: Set creation timestamp and default status
        task.CreatedAt = DateTime.UtcNow;
        if (task.Status == default)
        {
            task.Status = TaskStatus.Todo;
        }
        if (task.Priority == default)
        {
            task.Priority = TaskPriority.Medium;
        }

        var result = await _taskRepository.AddAsync(task, cancellationToken);
        _logger.LogInformation("Task created successfully: Id={TaskId}, Title={Title}",
            result.Id, result.Title);
        return result;
    }

    public async Task UpdateTaskAsync(TaskBase task, CancellationToken cancellationToken = default)
    {
        // Business logic: Set update timestamp
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task, cancellationToken);
    }

    public async Task DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting task: Id={TaskId}", id);
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task != null)
        {
            await _taskRepository.DeleteAsync(task, cancellationToken);
            _logger.LogInformation("Task deleted successfully: Id={TaskId}, Title={Title}", id, task.Title);
        }
        else
        {
            _logger.LogWarning("Task not found for deletion: Id={TaskId}", id);
        }
    }

    public Task<bool> ValidateTaskAsync(TaskBase task)
    {
        // Business logic: Validate task
        var isValid = !string.IsNullOrWhiteSpace(task.Title) &&
                     task.ProjectId != Guid.Empty;

        return Task.FromResult(isValid);
    }

    public async Task AssignTaskToUserAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning task {TaskId} to user {UserId}", taskId, userId);
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (task != null && user != null)
        {
            task.AssignedToId = userId;
            task.UpdatedAt = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);
            _logger.LogInformation("Task assigned successfully: TaskId={TaskId}, UserId={UserId}, Username={Username}",
                taskId, userId, user.Username);
        }
        else
        {
            _logger.LogWarning("Failed to assign task: Task or User not found. TaskId={TaskId}, UserId={UserId}",
                taskId, userId);
        }
    }

    public async Task ChangeTaskStatusAsync(Guid taskId, TaskStatus newStatus, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);

        if (task != null)
        {
            var oldStatus = task.Status;
            task.Status = newStatus;
            task.UpdatedAt = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);
            _logger.LogInformation("Task status changed: TaskId={TaskId}, OldStatus={OldStatus}, NewStatus={NewStatus}",
                taskId, oldStatus, newStatus);
        }
        else
        {
            _logger.LogWarning("Task not found for status change: TaskId={TaskId}", taskId);
        }
    }
}

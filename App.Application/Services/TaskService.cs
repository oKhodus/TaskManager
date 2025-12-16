using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using TaskStatus = App.Domain.Enums.TaskStatus;

namespace App.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
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

        return await _taskRepository.AddAsync(task, cancellationToken);
    }

    public async Task UpdateTaskAsync(TaskBase task, CancellationToken cancellationToken = default)
    {
        // Business logic: Set update timestamp
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task, cancellationToken);
    }

    public async Task DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task != null)
        {
            await _taskRepository.DeleteAsync(task, cancellationToken);
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
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (task != null && user != null)
        {
            task.AssignedToId = userId;
            task.UpdatedAt = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);
        }
    }

    public async Task ChangeTaskStatusAsync(Guid taskId, TaskStatus newStatus, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);

        if (task != null)
        {
            task.Status = newStatus;
            task.UpdatedAt = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);
        }
    }
}

using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using App.Domain.Enums;

namespace App.Application.Services;

/// <summary>
/// Implementation of Kanban board service
/// Handles task organization and status transitions for visual workflow
/// </summary>
public class KanbanService : IKanbanService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;

    public KanbanService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Dictionary<Domain.Enums.TaskStatus, List<TaskBase>>> GetKanbanBoardAsync(
        Guid projectId,
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        // Get all tasks for the selected project
        var allTasks = await _taskRepository.ListAsync(cancellationToken);

        // Filter by project
        var projectTasks = allTasks.Where(t => t.ProjectId == projectId);

        // If worker (not admin), filter to only show their assigned tasks
        if (!isAdmin)
        {
            projectTasks = projectTasks.Where(t => t.AssignedToId == userId);
        }

        // Group by status for Kanban columns
        var groupedTasks = new Dictionary<Domain.Enums.TaskStatus, List<TaskBase>>();

        // Initialize all columns (even empty ones)
        foreach (Domain.Enums.TaskStatus status in Enum.GetValues(typeof(Domain.Enums.TaskStatus)))
        {
            groupedTasks[status] = new List<TaskBase>();
        }

        // Fill columns with tasks
        foreach (var task in projectTasks)
        {
            groupedTasks[task.Status].Add(task);
        }

        return groupedTasks;
    }

    public async Task MoveTaskAsync(Guid taskId, Domain.Enums.TaskStatus newStatus, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);

        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {taskId} not found");
        }

        // Update task status
        task.Status = newStatus;

        // Update timestamp if entity has UpdatedAt
        if (task is TaskBase taskBase)
        {
            // Task entities don't have UpdatedAt yet, but ready for future
        }

        await _taskRepository.UpdateAsync(task, cancellationToken);
    }

    public async Task<List<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default)
    {
        var allProjects = await _projectRepository.ListAsync(cancellationToken);
        return allProjects.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();
    }
}

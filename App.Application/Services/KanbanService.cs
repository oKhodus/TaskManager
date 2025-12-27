using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace App.Application.Services;

/// <summary>
/// Implementation of Kanban board service
/// Handles task organization and status transitions for visual workflow
/// </summary>
public class KanbanService : IKanbanService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<KanbanService> _logger;

    public KanbanService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        ILogger<KanbanService> logger)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<Dictionary<Domain.Enums.TaskStatus, List<TaskBase>>> GetKanbanBoardAsync(
        Guid projectId,
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Loading Kanban board for ProjectId: {ProjectId}, UserId: {UserId}, IsAdmin: {IsAdmin}",
            projectId, userId, isAdmin);

        // Get all tasks for the selected project
        var allTasks = await _taskRepository.ListAsync(cancellationToken);

        // Filter by project - show ALL tasks to all users (Admin and Worker)
        var projectTasks = allTasks.Where(t => t.ProjectId == projectId);

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

        var totalTasks = groupedTasks.Sum(g => g.Value.Count);
        _logger.LogInformation(
            "Kanban board loaded successfully. ProjectId: {ProjectId}, TotalTasks: {TotalTasks}",
            projectId, totalTasks);

        return groupedTasks;
    }

    public async Task MoveTaskAsync(Guid taskId, Domain.Enums.TaskStatus newStatus, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Attempting to move task. TaskId: {TaskId}, NewStatus: {NewStatus}",
            taskId, newStatus);

        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Task not found. TaskId: {TaskId}", taskId);
            throw new InvalidOperationException($"Task with ID {taskId} not found");
        }

        var oldStatus = task.Status;

        // Update task status
        task.Status = newStatus;

        // Update timestamp if entity has UpdatedAt
        if (task is TaskBase taskBase)
        {
            // Task entities don't have UpdatedAt yet, but ready for future
        }

        await _taskRepository.UpdateAsync(task, cancellationToken);

        _logger.LogInformation(
            "Task moved successfully. TaskId: {TaskId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            taskId, oldStatus, newStatus);
    }

    public async Task<List<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default)
    {
        var allProjects = await _projectRepository.ListAsync(cancellationToken);
        return allProjects.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();
    }
}

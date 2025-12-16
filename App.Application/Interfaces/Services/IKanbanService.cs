using App.Domain.Entities;
using App.Domain.Enums;

namespace App.Application.Interfaces.Services;

/// <summary>
/// Service for managing Kanban board operations
/// Provides task organization and status management for visual workflow
/// </summary>
public interface IKanbanService
{
    /// <summary>
    /// Get tasks for Kanban board filtered by project and user
    /// Workers see only their assigned tasks, Admins see all tasks
    /// </summary>
    /// <param name="projectId">Project to filter tasks by</param>
    /// <param name="userId">Current user ID (for filtering worker tasks)</param>
    /// <param name="isAdmin">Whether current user is admin</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of tasks grouped by status</returns>
    Task<Dictionary<Domain.Enums.TaskStatus, List<TaskBase>>> GetKanbanBoardAsync(
        Guid projectId,
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Move task to different status column (drag-and-drop)
    /// </summary>
    /// <param name="taskId">Task to move</param>
    /// <param name="newStatus">New status to assign</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MoveTaskAsync(Guid taskId, Domain.Enums.TaskStatus newStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active projects for filter dropdown
    /// </summary>
    Task<List<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default);
}

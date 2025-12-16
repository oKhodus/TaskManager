namespace App.Domain.Enums;

/// <summary>
/// Task status values matching the reference Kanban board
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task Set (Задача поставлена) - Initial state
    /// </summary>
    Todo = 0,

    /// <summary>
    /// Assigned (Отдано в работу) - Task assigned to worker
    /// </summary>
    Assigned = 1,

    /// <summary>
    /// In Progress (В обработке) - Task is being worked on
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Review (На проверку) - Task is under review
    /// </summary>
    Review = 3,

    /// <summary>
    /// Done (Готово) - Task completed
    /// </summary>
    Done = 4
}


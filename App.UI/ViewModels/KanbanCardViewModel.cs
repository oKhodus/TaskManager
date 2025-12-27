using System;
using App.Domain.Entities;
using App.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace App.UI.ViewModels;

/// <summary>
/// ViewModel for individual task card on Kanban board
/// Represents a single task with priority color coding
/// </summary>
public partial class KanbanCardViewModel : ViewModelBase
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private TaskPriority _priority;

    [ObservableProperty]
    private TaskStatus _status;

    [ObservableProperty]
    private string _assignedToName = string.Empty;

    [ObservableProperty]
    private Guid? _assignedToId;

    [ObservableProperty]
    private string _projectName = string.Empty;

    /// <summary>
    /// Priority color for visual coding
    /// </summary>
    public string PriorityColor => Priority switch
    {
        TaskPriority.Critical => "#f87171",  // Red
        TaskPriority.High => "#facc15",      // Yellow
        TaskPriority.Medium => "#4ade80",    // Green
        TaskPriority.Low => "#6b7280",       // Gray
        _ => "#6b7280"                       // Gray fallback
    };

    /// <summary>
    /// Priority display text
    /// </summary>
    public string PriorityText => Priority.ToString();

    public static KanbanCardViewModel FromTask(TaskBase task, string assignedToName, string projectName)
    {
        return new KanbanCardViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description ?? string.Empty,
            Priority = task.Priority,
            Status = task.Status,
            AssignedToName = assignedToName,
            AssignedToId = task.AssignedToId,
            ProjectName = projectName
        };
    }
}

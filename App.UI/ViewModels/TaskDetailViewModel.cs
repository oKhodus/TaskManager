using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskStatus = App.Domain.Enums.TaskStatus;

namespace App.UI.ViewModels;

public partial class TaskDetailViewModel : ViewModelBase
{
    private readonly ITaskService _taskService;

    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    [Required(ErrorMessage = "Title is required")]
    [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private TaskStatus _status;

    [ObservableProperty]
    private TaskPriority _priority;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime? _updatedAt;

    [ObservableProperty]
    private DateTime? _dueDate;

    [ObservableProperty]
    private Guid _projectId;

    [ObservableProperty]
    private Guid? _sprintId;

    [ObservableProperty]
    private Guid? _assignedToId;

    [ObservableProperty]
    private string? _assignedToName;

    [ObservableProperty]
    private string? _projectName;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _taskType = string.Empty;

    // Bug-specific fields
    [ObservableProperty]
    private string? _stepsToReproduce;

    [ObservableProperty]
    private string? _expectedBehavior;

    [ObservableProperty]
    private string? _actualBehavior;

    [ObservableProperty]
    private string? _environment;

    [ObservableProperty]
    private string? _severity;

    // Feature-specific fields
    [ObservableProperty]
    private string? _acceptanceCriteria;

    [ObservableProperty]
    private int? _storyPoints;

    [ObservableProperty]
    private string? _epic;

    public TaskDetailViewModel(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public void LoadTask(TaskBase task)
    {
        Id = task.Id;
        Title = task.Title;
        Description = task.Description;
        Status = task.Status;
        Priority = task.Priority;
        CreatedAt = task.CreatedAt;
        UpdatedAt = task.UpdatedAt;
        DueDate = task.DueDate;
        ProjectId = task.ProjectId;
        SprintId = task.SprintId;
        AssignedToId = task.AssignedToId;
        AssignedToName = task.AssignedTo?.Username;
        ProjectName = task.Project?.Name;
        IsEditMode = false;

        // Determine task type and load specific fields
        if (task is BugTask bugTask)
        {
            TaskType = "Bug";
            StepsToReproduce = bugTask.StepsToReproduce;
            ExpectedBehavior = bugTask.ExpectedBehavior;
            ActualBehavior = bugTask.ActualBehavior;
            Environment = bugTask.Environment;
            Severity = bugTask.Severity;
        }
        else if (task is FeatureTask featureTask)
        {
            TaskType = "Feature";
            AcceptanceCriteria = featureTask.AcceptanceCriteria;
            StoryPoints = featureTask.StoryPoints;
            Epic = featureTask.Epic;
        }
        else
        {
            TaskType = "Task";
        }
    }

    [RelayCommand]
    private void StartEdit()
    {
        IsEditMode = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        TaskBase task;

        // Create the appropriate task type
        if (TaskType == "Bug")
        {
            task = new BugTask
            {
                StepsToReproduce = StepsToReproduce,
                ExpectedBehavior = ExpectedBehavior,
                ActualBehavior = ActualBehavior,
                Environment = Environment,
                Severity = Severity ?? "Minor"
            };
        }
        else if (TaskType == "Feature")
        {
            task = new FeatureTask
            {
                AcceptanceCriteria = AcceptanceCriteria,
                StoryPoints = StoryPoints,
                Epic = Epic
            };
        }
        else
        {
            // Default to FeatureTask for unknown types
            task = new FeatureTask();
        }

        // Set common properties
        task.Id = Id;
        task.Title = Title;
        task.Description = Description;
        task.Status = Status;
        task.Priority = Priority;
        task.CreatedAt = CreatedAt;
        task.UpdatedAt = UpdatedAt;
        task.DueDate = DueDate;
        task.ProjectId = ProjectId;
        task.SprintId = SprintId;
        task.AssignedToId = AssignedToId;

        if (await _taskService.ValidateTaskAsync(task))
        {
            await _taskService.UpdateTaskAsync(task);
            IsEditMode = false;
        }
    }

    [RelayCommand]
    private async Task ChangeStatusAsync(TaskStatus newStatus)
    {
        await _taskService.ChangeTaskStatusAsync(Id, newStatus);
        Status = newStatus;
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace App.UI.ViewModels;

/// <summary>
/// ViewModel for viewing and editing task details
/// Supports updating task status, priority, assignee, and other fields
/// </summary>
public partial class TaskDetailWindowViewModel : ViewModelBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TaskDetailWindowViewModel> _logger;

    private Guid _taskId;
    private TaskBase? _originalTask;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private TaskPriority _selectedPriority = TaskPriority.Medium;

    [ObservableProperty]
    private Domain.Enums.TaskStatus _selectedStatus = Domain.Enums.TaskStatus.Todo;

    [ObservableProperty]
    private Project? _selectedProject;

    [ObservableProperty]
    private User? _selectedAssignee;

    [ObservableProperty]
    private User? _createdBy;

    [ObservableProperty]
    private DateTimeOffset? _dueDate;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime? _updatedAt;

    [ObservableProperty]
    private string _taskType = string.Empty;

    // Collections for dropdowns
    [ObservableProperty]
    private ObservableCollection<TaskPriority> _availablePriorities = new();

    [ObservableProperty]
    private ObservableCollection<Domain.Enums.TaskStatus> _availableStatuses = new();

    [ObservableProperty]
    private ObservableCollection<Project> _availableProjects = new();

    [ObservableProperty]
    private ObservableCollection<User> _availableUsers = new();

    // UI State
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _canEditStatus;

    [ObservableProperty]
    private bool _canEditAllFields;

    /// <summary>
    /// Show worker edit notice only when user can edit status but not all fields
    /// </summary>
    public bool ShowWorkerEditNotice => CanEditStatus && !CanEditAllFields;

    public event EventHandler? TaskUpdated;
    public event EventHandler? CloseRequested;

    public TaskDetailWindowViewModel(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        ILogger<TaskDetailWindowViewModel> logger)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _logger = logger;

        IsAdmin = _currentUserService.IsAdmin;

        InitializeLists();
        _logger.LogDebug("TaskDetailWindowViewModel initialized");
    }

    private void InitializeLists()
    {
        // Initialize priorities
        foreach (TaskPriority priority in Enum.GetValues(typeof(TaskPriority)))
        {
            AvailablePriorities.Add(priority);
        }

        // Initialize statuses
        foreach (Domain.Enums.TaskStatus status in Enum.GetValues(typeof(Domain.Enums.TaskStatus)))
        {
            AvailableStatuses.Add(status);
        }

        _logger.LogDebug("Initialized priority and status lists. Priorities: {PriorityCount}, Statuses: {StatusCount}",
            AvailablePriorities.Count, AvailableStatuses.Count);
    }

    public async Task LoadTaskAsync(Guid taskId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            _taskId = taskId;

            _logger.LogInformation("Loading task details. TaskId: {TaskId}", taskId);

            // Load task
            _originalTask = await _taskRepository.GetByIdAsync(taskId);

            if (_originalTask == null)
            {
                ErrorMessage = "Task not found";
                _logger.LogWarning("Task not found. TaskId: {TaskId}", taskId);
                return;
            }

            _logger.LogDebug("Task loaded. TaskId: {TaskId}, Title: {Title}, Status: {Status}, Priority: {Priority}",
                _originalTask.Id, _originalTask.Title, _originalTask.Status, _originalTask.Priority);

            // Load related data
            await LoadProjectsAndUsersAsync();

            // Populate fields
            Title = _originalTask.Title;
            Description = _originalTask.Description ?? string.Empty;
            SelectedPriority = _originalTask.Priority;
            SelectedStatus = _originalTask.Status;
            DueDate = _originalTask.DueDate.HasValue ? new DateTimeOffset(_originalTask.DueDate.Value) : null;
            CreatedAt = _originalTask.CreatedAt;
            UpdatedAt = _originalTask.UpdatedAt;

            // Set task type display
            TaskType = _originalTask switch
            {
                BugTask => "Bug",
                FeatureTask => "Feature",
                _ => "Task"
            };

            // Find and set project
            SelectedProject = AvailableProjects.FirstOrDefault(p => p.Id == _originalTask.ProjectId);

            // Find and set assignee
            if (_originalTask.AssignedToId.HasValue)
            {
                SelectedAssignee = AvailableUsers.FirstOrDefault(u => u.Id == _originalTask.AssignedToId.Value);
            }

            // Find and set creator
            CreatedBy = AvailableUsers.FirstOrDefault(u => u.Id == _originalTask.CreatedById);

            // Determine edit permissions:
            // - CanEditAllFields: Only Admin
            // - CanEditStatus: Admin OR assigned Worker
            var isAssignedToCurrentUser = _originalTask.AssignedToId.HasValue &&
                                         _originalTask.AssignedToId.Value == _currentUserService.UserId;
            CanEditAllFields = IsAdmin;
            CanEditStatus = IsAdmin || isAssignedToCurrentUser;

            // Notify dependent property
            OnPropertyChanged(nameof(ShowWorkerEditNotice));

            _logger.LogInformation("Task details loaded successfully. TaskId: {TaskId}, Project: {ProjectName}, AssignedTo: {AssigneeName}, CanEditAllFields: {CanEditAllFields}, CanEditStatus: {CanEditStatus}",
                taskId, SelectedProject?.Name ?? "N/A", SelectedAssignee != null ? $"{SelectedAssignee.FirstName} {SelectedAssignee.LastName}" : "Unassigned", CanEditAllFields, CanEditStatus);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load task: {ex.Message}";
            _logger.LogError(ex, "Failed to load task details. TaskId: {TaskId}", taskId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadProjectsAndUsersAsync()
    {
        _logger.LogDebug("Loading projects and users for dropdowns");

        // Load projects
        var projects = await _projectRepository.ListAsync();
        AvailableProjects = new ObservableCollection<Project>(
            projects.Where(p => p.IsActive).OrderBy(p => p.Name));

        // Load users
        var users = await _userRepository.ListAsync();
        AvailableUsers = new ObservableCollection<User>(
            users.Where(u => u.IsActive).OrderBy(u => u.FirstName));

        _logger.LogDebug("Loaded projects and users. Projects: {ProjectCount}, Users: {UserCount}",
            AvailableProjects.Count, AvailableUsers.Count);
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        try
        {
            ErrorMessage = null;
            SuccessMessage = null;

            // Validation
            if (!CanEditStatus)
            {
                ErrorMessage = "You do not have permission to edit this task";
                _logger.LogWarning("User without permission attempted to edit task. TaskId: {TaskId}, UserId: {UserId}, IsAdmin: {IsAdmin}",
                    _taskId, _currentUserService.UserId, IsAdmin);
                return;
            }

            if (_originalTask == null)
            {
                ErrorMessage = "Task data not loaded";
                _logger.LogError("Attempted to save changes without loaded task. TaskId: {TaskId}", _taskId);
                return;
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Title is required";
                _logger.LogWarning("Task save attempt with empty title. TaskId: {TaskId}", _taskId);
                return;
            }

            if (SelectedProject == null)
            {
                ErrorMessage = "Please select a project";
                _logger.LogWarning("Task save attempt without project. TaskId: {TaskId}", _taskId);
                return;
            }

            IsSaving = true;

            _logger.LogInformation("Saving task changes. TaskId: {TaskId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}, CanEditAllFields: {CanEditAllFields}",
                _taskId, _originalTask.Status, SelectedStatus, CanEditAllFields);

            // Update task fields based on permissions
            // Status can be updated by Admin OR assigned Worker
            _originalTask.Status = SelectedStatus;

            // Only Admin can update other fields
            if (CanEditAllFields)
            {
                _originalTask.Title = Title;
                _originalTask.Description = Description;
                _originalTask.Priority = SelectedPriority;
                _originalTask.ProjectId = SelectedProject.Id;
                _originalTask.AssignedToId = SelectedAssignee?.Id;
                _originalTask.DueDate = DueDate.HasValue ? DueDate.Value.DateTime : null;

                _logger.LogDebug("Admin editing all fields. TaskId: {TaskId}, Title: {Title}, Priority: {Priority}",
                    _taskId, Title, SelectedPriority);
            }
            else
            {
                _logger.LogDebug("Worker editing only Status. TaskId: {TaskId}, NewStatus: {NewStatus}",
                    _taskId, SelectedStatus);
            }

            // Update timestamp
            var now = DateTime.UtcNow;
            _originalTask.UpdatedAt = now;
            UpdatedAt = now;

            // Save to database
            await _taskRepository.UpdateAsync(_originalTask);

            if (CanEditAllFields)
            {
                _logger.LogInformation("Task updated successfully (Admin). TaskId: {TaskId}, Title: {Title}, Status: {Status}, Priority: {Priority}, AssignedTo: {AssigneeId}",
                    _taskId, Title, SelectedStatus, SelectedPriority, SelectedAssignee?.Id);
            }
            else
            {
                _logger.LogInformation("Task status updated successfully (Worker). TaskId: {TaskId}, NewStatus: {Status}",
                    _taskId, SelectedStatus);
            }

            SuccessMessage = "Task updated successfully!";

            // Notify listeners
            TaskUpdated?.Invoke(this, EventArgs.Empty);

            // Close window after delay
            await Task.Delay(1000);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save changes: {ex.Message}";
            _logger.LogError(ex, "Failed to save task changes. TaskId: {TaskId}", _taskId);
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        _logger.LogDebug("Task detail window closed without saving. TaskId: {TaskId}", _taskId);
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}

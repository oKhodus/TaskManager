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

namespace App.UI.ViewModels;

/// <summary>
/// ViewModel for creating new tasks
/// Supports pre-selection of project if coming from Kanban board
/// </summary>
public partial class CreateTaskViewModel : ViewModelBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Project> _availableProjects = new();

    [ObservableProperty]
    private Project? _selectedProject;

    [ObservableProperty]
    private ObservableCollection<User> _availableUsers = new();

    [ObservableProperty]
    private User? _selectedAssignee;

    [ObservableProperty]
    private ObservableCollection<string> _taskTypes = new() { "Feature", "Bug" };

    [ObservableProperty]
    private string _selectedTaskType = "Feature";

    [ObservableProperty]
    private ObservableCollection<TaskPriority> _availablePriorities = new();

    [ObservableProperty]
    private TaskPriority _selectedPriority = TaskPriority.Medium;

    [ObservableProperty]
    private ObservableCollection<Domain.Enums.TaskStatus> _availableStatuses = new();

    [ObservableProperty]
    private Domain.Enums.TaskStatus _selectedStatus = Domain.Enums.TaskStatus.Todo;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public event EventHandler? TaskCreated;

    public CreateTaskViewModel(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;

        InitializeLists();
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
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Load projects
            var projects = await _projectRepository.ListAsync();
            AvailableProjects = new ObservableCollection<Project>(projects.Where(p => p.IsActive).OrderBy(p => p.Name));

            // Load users for assignment
            var users = await _userRepository.ListAsync();
            AvailableUsers = new ObservableCollection<User>(users.Where(u => u.IsActive).OrderBy(u => u.FirstName));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Set pre-selected project (called from Kanban board)
    /// </summary>
    public void SetProject(Project project)
    {
        SelectedProject = project;
    }

    [RelayCommand]
    private async Task CreateTaskAsync()
    {
        try
        {
            ErrorMessage = null;
            SuccessMessage = null;

            // Validation
            if (!_currentUserService.IsAdmin)
            {
                ErrorMessage = "Only administrators can create tasks";
                return;
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Title is required";
                return;
            }

            if (SelectedProject == null)
            {
                ErrorMessage = "Please select a project";
                return;
            }

            if (_currentUserService.CurrentUser == null)
            {
                ErrorMessage = "User not authenticated";
                return;
            }

            IsLoading = true;

            // Create task based on type
            TaskBase newTask = SelectedTaskType == "Bug"
                ? new BugTask
                {
                    Id = Guid.NewGuid(),
                    Title = Title,
                    Description = Description,
                    Status = SelectedStatus,
                    Priority = SelectedPriority,
                    ProjectId = SelectedProject.Id,
                    CreatedById = _currentUserService.UserId!.Value,
                    AssignedToId = SelectedAssignee?.Id,
                    CreatedAt = DateTime.UtcNow,
                    Severity = "Medium" // Default severity
                }
                : new FeatureTask
                {
                    Id = Guid.NewGuid(),
                    Title = Title,
                    Description = Description,
                    Status = SelectedStatus,
                    Priority = SelectedPriority,
                    ProjectId = SelectedProject.Id,
                    CreatedById = _currentUserService.UserId!.Value,
                    AssignedToId = SelectedAssignee?.Id,
                    CreatedAt = DateTime.UtcNow,
                    StoryPoints = 0 // Default story points
                };

            await _taskRepository.AddAsync(newTask);

            SuccessMessage = $"Task '{Title}' created successfully!";

            // Notify listeners
            TaskCreated?.Invoke(this, EventArgs.Empty);

            // Clear form after short delay
            await Task.Delay(1000);
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create task: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        Title = string.Empty;
        Description = string.Empty;
        SelectedProject = null;
        SelectedAssignee = null;
        SelectedTaskType = "Feature";
        SelectedPriority = TaskPriority.Medium;
        SelectedStatus = Domain.Enums.TaskStatus.Todo;
        ErrorMessage = null;
        SuccessMessage = null;
    }
}

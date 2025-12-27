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
/// Main ViewModel for Kanban board
/// Coordinates columns, handles drag-and-drop, and manages filtering
/// </summary>
public partial class KanbanBoardViewModel : ViewModelBase
{
    private readonly IKanbanService _kanbanService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<KanbanBoardViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<KanbanColumnViewModel> _columns = new();

    [ObservableProperty]
    private ObservableCollection<Project> _availableProjects = new();

    [ObservableProperty]
    private Project? _selectedProject;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Check if current user is admin
    /// </summary>
    public bool IsAdmin => _currentUserService.IsAdmin;

    public event EventHandler? CreateTaskRequested;
    public event EventHandler<Guid>? TaskDetailRequested;

    public KanbanBoardViewModel(
        IKanbanService kanbanService,
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IProjectRepository projectRepository,
        ILogger<KanbanBoardViewModel> logger)
    {
        _kanbanService = kanbanService;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _logger = logger;

        // Initialize columns for all statuses
        InitializeColumns();
        _logger.LogDebug("KanbanBoardViewModel initialized with {ColumnCount} columns", Columns.Count);
    }

    private void InitializeColumns()
    {
        Columns.Clear();
        Columns.Add(new KanbanColumnViewModel(Domain.Enums.TaskStatus.Todo));
        Columns.Add(new KanbanColumnViewModel(Domain.Enums.TaskStatus.Assigned));
        Columns.Add(new KanbanColumnViewModel(Domain.Enums.TaskStatus.InProgress));
        Columns.Add(new KanbanColumnViewModel(Domain.Enums.TaskStatus.Review));
        Columns.Add(new KanbanColumnViewModel(Domain.Enums.TaskStatus.Done));
    }

    [RelayCommand]
    private async Task LoadProjectsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            _logger.LogDebug("Loading active projects");
            var projects = await _kanbanService.GetActiveProjectsAsync();
            AvailableProjects = new ObservableCollection<Project>(projects);

            _logger.LogInformation("Loaded {ProjectCount} active projects", projects.Count());

            // Notify UI about HasNoProjects change
            OnPropertyChanged(nameof(HasNoProjects));

            // Don't auto-select - let user choose
            // Clear selection if previously selected project no longer exists
            if (SelectedProject != null && !AvailableProjects.Any(p => p.Id == SelectedProject.Id))
            {
                _logger.LogDebug("Previously selected project no longer exists. Clearing selection");
                SelectedProject = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load projects: {ex.Message}";
            _logger.LogError(ex, "Failed to load projects");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Check if there are no projects available
    /// </summary>
    public bool HasNoProjects => !AvailableProjects.Any();

    /// <summary>
    /// Message to display when no projects exist
    /// </summary>
    public string NoProjectsMessage => "No active projects available. Please create a project first.";

    [RelayCommand]
    private async Task LoadBoardAsync()
    {
        if (SelectedProject == null || _currentUserService.CurrentUser == null)
        {
            _logger.LogDebug("LoadBoardAsync skipped - no selected project or current user");
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var userId = _currentUserService.UserId!.Value;
            var isAdmin = _currentUserService.IsAdmin;

            _logger.LogDebug("Loading Kanban board for ProjectId: {ProjectId}, UserId: {UserId}, IsAdmin: {IsAdmin}",
                SelectedProject.Id, userId, isAdmin);

            // Get tasks grouped by status
            var taskGroups = await _kanbanService.GetKanbanBoardAsync(
                SelectedProject.Id,
                userId,
                isAdmin);

            // Load user and project data for display
            var users = await _userRepository.ListAsync();
            var userDict = users.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");

            // Clear existing cards
            foreach (var column in Columns)
            {
                column.ClearCards();
            }

            var totalTasksLoaded = 0;

            // Populate columns with cards
            foreach (var (status, tasks) in taskGroups)
            {
                var column = Columns.FirstOrDefault(c => c.Status == status);
                if (column == null) continue;

                foreach (var task in tasks.OrderBy(t => t.CreatedAt))
                {
                    var assignedToName = task.AssignedToId.HasValue && userDict.ContainsKey(task.AssignedToId.Value)
                        ? userDict[task.AssignedToId.Value]
                        : "Unassigned";

                    var card = KanbanCardViewModel.FromTask(task, assignedToName, SelectedProject.Name);
                    column.AddCard(card);
                    totalTasksLoaded++;
                }
            }

            _logger.LogInformation("Kanban board loaded successfully. ProjectId: {ProjectId}, Total Tasks: {TaskCount}",
                SelectedProject.Id, totalTasksLoaded);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load board: {ex.Message}";
            _logger.LogError(ex, "Failed to load Kanban board for ProjectId: {ProjectId}", SelectedProject?.Id);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Handle card drop - move task to new status
    /// </summary>
    public async Task OnCardDroppedAsync(KanbanCardViewModel card, Domain.Enums.TaskStatus newStatus)
    {
        if (card.Status == newStatus)
        {
            _logger.LogDebug("Card not moved - new status same as current. TaskId: {TaskId}", card.Id);
            return; // No change
        }

        // Permission check: Workers can only move tasks assigned to them
        var isAdmin = _currentUserService.IsAdmin;
        var currentUserId = _currentUserService.UserId;
        var isAssignedToCurrentUser = card.AssignedToId.HasValue && card.AssignedToId.Value == currentUserId;

        if (!isAdmin && !isAssignedToCurrentUser)
        {
            ErrorMessage = "You can only move tasks assigned to you";
            _logger.LogWarning("Worker attempted to move task not assigned to them. TaskId: {TaskId}, WorkerId: {WorkerId}, AssignedToId: {AssignedToId}",
                card.Id, currentUserId, card.AssignedToId);
            return;
        }

        try
        {
            _logger.LogInformation("Moving task. TaskId: {TaskId}, From: {OldStatus}, To: {NewStatus}, UserId: {UserId}, IsAdmin: {IsAdmin}",
                card.Id, card.Status, newStatus, currentUserId, isAdmin);

            // Update in database
            await _kanbanService.MoveTaskAsync(card.Id, newStatus);

            // Update UI
            var oldColumn = Columns.FirstOrDefault(c => c.Status == card.Status);
            var newColumn = Columns.FirstOrDefault(c => c.Status == newStatus);

            if (oldColumn != null && newColumn != null)
            {
                oldColumn.RemoveCard(card);
                card.Status = newStatus;
                newColumn.AddCard(card);

                _logger.LogInformation("Task moved successfully. TaskId: {TaskId}, NewStatus: {NewStatus}",
                    card.Id, newStatus);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to move task: {ex.Message}";
            _logger.LogError(ex, "Failed to move task. TaskId: {TaskId}, NewStatus: {NewStatus}",
                card.Id, newStatus);
        }
    }

    [RelayCommand]
    private void OpenCreateTaskDialog()
    {
        _logger.LogDebug("Create task dialog requested");
        CreateTaskRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Open task detail window for viewing/editing
    /// </summary>
    [RelayCommand]
    private void OpenTaskDetail(Guid taskId)
    {
        _logger.LogInformation("Task detail window requested. TaskId: {TaskId}", taskId);
        TaskDetailRequested?.Invoke(this, taskId);
    }

    /// <summary>
    /// Refresh board after task creation or update
    /// </summary>
    public async Task RefreshBoardAsync()
    {
        _logger.LogDebug("Board refresh requested");
        await LoadBoardAsync();
    }

    /// <summary>
    /// Clear error message
    /// </summary>
    [RelayCommand]
    private void ClearError()
    {
        ErrorMessage = null;
        _logger.LogDebug("Error message cleared by user");
    }

    partial void OnSelectedProjectChanged(Project? value)
    {
        if (value != null)
        {
            _logger.LogInformation("Project selected. ProjectId: {ProjectId}, ProjectName: {ProjectName}",
                value.Id, value.Name);
            _ = LoadBoardAsync();
        }
        else
        {
            _logger.LogDebug("Project selection cleared");
        }
    }
}

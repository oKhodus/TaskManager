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
/// Main ViewModel for Kanban board
/// Coordinates columns, handles drag-and-drop, and manages filtering
/// </summary>
public partial class KanbanBoardViewModel : ViewModelBase
{
    private readonly IKanbanService _kanbanService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;

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

    public KanbanBoardViewModel(
        IKanbanService kanbanService,
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IProjectRepository projectRepository)
    {
        _kanbanService = kanbanService;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _projectRepository = projectRepository;

        // Initialize columns for all statuses
        InitializeColumns();
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

            var projects = await _kanbanService.GetActiveProjectsAsync();
            AvailableProjects = new ObservableCollection<Project>(projects);

            // Notify UI about HasNoProjects change
            OnPropertyChanged(nameof(HasNoProjects));

            // Don't auto-select - let user choose
            // Clear selection if previously selected project no longer exists
            if (SelectedProject != null && !AvailableProjects.Any(p => p.Id == SelectedProject.Id))
            {
                SelectedProject = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load projects: {ex.Message}";
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
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var userId = _currentUserService.UserId!.Value;
            var isAdmin = _currentUserService.IsAdmin;

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
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load board: {ex.Message}";
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
            return; // No change
        }

        try
        {
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
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to move task: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenCreateTaskDialog()
    {
        CreateTaskRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Refresh board after task creation or update
    /// </summary>
    public async Task RefreshBoardAsync()
    {
        await LoadBoardAsync();
    }

    partial void OnSelectedProjectChanged(Project? value)
    {
        if (value != null)
        {
            _ = LoadBoardAsync();
        }
    }
}

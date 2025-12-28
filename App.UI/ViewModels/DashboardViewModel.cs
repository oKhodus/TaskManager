using System;
using App.Application.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.UI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private ProjectMasterViewModel? _projectMasterViewModel;

    [ObservableProperty]
    private KanbanBoardViewModel? _kanbanBoardViewModel;

    [ObservableProperty]
    private int _selectedTabIndex;

    public string CurrentUsername => _currentUserService.Username ?? "Guest";
    public string CurrentUserRole => _currentUserService.Role?.ToString() ?? "Unknown";
    public bool IsAdmin => _currentUserService.IsAdmin;

    public event EventHandler? LogoutRequested;

    public DashboardViewModel(
        ICurrentUserService currentUserService,
        ProjectMasterViewModel projectMasterViewModel,
        KanbanBoardViewModel kanbanBoardViewModel)
    {
        _currentUserService = currentUserService;
        _projectMasterViewModel = projectMasterViewModel;
        _kanbanBoardViewModel = kanbanBoardViewModel;

        // Admin sees Projects tab (index 0)
        // Worker sees Kanban tab (index 1, because Projects tab is hidden but still occupies index 0)
        _selectedTabIndex = _currentUserService.IsAdmin ? 0 : 1;
    }

    [RelayCommand]
    private void Logout()
    {
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}

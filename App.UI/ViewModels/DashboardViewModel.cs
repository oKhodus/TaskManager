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
    private TaskMasterViewModel? _taskMasterViewModel;

    [ObservableProperty]
    private int _selectedTabIndex;

    public string CurrentUsername => _currentUserService.Username ?? "Guest";
    public string CurrentUserRole => _currentUserService.Role?.ToString() ?? "Unknown";

    public event EventHandler? LogoutRequested;

    public DashboardViewModel(
        ICurrentUserService currentUserService,
        ProjectMasterViewModel projectMasterViewModel,
        TaskMasterViewModel taskMasterViewModel)
    {
        _currentUserService = currentUserService;
        _projectMasterViewModel = projectMasterViewModel;
        _taskMasterViewModel = taskMasterViewModel;
        _selectedTabIndex = 0;
    }

    [RelayCommand]
    private void Logout()
    {
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}

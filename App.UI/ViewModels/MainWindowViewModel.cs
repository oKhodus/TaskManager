using App.Application.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace App.UI.ViewModels;

/// <summary>
/// Main window ViewModel - manages navigation between login and dashboard
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private ViewModelBase? _currentView;

    public LoginViewModel LoginViewModel { get; }
    public DashboardViewModel DashboardViewModel { get; }

    public MainWindowViewModel(
        ICurrentUserService currentUserService,
        LoginViewModel loginViewModel,
        DashboardViewModel dashboardViewModel)
    {
        _currentUserService = currentUserService;
        LoginViewModel = loginViewModel;
        DashboardViewModel = dashboardViewModel;

        // Start with login view
        CurrentView = LoginViewModel;

        // Subscribe to login success
        LoginViewModel.LoginSuccessful += (s, e) => OnLoginSuccess();

        // Subscribe to logout request
        DashboardViewModel.LogoutRequested += (s, e) => OnLogout();
    }

    public void OnLoginSuccess()
    {
        CurrentView = DashboardViewModel;
    }

    public void OnLogout()
    {
        _currentUserService.ClearCurrentUser();
        CurrentView = LoginViewModel;
    }
}

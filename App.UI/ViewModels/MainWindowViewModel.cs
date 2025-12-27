using System;
using App.Application.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace App.UI.ViewModels;

/// <summary>
/// Main window ViewModel - manages navigation between login and dashboard
/// Recreates DashboardViewModel on each login for clean state (SOLID compliance)
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IServiceProvider _serviceProvider;
    private DashboardViewModel? _dashboardViewModel;

    [ObservableProperty]
    private ViewModelBase? _currentView;

    public LoginViewModel LoginViewModel { get; }

    public MainWindowViewModel(
        ICurrentUserService currentUserService,
        IServiceProvider serviceProvider,
        LoginViewModel loginViewModel)
    {
        _currentUserService = currentUserService;
        _serviceProvider = serviceProvider;
        LoginViewModel = loginViewModel;

        // Start with login view
        CurrentView = LoginViewModel;

        // Subscribe to login success
        LoginViewModel.LoginSuccessful += (s, e) => OnLoginSuccess();
    }

    public void OnLoginSuccess()
    {
        // Unsubscribe from previous dashboard if exists
        if (_dashboardViewModel != null)
        {
            _dashboardViewModel.LogoutRequested -= OnLogout;
        }

        // Create new DashboardViewModel with fresh state
        _dashboardViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();

        // Subscribe to logout request
        _dashboardViewModel.LogoutRequested += OnLogout;

        // Switch to dashboard view
        CurrentView = _dashboardViewModel;
    }

    private void OnLogout(object? sender, EventArgs e)
    {
        _currentUserService.ClearCurrentUser();
        CurrentView = LoginViewModel;
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace App.UI.ViewModels;

/// <summary>
/// ViewModel for login screen
/// </summary>
public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LoginViewModel> _logger;

    [ObservableProperty]
    [Required(ErrorMessage = "Username is required")]
    private string _username = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Password is required")]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    public event EventHandler? LoginSuccessful;

    public LoginViewModel(
        IAuthenticationService authenticationService,
        ICurrentUserService currentUserService,
        ILogger<LoginViewModel> logger)
    {
        _authenticationService = authenticationService;
        _currentUserService = currentUserService;
        _logger = logger;

        _logger.LogDebug("LoginViewModel initialized");
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        IsLoading = true;

        try
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both username and password";
                _logger.LogWarning("Login attempt with missing credentials");
                return;
            }

            _logger.LogInformation("Login attempt for username: {Username}", Username);

            var user = await _authenticationService.AuthenticateAsync(Username, Password);

            if (user == null)
            {
                ErrorMessage = "Invalid username or password";
                _logger.LogWarning("Failed login attempt for username: {Username} - Invalid credentials", Username);
                return;
            }

            // Set current user
            _currentUserService.SetCurrentUser(user);

            _logger.LogInformation("User successfully logged in. UserId: {UserId}, Username: {Username}", user.Id, user.Username);

            // Raise login successful event
            LoginSuccessful?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
            _logger.LogError(ex, "Login failed with exception for username: {Username}", Username);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

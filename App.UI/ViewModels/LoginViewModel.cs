using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.UI.ViewModels;

/// <summary>
/// ViewModel for login screen
/// </summary>
public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICurrentUserService _currentUserService;

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
        ICurrentUserService currentUserService)
    {
        _authenticationService = authenticationService;
        _currentUserService = currentUserService;
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
                return;
            }

            var user = await _authenticationService.AuthenticateAsync(Username, Password);

            if (user == null)
            {
                ErrorMessage = "Invalid username or password";
                return;
            }

            // Set current user
            _currentUserService.SetCurrentUser(user);

            // Raise login successful event
            LoginSuccessful?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

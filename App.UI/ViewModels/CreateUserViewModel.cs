using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;
using App.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace App.UI.ViewModels;

/// <summary>
/// ViewModel for creating new users (admin only)
/// Validation performed manually in CreateUserAsync for better UX
/// </summary>
public partial class CreateUserViewModel : ViewModelBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateUserViewModel> _logger;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private UserRole _selectedRole = UserRole.Worker;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private string? _usernameError;

    [ObservableProperty]
    private string? _passwordError;

    [ObservableProperty]
    private string? _emailError;

    [ObservableProperty]
    private string? _firstNameError;

    [ObservableProperty]
    private string? _lastNameError;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private bool _isLoading;

    partial void OnUsernameChanged(string value)
    {
        UsernameError = null;
    }

    partial void OnPasswordChanged(string value)
    {
        PasswordError = null;
    }

    partial void OnEmailChanged(string value)
    {
        EmailError = null;
    }

    partial void OnFirstNameChanged(string value)
    {
        FirstNameError = null;
    }

    partial void OnLastNameChanged(string value)
    {
        LastNameError = null;
    }

    public ObservableCollection<UserRole> AvailableRoles { get; } = new()
    {
        UserRole.Worker,
        UserRole.Admin
    };

    public event EventHandler? UserCreated;

    public CreateUserViewModel(
        IUserManagementService userManagementService,
        ICurrentUserService currentUserService,
        ILogger<CreateUserViewModel> logger)
    {
        _userManagementService = userManagementService;
        _currentUserService = currentUserService;
        _logger = logger;

        _logger.LogDebug("CreateUserViewModel initialized");
    }

    [RelayCommand]
    private async Task CreateUserAsync()
    {
        // Clear all errors
        UsernameError = null;
        PasswordError = null;
        EmailError = null;
        FirstNameError = null;
        LastNameError = null;
        SuccessMessage = null;
        IsLoading = true;

        try
        {
            // Validate admin access
            if (!_currentUserService.IsAdmin)
            {
                UsernameError = "Only administrators can create users";
                _logger.LogWarning("Non-admin user attempted to create user");
                return;
            }

            // Validate inputs
            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameError = "Username is required";
                _logger.LogWarning("User creation attempt with missing username");
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Password is required";
                _logger.LogWarning("User creation attempt with missing password");
            }
            else if (Password.Length < 6)
            {
                PasswordError = "Password must be at least 6 characters";
                _logger.LogWarning("User creation attempt with invalid password length for username: {Username}", Username);
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email is required";
                _logger.LogWarning("User creation attempt with missing email");
            }

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                FirstNameError = "First name is required";
                _logger.LogWarning("User creation attempt with missing first name");
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                LastNameError = "Last name is required";
                _logger.LogWarning("User creation attempt with missing last name");
            }

            // If validation failed, return early
            if (UsernameError != null || PasswordError != null || EmailError != null ||
                FirstNameError != null || LastNameError != null)
            {
                return;
            }

            _logger.LogInformation("Creating user. Username: {Username}, Email: {Email}, Role: {Role}, IsActive: {IsActive}",
                Username, Email, SelectedRole, IsActive);

            // Create user
            var user = await _userManagementService.CreateUserAsync(
                Username,
                Password,
                Email,
                FirstName,
                LastName,
                SelectedRole
            );

            // Update active status if needed
            if (!IsActive && user.IsActive)
            {
                await _userManagementService.DeactivateUserAsync(user.Id);
                _logger.LogInformation("User deactivated after creation. UserId: {UserId}, Username: {Username}",
                    user.Id, Username);
            }

            _logger.LogInformation("User created successfully. UserId: {UserId}, Username: {Username}, Role: {Role}",
                user.Id, Username, SelectedRole);

            SuccessMessage = $"User '{Username}' created successfully!";

            // Raise event
            UserCreated?.Invoke(this, EventArgs.Empty);

            // Clear form after short delay
            await Task.Delay(1500);
            ClearForm();
        }
        catch (InvalidOperationException ex)
        {
            UsernameError = ex.Message;
            _logger.LogWarning(ex, "Invalid operation during user creation for username: {Username}", Username);
        }
        catch (Exception ex)
        {
            UsernameError = $"Failed to create user: {ex.Message}";
            _logger.LogError(ex, "Failed to create user. Username: {Username}, Email: {Email}",
                Username, Email);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        Username = string.Empty;
        Password = string.Empty;
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        SelectedRole = UserRole.Worker;
        IsActive = true;
        UsernameError = null;
        PasswordError = null;
        EmailError = null;
        FirstNameError = null;
        LastNameError = null;
        SuccessMessage = null;
        _logger.LogDebug("User creation form cleared");
    }
}

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using App.Application.Interfaces.Services;
using App.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.UI.ViewModels;

/// <summary>
/// ViewModel for creating new users (admin only)
/// </summary>
public partial class CreateUserViewModel : ViewModelBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    private string _username = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    private string _password = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    private string _email = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "First name is required")]
    private string _firstName = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Last name is required")]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private UserRole _selectedRole = UserRole.Worker;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<UserRole> AvailableRoles { get; } = new()
    {
        UserRole.Worker,
        UserRole.Admin
    };

    public event EventHandler? UserCreated;

    public CreateUserViewModel(
        IUserManagementService userManagementService,
        ICurrentUserService currentUserService)
    {
        _userManagementService = userManagementService;
        _currentUserService = currentUserService;
    }

    [RelayCommand]
    private async Task CreateUserAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsLoading = true;

        try
        {
            // Validate admin access
            if (!_currentUserService.IsAdmin)
            {
                ErrorMessage = "Only administrators can create users";
                return;
            }

            // Validate inputs
            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Username is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password is required";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Password must be at least 6 characters";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                ErrorMessage = "First name is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                ErrorMessage = "Last name is required";
                return;
            }

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
            }

            SuccessMessage = $"User '{Username}' created successfully!";

            // Raise event
            UserCreated?.Invoke(this, EventArgs.Empty);

            // Clear form after short delay
            await Task.Delay(1500);
            ClearForm();
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create user: {ex.Message}";
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
        ErrorMessage = null;
        SuccessMessage = null;
    }
}

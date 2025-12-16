using App.Application.Interfaces.Services;
using App.Domain.Entities;
using App.Domain.Enums;

namespace App.Application.Services;

/// <summary>
/// Current user service implementation
/// Thread-safe singleton service for managing authenticated user context
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private User? _currentUser;
    private readonly object _lock = new object();

    public User? CurrentUser
    {
        get
        {
            lock (_lock)
            {
                return _currentUser;
            }
        }
    }

    public Guid? UserId => CurrentUser?.Id;

    public string? Username => CurrentUser?.Username;

    public UserRole? Role => CurrentUser?.Role;

    public bool IsAuthenticated => CurrentUser != null;

    public bool IsAdmin => Role == UserRole.Admin;

    public void SetCurrentUser(User user)
    {
        lock (_lock)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
        }
    }

    public void ClearCurrentUser()
    {
        lock (_lock)
        {
            _currentUser = null;
        }
    }

    public bool HasPermission(UserRole requiredRole)
    {
        if (!IsAuthenticated)
            return false;

        // Admin has all permissions
        if (IsAdmin)
            return true;

        // Check if user's role meets the requirement
        return Role >= requiredRole;
    }
}

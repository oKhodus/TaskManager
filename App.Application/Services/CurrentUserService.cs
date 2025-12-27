using App.Application.Interfaces.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace App.Application.Services;

/// <summary>
/// Current user service implementation
/// Thread-safe singleton service for managing authenticated user context
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private User? _currentUser;
    private readonly object _lock = new object();
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(ILogger<CurrentUserService> logger)
    {
        _logger = logger;
    }

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
            _logger.LogInformation("Current user set: UserId={UserId}, Username={Username}, Role={Role}",
                user.Id, user.Username, user.Role);
        }
    }

    public void ClearCurrentUser()
    {
        lock (_lock)
        {
            var previousUserId = _currentUser?.Id;
            var previousUsername = _currentUser?.Username;
            _currentUser = null;
            _logger.LogInformation("Current user cleared. Previous user: UserId={UserId}, Username={Username}",
                previousUserId, previousUsername);
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

using App.Domain.Entities;
using App.Domain.Enums;

namespace App.Application.Interfaces.Services;

/// <summary>
/// Service for managing current authenticated user context
/// Designed to be extended with department and other attributes in the future
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Current authenticated user
    /// </summary>
    User? CurrentUser { get; }

    /// <summary>
    /// User ID of current user
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Username of current user
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Role of current user
    /// </summary>
    UserRole? Role { get; }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Check if current user has admin role
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// Set the current user (after login)
    /// </summary>
    /// <param name="user">User to set as current</param>
    void SetCurrentUser(User user);

    /// <summary>
    /// Clear current user (logout)
    /// </summary>
    void ClearCurrentUser();

    /// <summary>
    /// Check if current user has permission to perform an action
    /// Extensible for future department-based permissions
    /// </summary>
    /// <param name="requiredRole">Minimum required role</param>
    /// <returns>True if user has permission</returns>
    bool HasPermission(UserRole requiredRole);
}

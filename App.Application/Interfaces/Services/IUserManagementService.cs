using App.Domain.Entities;
using App.Domain.Enums;

namespace App.Application.Interfaces.Services;

/// <summary>
/// Service for user management operations (admin only)
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Get all users in the system
    /// </summary>
    Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active users only
    /// </summary>
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new user (admin only)
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">Plain text password</param>
    /// <param name="email">Email address</param>
    /// <param name="firstName">First name</param>
    /// <param name="lastName">Last name</param>
    /// <param name="role">User role</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user</returns>
    Task<User> CreateUserAsync(
        string username,
        string password,
        string email,
        string firstName,
        string lastName,
        UserRole role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user information
    /// </summary>
    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate a user (soft delete)
    /// </summary>
    Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate a previously deactivated user
    /// </summary>
    Task ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if username is available
    /// </summary>
    Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if email is available
    /// </summary>
    Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);
}

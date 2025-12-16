using App.Domain.Entities;

namespace App.Application.Interfaces.Services;

/// <summary>
/// Service for user authentication and authorization
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticate a user with username and password
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authenticated user if successful, null otherwise</returns>
    Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="user">User to register</param>
    /// <param name="password">Plain text password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user</returns>
    Task<User> RegisterAsync(User user, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="currentPassword">Current password</param>
    /// <param name="newPassword">New password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate user credentials
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if credentials are valid</returns>
    Task<bool> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default);
}

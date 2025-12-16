using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain.Entities;

namespace App.Application.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        // Find user by username
        var users = await _userRepository.ListAsync(
            query => query.Where(u => u.Username == username && u.IsActive),
            cancellationToken
        );

        var user = users.FirstOrDefault();
        if (user == null)
            return null;

        // Verify password
        if (!_passwordHasher.VerifyPassword(user.PasswordHash, password))
            return null;

        return user;
    }

    public async Task<User> RegisterAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        // Check if username already exists
        var existingUsers = await _userRepository.ListAsync(
            query => query.Where(u => u.Username == user.Username),
            cancellationToken
        );

        if (existingUsers.Any())
            throw new InvalidOperationException($"Username '{user.Username}' is already taken");

        // Check if email already exists
        existingUsers = await _userRepository.ListAsync(
            query => query.Where(u => u.Email == user.Email),
            cancellationToken
        );

        if (existingUsers.Any())
            throw new InvalidOperationException($"Email '{user.Email}' is already registered");

        // Hash password
        user.PasswordHash = _passwordHasher.HashPassword(password);
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        // Save user
        return await _userRepository.AddAsync(user, cancellationToken);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            return false;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return false;

        // Verify current password
        if (!_passwordHasher.VerifyPassword(user.PasswordHash, currentPassword))
            return false;

        // Hash new password
        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }

    public async Task<bool> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await AuthenticateAsync(username, password, cancellationToken);
        return user != null;
    }
}

using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain.Entities;
using App.Domain.Enums;

namespace App.Application.Services;

/// <summary>
/// User management service implementation
/// </summary>
public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserManagementService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _userRepository.ListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _userRepository.ListAsync(
            query => query.Where(u => u.IsActive),
            cancellationToken
        );
    }

    public async Task<User> CreateUserAsync(
        string username,
        string password,
        string email,
        string firstName,
        string lastName,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required", nameof(password));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        // Check if username already exists
        if (!await IsUsernameAvailableAsync(username, cancellationToken))
            throw new InvalidOperationException($"Username '{username}' is already taken");

        // Check if email already exists
        if (!await IsEmailAvailableAsync(email, cancellationToken))
            throw new InvalidOperationException($"Email '{email}' is already registered");

        // Create user
        var user = new User
        {
            Username = username,
            PasswordHash = _passwordHasher.HashPassword(password),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return await _userRepository.AddAsync(user, cancellationToken);
    }

    public async Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return user;
    }

    public async Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.ListAsync(
            query => query.Where(u => u.Username == username),
            cancellationToken
        );
        return !users.Any();
    }

    public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.ListAsync(
            query => query.Where(u => u.Email == email),
            cancellationToken
        );
        return !users.Any();
    }
}

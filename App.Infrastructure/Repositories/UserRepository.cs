using App.Application.Interfaces.Repositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger) : base(context, logger)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching user with username {Username}", username);

        var user = await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (user != null)
            _logger.LogDebug("Retrieved user with username {Username} and ID {UserId}", username, user.Id);
        else
            _logger.LogDebug("User with username {Username} not found", username);

        return user;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching user with email {Email}", email);

        var user = await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user != null)
            _logger.LogDebug("Retrieved user with email {Email} and ID {UserId}", email, user.Id);
        else
            _logger.LogDebug("User with email {Email} not found", email);

        return user;
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching active users");

        var users = await _dbSet
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {UserCount} active users", users.Count);
        return users;
    }

    public async Task<User?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching user with tasks for user ID {UserId}", id);

        var user = await _dbSet
            .Include(u => u.AssignedTasks)
            .ThenInclude(t => t.Project)
            .Include(u => u.CreatedTasks)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user != null)
            _logger.LogDebug("Retrieved user with {TaskCount} assigned tasks and {CreatedCount} created tasks for user ID {UserId}",
                user.AssignedTasks?.Count ?? 0, user.CreatedTasks?.Count ?? 0, id);
        else
            _logger.LogDebug("User with ID {UserId} not found", id);

        return user;
    }
}

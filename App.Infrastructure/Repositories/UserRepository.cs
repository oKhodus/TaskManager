using App.Application.Interfaces.Repositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.AssignedTasks)
            .ThenInclude(t => t.Project)
            .Include(u => u.CreatedTasks)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}

using App.Application.Interfaces.Repositories;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Repositories;

public class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<RepositoryBase<T>> _logger;

    public RepositoryBase(ApplicationDbContext context, ILogger<RepositoryBase<T>> logger)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> ListAsync(Func<IQueryable<T>, IQueryable<T>> query, CancellationToken cancellationToken = default)
    {
        return await query(_dbSet).ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(T).Name;
        var entityId = entity.GetType().GetProperty("Id")?.GetValue(entity);
        _logger.LogDebug("Adding {EntityType} with ID {EntityId}", entityType, entityId);

        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully added {EntityType} with ID {EntityId}", entityType, entityId);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(T).Name;
        var entityId = entity.GetType().GetProperty("Id")?.GetValue(entity);
        _logger.LogDebug("Updating {EntityType} with ID {EntityId}", entityType, entityId);

        // Detach any existing tracked entity with the same key
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            var existingEntity = await _dbSet.FindAsync([entry.Property("Id").CurrentValue], cancellationToken);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = EntityState.Detached;
            }
        }

        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully updated {EntityType} with ID {EntityId}", entityType, entityId);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(T).Name;
        var entityId = entity.GetType().GetProperty("Id")?.GetValue(entity);
        _logger.LogDebug("Deleting {EntityType} with ID {EntityId}", entityType, entityId);

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully deleted {EntityType} with ID {EntityId}", entityType, entityId);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(Func<IQueryable<T>, IQueryable<T>> query, CancellationToken cancellationToken = default)
    {
        return await query(_dbSet).AnyAsync(cancellationToken);
    }
}

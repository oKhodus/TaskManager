namespace App.Application.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> ListAsync(Func<IQueryable<T>, IQueryable<T>> query, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Func<IQueryable<T>, IQueryable<T>> query, CancellationToken cancellationToken = default);
}

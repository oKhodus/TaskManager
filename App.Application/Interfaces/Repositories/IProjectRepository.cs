using App.Domain.Entities;

namespace App.Application.Interfaces.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default);
    Task<Project?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Project?> GetWithSprintsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsKeyUniqueAsync(string key, Guid? excludeProjectId = null, CancellationToken cancellationToken = default);
}

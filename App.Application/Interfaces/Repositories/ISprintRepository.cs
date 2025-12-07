using App.Domain.Entities;

namespace App.Application.Interfaces.Repositories;

public interface ISprintRepository : IRepository<Sprint>
{
    Task<IEnumerable<Sprint>> GetActiveSprintsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Sprint>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Sprint?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sprint?> GetCurrentSprintForProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}

using App.Domain.Entities;

namespace App.Application.Interfaces.Repositories;

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
}

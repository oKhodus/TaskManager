using App.Application.Interfaces.Repositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Repositories;

public class TagRepository : RepositoryBase<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext context, ILogger<TagRepository> logger) : base(context, logger)
    {
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching tag with name {TagName}", name);

        var tag = await _dbSet
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);

        if (tag != null)
            _logger.LogDebug("Retrieved tag with name {TagName} and ID {TagId}", name, tag.Id);
        else
            _logger.LogDebug("Tag with name {TagName} not found", name);

        return tag;
    }

    public async Task<IEnumerable<Tag>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching tags for task ID {TaskId}", taskId);

        var tags = await _dbSet
            .Where(t => t.Tasks.Any(task => task.Id == taskId))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {TagCount} tags for task ID {TaskId}", tags.Count, taskId);
        return tags;
    }
}

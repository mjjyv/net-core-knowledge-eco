using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Interfaces;
using Ecosystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.Infrastructure.Repositories;

/// <summary>
/// Repository chuyên biệt cho thực thể Khía cạnh cấp 2 (Aspect).
/// </summary>
public class AspectRepository : GenericRepository<Aspect>, IAspectRepository
{
    public AspectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Aspect>> GetByTopicIdAsync(Guid topicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.TopicId == topicId)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<Aspect?> GetByIdWithCategoriesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.Categories.OrderBy(c => c.OrderIndex))
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var cleanSlug = slug.Trim().ToLowerInvariant();
        var query = _dbSet.IgnoreQueryFilters().AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return await query.AnyAsync(a => a.Slug.ToLower() == cleanSlug, cancellationToken);
    }
}

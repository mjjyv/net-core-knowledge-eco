using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Interfaces;
using Ecosystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.Infrastructure.Repositories;

/// <summary>
/// Repository chuyên biệt cho thực thể Chuyên mục cấp 3 (Category).
/// </summary>
public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetByAspectIdAsync(Guid aspectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.AspectId == aspectId)
            .OrderBy(c => c.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var cleanSlug = slug.Trim().ToLowerInvariant();
        var query = _dbSet.IgnoreQueryFilters().AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(c => c.Slug.ToLower() == cleanSlug, cancellationToken);
    }
}

using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Interfaces;
using Ecosystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.Infrastructure.Repositories;

/// <summary>
/// Repository chuyên biệt cho thực thể Chủ đề lớn cấp 1 (Topic).
/// </summary>
public class TopicRepository : GenericRepository<Topic>, ITopicRepository
{
    public TopicRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Topic>> GetFilteredAsync(
        string? searchTerm = null,
        string? sortBy = null,
        bool isDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term) ||
                t.Slug.ToLower().Contains(term));
        }

        query = (sortBy?.ToLowerInvariant(), isDescending) switch
        {
            ("title", true) => query.OrderByDescending(t => t.Title),
            ("title", false) => query.OrderBy(t => t.Title),
            ("createdat", true) => query.OrderByDescending(t => t.CreatedAt),
            ("createdat", false) => query.OrderBy(t => t.CreatedAt),
            (_, true) => query.OrderByDescending(t => t.OrderIndex).ThenByDescending(t => t.CreatedAt),
            _ => query.OrderBy(t => t.OrderIndex).ThenBy(t => t.CreatedAt)
        };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Topic?> GetByIdWithAspectsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.Aspects.OrderBy(a => a.OrderIndex))
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Topic?> GetBySlugWithHierarchyAsync(string slug, CancellationToken cancellationToken = default)
    {
        var cleanSlug = slug.Trim().ToLowerInvariant();
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.Aspects.OrderBy(a => a.OrderIndex))
                .ThenInclude(a => a.Categories.OrderBy(c => c.OrderIndex))
                    .ThenInclude(c => c.Articles)
            .FirstOrDefaultAsync(t => t.Slug.ToLower() == cleanSlug, cancellationToken);
    }

    public async Task<IEnumerable<Topic>> GetAllWithFullHierarchyAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.Aspects.OrderBy(a => a.OrderIndex))
                .ThenInclude(a => a.Categories.OrderBy(c => c.OrderIndex))
                    .ThenInclude(c => c.Articles)
            .OrderBy(t => t.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var cleanSlug = slug.Trim().ToLowerInvariant();
        var query = _dbSet.IgnoreQueryFilters().AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync(t => t.Slug.ToLower() == cleanSlug, cancellationToken);
    }
}

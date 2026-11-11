using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;
using Ecosystem.Domain.Interfaces;
using Ecosystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.Infrastructure.Repositories;

/// <summary>
/// Repository chuyên biệt cho thực thể Article với tối ưu Eager Loading (.Include/.ThenInclude) và .AsNoTracking().
/// </summary>
public class ArticleRepository : GenericRepository<Article>, IArticleRepository
{
    public ArticleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Article>> GetFilteredAsync(
        Guid? categoryId = null,
        Guid? aspectId = null,
        Guid? topicId = null,
        Guid? authorId = null,
        ArticleStatus? status = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool isDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == categoryId.Value);
        }

        if (aspectId.HasValue)
        {
            query = query.Where(a => a.Category.AspectId == aspectId.Value);
        }

        if (topicId.HasValue)
        {
            query = query.Where(a => a.Category.Aspect.TopicId == topicId.Value);
        }

        if (authorId.HasValue)
        {
            query = query.Where(a => a.AuthorId == authorId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(a => a.Title.ToLower().Contains(term) || a.Summary.ToLower().Contains(term));
        }

        query = sortBy?.Trim().ToLowerInvariant() switch
        {
            "title" => isDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "views" => isDescending ? query.OrderByDescending(a => a.ViewCount) : query.OrderBy(a => a.ViewCount),
            "readingtime" => isDescending ? query.OrderByDescending(a => a.ReadingTimeMinutes) : query.OrderBy(a => a.ReadingTimeMinutes),
            _ => isDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt)
        };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Article?> GetByIdWithHierarchyAsync(Guid id, bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(a => a.Category)
                .ThenInclude(c => c.Aspect)
                    .ThenInclude(asp => asp.Topic)
            .Include(a => a.Author)
            .AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Article?> GetBySlugWithHierarchyAsync(string slug, bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        var cleanSlug = slug.Trim().ToLowerInvariant();
        var query = _dbSet
            .Include(a => a.Category)
                .ThenInclude(c => c.Aspect)
                    .ThenInclude(asp => asp.Topic)
            .Include(a => a.Author)
            .AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(a => a.Slug.ToLower() == cleanSlug, cancellationToken);
    }

    public async Task<IEnumerable<Article>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => a.CategoryId == categoryId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
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

    public async Task<Article?> GetByIdIgnoreFiltersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}

using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;

namespace Ecosystem.Domain.Interfaces;

/// <summary>
/// Repository chuyên biệt cho thực thể Bài viết (Article) với các truy vấn phân cấp 4 tầng tối ưu.
/// </summary>
public interface IArticleRepository : IRepository<Article>
{
    Task<IEnumerable<Article>> GetFilteredAsync(
        Guid? categoryId = null,
        Guid? aspectId = null,
        Guid? topicId = null,
        Guid? authorId = null,
        ArticleStatus? status = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool isDescending = true,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Article> Items, int TotalCount)> GetPagedAsync(
        Guid? categoryId = null,
        Guid? aspectId = null,
        Guid? topicId = null,
        Guid? authorId = null,
        ArticleStatus? status = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool isDescending = true,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<Article?> GetByIdWithHierarchyAsync(Guid id, bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task<Article?> GetBySlugWithHierarchyAsync(string slug, bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<Article>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<Article?> GetByIdIgnoreFiltersAsync(Guid id, CancellationToken cancellationToken = default);
}

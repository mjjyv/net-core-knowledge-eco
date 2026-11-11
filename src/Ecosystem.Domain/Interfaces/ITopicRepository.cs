using Ecosystem.Domain.Entities;

namespace Ecosystem.Domain.Interfaces;

/// <summary>
/// Repository chuyên biệt cho thực thể Chủ đề lớn cấp 1 (Topic).
/// </summary>
public interface ITopicRepository : IRepository<Topic>
{
    Task<IEnumerable<Topic>> GetFilteredAsync(
        string? searchTerm = null,
        string? sortBy = null,
        bool isDescending = false,
        CancellationToken cancellationToken = default);

    Task<Topic?> GetByIdWithAspectsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Topic?> GetBySlugWithHierarchyAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Topic>> GetAllWithFullHierarchyAsync(CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

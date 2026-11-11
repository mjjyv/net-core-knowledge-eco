using Ecosystem.Domain.Entities;

namespace Ecosystem.Domain.Interfaces;

/// <summary>
/// Repository chuyên biệt cho thực thể Chuyên mục cấp 3 (Category).
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetByAspectIdAsync(Guid aspectId, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

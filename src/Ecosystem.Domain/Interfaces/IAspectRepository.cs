using Ecosystem.Domain.Entities;

namespace Ecosystem.Domain.Interfaces;

/// <summary>
/// Repository chuyên biệt cho thực thể Khía cạnh cấp 2 (Aspect).
/// </summary>
public interface IAspectRepository : IRepository<Aspect>
{
    Task<IEnumerable<Aspect>> GetByTopicIdAsync(Guid topicId, CancellationToken cancellationToken = default);
    Task<Aspect?> GetByIdWithCategoriesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

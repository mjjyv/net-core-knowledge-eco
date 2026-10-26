using Ecosystem.Domain.Common;

namespace Ecosystem.Domain.Entities;

/// <summary>
/// Chuyên mục cụ thể cấp 3 thuộc về một Aspect (ví dụ: Dependency Injection, EF Core Advanced).
/// </summary>
public class Category : BaseEntity
{
    public Guid AspectId { get; set; }
    public Aspect Aspect { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; } = 0;

    // Navigation collection
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}

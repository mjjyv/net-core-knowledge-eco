using Ecosystem.Domain.Common;

namespace Ecosystem.Domain.Entities;

/// <summary>
/// Chủ đề lớn cấp 1 trong Cây Tri Thức (ví dụ: Nền tảng Lập trình .NET, Hệ thống Phân tán).
/// </summary>
public class Topic : BaseEntity, IAggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; } = 0;

    // Navigation collection
    public ICollection<Aspect> Aspects { get; set; } = new List<Aspect>();
}

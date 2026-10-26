using Ecosystem.Domain.Common;

namespace Ecosystem.Domain.Entities;

/// <summary>
/// Khía cạnh chuyên sâu cấp 2 thuộc về một Topic (ví dụ: ASP.NET Core Backend, Realtime SignalR).
/// </summary>
public class Aspect : BaseEntity
{
    public Guid TopicId { get; set; }
    public Topic Topic { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; } = 0;

    // Navigation collection
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}

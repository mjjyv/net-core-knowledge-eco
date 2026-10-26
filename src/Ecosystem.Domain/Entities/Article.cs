using Ecosystem.Domain.Common;
using Ecosystem.Domain.Enums;

namespace Ecosystem.Domain.Entities;

/// <summary>
/// Bài viết tri thức Markdown cấp 4 trong Cây Tri Thức.
/// </summary>
public class Article : BaseEntity, IAggregateRoot
{
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string MarkdownContent { get; set; } = string.Empty;
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public int ViewCount { get; set; } = 0;
    public int ReadingTimeMinutes { get; set; } = 0;
    public string TagsJson { get; set; } = "[]";
}

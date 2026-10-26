namespace Ecosystem.Domain.Enums;

/// <summary>
/// Trạng thái xuất bản của bài viết tri thức Markdown.
/// </summary>
public enum ArticleStatus
{
    /// <summary>
    /// Bản nháp (chưa công khai)
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Đã xuất bản công khai
    /// </summary>
    Published = 1,

    /// <summary>
    /// Đã lưu trữ (ẩn khỏi danh sách thông thường)
    /// </summary>
    Archived = 2
}

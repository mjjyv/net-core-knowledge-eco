using System.ComponentModel.DataAnnotations;
using Ecosystem.Application.Common;
using Ecosystem.Application.Validation;
using Ecosystem.Domain.Enums;

namespace Ecosystem.Application.DTOs;

/// <summary>
/// DTO tiếp nhận yêu cầu tạo mới Bài viết tri thức cấp 4 (Markdown Article).
/// </summary>
public class ArticleCreateDto
{
    /// <summary>
    /// Mã chuyên mục cấp 3 (Category) mà bài viết trực thuộc.
    /// </summary>
    /// <example>11111111-aaaa-c111-1111-111111111111</example>
    [Required(ErrorMessage = "Mã chuyên mục (CategoryId) là bắt buộc.")]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Mã tác giả (AuthorId). Nếu không truyền sẽ gán mặc định tài khoản quản trị viên.
    /// </summary>
    /// <example>aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa</example>
    public Guid? AuthorId { get; set; }

    /// <summary>
    /// Tiêu đề bài viết.
    /// </summary>
    /// <example>Kiến Trúc Clean Architecture Với .NET 8</example>
    [Required(ErrorMessage = "Tiêu đề bài viết không được để trống.")]
    [StringLength(250, MinimumLength = 5, ErrorMessage = "Tiêu đề bài viết phải có độ dài từ 5 đến 250 ký tự.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn định danh chuẩn SEO (Slug). Nếu để trống hệ thống sẽ tự động tạo từ tiêu đề.
    /// </summary>
    /// <example>kien-truc-clean-architecture-voi-net-8</example>
    [StringLength(280, ErrorMessage = "Slug không được vượt quá 280 ký tự.")]
    [SlugValidation(ErrorMessage = "Slug phải theo định dạng kebab-case chuẩn (ví dụ: 'bai-viet-mau').")]
    public string? Slug { get; set; }

    /// <summary>
    /// Tóm tắt nội dung bài viết.
    /// </summary>
    /// <example>Tổng quan về cách phân tách các tầng trong Clean Architecture ứng dụng ASP.NET Core.</example>
    [StringLength(1000, ErrorMessage = "Tóm tắt không được vượt quá 1000 ký tự.")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung chi tiết bài viết theo định dạng Markdown.
    /// </summary>
    /// <example># Hướng Dẫn Clean Architecture&#xA;&#xA;Nội dung chi tiết bài viết...</example>
    [Required(ErrorMessage = "Nội dung Markdown của bài viết không được để trống.")]
    public string MarkdownContent { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái xuất bản của bài viết (Draft = 0, Published = 1, Archived = 2).
    /// </summary>
    /// <example>1</example>
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

    /// <summary>
    /// Thời gian đọc ước tính tính theo phút. Nếu không nhập hệ thống sẽ tự động tính toán.
    /// </summary>
    /// <example>5</example>
    [Range(0, 1440, ErrorMessage = "Thời gian đọc phải từ 0 đến 1440 phút.")]
    public int? ReadingTimeMinutes { get; set; }

    /// <summary>
    /// Danh sách các thẻ từ khóa (tags) gắn với bài viết.
    /// </summary>
    /// <example>["dotnet8", "clean-architecture", "csharp"]</example>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// DTO tiếp nhận yêu cầu cập nhật Bài viết tri thức.
/// </summary>
public class ArticleUpdateDto
{
    /// <summary>
    /// Mã chuyên mục cấp 3 (Category) mà bài viết trực thuộc.
    /// </summary>
    /// <example>11111111-aaaa-c111-1111-111111111111</example>
    [Required(ErrorMessage = "Mã chuyên mục (CategoryId) là bắt buộc.")]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Tiêu đề bài viết.
    /// </summary>
    /// <example>Kiến Trúc Clean Architecture Với .NET 8 (Bản Cập Nhật)</example>
    [Required(ErrorMessage = "Tiêu đề bài viết không được để trống.")]
    [StringLength(250, MinimumLength = 5, ErrorMessage = "Tiêu đề bài viết phải có độ dài từ 5 đến 250 ký tự.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn định danh chuẩn SEO (Slug). Nếu để trống hệ thống sẽ tự động cập nhật từ tiêu đề mới.
    /// </summary>
    /// <example>kien-truc-clean-architecture-voi-net-8-cap-nhat</example>
    [StringLength(280, ErrorMessage = "Slug không được vượt quá 280 ký tự.")]
    [SlugValidation(ErrorMessage = "Slug phải theo định dạng kebab-case chuẩn.")]
    public string? Slug { get; set; }

    /// <summary>
    /// Tóm tắt nội dung bài viết.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Tóm tắt không được vượt quá 1000 ký tự.")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung chi tiết bài viết theo định dạng Markdown.
    /// </summary>
    [Required(ErrorMessage = "Nội dung Markdown của bài viết không được để trống.")]
    public string MarkdownContent { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái xuất bản của bài viết (Draft = 0, Published = 1, Archived = 2).
    /// </summary>
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

    /// <summary>
    /// Thời gian đọc ước tính tính theo phút.
    /// </summary>
    [Range(0, 1440, ErrorMessage = "Thời gian đọc phải từ 0 đến 1440 phút.")]
    public int? ReadingTimeMinutes { get; set; }

    /// <summary>
    /// Danh sách các thẻ từ khóa (tags) gắn với bài viết.
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// DTO phản hồi thông tin tóm tắt bài viết trong danh sách (loại bỏ nội dung Markdown lớn để tối ưu truyền tải).
/// </summary>
public class ArticleResponseDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryTitle { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public ArticleStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int ViewCount { get; set; }
    public int ReadingTimeMinutes { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO phản hồi chi tiết bài viết tri thức kèm đầy đủ nội dung Markdown và chuỗi phân cấp Breadcrumbs (Topic -> Aspect -> Category -> Article).
/// </summary>
public class ArticleDetailResponseDto : ArticleResponseDto
{
    /// <summary>
    /// Nội dung toàn văn định dạng Markdown.
    /// </summary>
    public string MarkdownContent { get; set; } = string.Empty;

    /// <summary>
    /// Mã chủ đề lớn cấp 1 (Topic).
    /// </summary>
    public Guid TopicId { get; set; }

    /// <summary>
    /// Tiêu đề chủ đề lớn cấp 1.
    /// </summary>
    public string TopicTitle { get; set; } = string.Empty;

    /// <summary>
    /// Slug chủ đề lớn cấp 1.
    /// </summary>
    public string TopicSlug { get; set; } = string.Empty;

    /// <summary>
    /// Mã khía cạnh cấp 2 (Aspect).
    /// </summary>
    public Guid AspectId { get; set; }

    /// <summary>
    /// Tiêu đề khía cạnh cấp 2.
    /// </summary>
    public string AspectTitle { get; set; } = string.Empty;

    /// <summary>
    /// Slug khía cạnh cấp 2.
    /// </summary>
    public string AspectSlug { get; set; } = string.Empty;
}

/// <summary>
/// Query parameters phân trang, lọc, tìm kiếm và sắp xếp danh sách bài viết (kế thừa PaginationParams).
/// </summary>
public class ArticleFilterQuery : PaginationParams
{
    /// <summary>
    /// Lọc theo chuyên mục cấp 3 (Category).
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Lọc theo khía cạnh cấp 2 (Aspect).
    /// </summary>
    public Guid? AspectId { get; set; }

    /// <summary>
    /// Lọc theo chủ đề cấp 1 (Topic).
    /// </summary>
    public Guid? TopicId { get; set; }

    /// <summary>
    /// Lọc theo tác giả (Author).
    /// </summary>
    public Guid? AuthorId { get; set; }

    /// <summary>
    /// Lọc theo trạng thái xuất bản (Draft = 0, Published = 1, Archived = 2).
    /// </summary>
    public ArticleStatus? Status { get; set; }
}

/// <summary>
/// Liên kết tệp đính kèm được trích xuất trong gói đồng bộ ecosystem://bundle.
/// </summary>
public class ArticleAttachmentLinkDto
{
    public string FileName { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}

/// <summary>
/// Gói dữ liệu tri thức tự chứa phục vụ đồng bộ liên thông sâu (ecosystem://bundle - Phase 14).
/// </summary>
public class ArticleBundleDto
{
    public Guid ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string MarkdownContent { get; set; } = string.Empty;
    public string CategoryPath { get; set; } = string.Empty;
    public string DeepLink { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<ArticleAttachmentLinkDto> Attachments { get; set; } = new();
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
}

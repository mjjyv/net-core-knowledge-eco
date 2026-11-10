using Ecosystem.Application.DTOs;

namespace Ecosystem.Application.Interfaces;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản trị Bài viết tri thức (Article) với EF Core LINQ và quan hệ phân cấp.
/// </summary>
public interface IArticleService
{
    /// <summary>
    /// Lấy danh sách bài viết hỗ trợ lọc, tìm kiếm và sắp xếp. Dùng AsNoTracking và projection DTO.
    /// </summary>
    Task<IEnumerable<ArticleResponseDto>> GetAllAsync(ArticleFilterQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy chi tiết bài viết theo GUID (kèm breadcrumbs phân cấp 4 tầng Topic -> Aspect -> Category -> Article).
    /// </summary>
    Task<ArticleDetailResponseDto?> GetByIdAsync(Guid id, bool incrementView = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy chi tiết bài viết theo Slug chuẩn SEO.
    /// </summary>
    Task<ArticleDetailResponseDto?> GetBySlugAsync(string slug, bool incrementView = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách bài viết thuộc về một chuyên mục cụ thể (Lab 5 Buổi 6).
    /// </summary>
    Task<IEnumerable<ArticleResponseDto>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo mới một bài viết tri thức (tự động tạo slug, kiểm tra trùng lặp và tính reading time).
    /// </summary>
    Task<ArticleDetailResponseDto> CreateAsync(ArticleCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin bài viết tri thức qua EF Core Change Tracking.
    /// </summary>
    Task<ArticleDetailResponseDto?> UpdateAsync(Guid id, ArticleUpdateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa bài viết (mặc định xóa mềm Soft Delete, hoặc xóa cứng Hard Delete nếu yêu cầu).
    /// </summary>
    Task<bool> DeleteAsync(Guid id, bool isHardDelete = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem một Slug đã tồn tại trong CSDL hay chưa.
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

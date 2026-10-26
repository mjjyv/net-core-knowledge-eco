namespace Ecosystem.Application.Interfaces;

/// <summary>
/// DTO chứa thông tin tóm tắt chuyên mục (Category) cấp 3.
/// </summary>
public record CategoryOverviewDto(Guid Id, string Title, string Slug, int ArticleCount);

/// <summary>
/// DTO chứa thông tin tóm tắt khía cạnh (Aspect) cấp 2.
/// </summary>
public record AspectOverviewDto(Guid Id, string Title, string Slug, IReadOnlyList<CategoryOverviewDto> Categories);

/// <summary>
/// DTO chứa thông tin tóm tắt chủ đề lớn (Topic) cấp 1.
/// </summary>
public record TopicOverviewDto(Guid Id, string Title, string Slug, string Description, IReadOnlyList<AspectOverviewDto> Aspects);

/// <summary>
/// Dịch vụ cung cấp thông tin tổng quan về Cây Tri Thức 4 cấp của The Ecosystem.
/// </summary>
public interface ITaxonomyOverviewService
{
    Task<IReadOnlyList<TopicOverviewDto>> GetTaxonomyHierarchyAsync(CancellationToken cancellationToken = default);
    Task<TopicOverviewDto> GetTopicBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

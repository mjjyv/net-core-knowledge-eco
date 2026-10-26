using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

[ApiController]
[Route("api/v1/taxonomy")]
public class TaxonomyOverviewController : ControllerBase
{
    private readonly ITaxonomyOverviewService _taxonomyService;

    public TaxonomyOverviewController(ITaxonomyOverviewService taxonomyService)
    {
        _taxonomyService = taxonomyService;
    }

    /// <summary>
    /// Lấy toàn bộ cây phân cấp tri thức 4 cấp của The Ecosystem.
    /// </summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var hierarchy = await _taxonomyService.GetTaxonomyHierarchyAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TopicOverviewDto>>.Ok(hierarchy, "Lấy thông tin tổng quan Cây Tri Thức thành công."));
    }

    /// <summary>
    /// Lấy chi tiết một chủ đề theo slug.
    /// </summary>
    [HttpGet("topics/{slug}")]
    public async Task<IActionResult> GetTopicBySlug(string slug, CancellationToken cancellationToken)
    {
        var topic = await _taxonomyService.GetTopicBySlugAsync(slug, cancellationToken);
        return Ok(ApiResponse<TopicOverviewDto>.Ok(topic, $"Lấy chủ đề '{slug}' thành công."));
    }
}

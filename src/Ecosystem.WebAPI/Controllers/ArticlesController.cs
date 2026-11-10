using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý RESTful CRUD API cho Bài viết tri thức cấp 4 (Articles) trong Hệ sinh thái (Phase 06).
/// Áp dụng tối ưu EF Core LINQ, Eager Loading (.Include/.ThenInclude) và ánh xạ DTO triệt tiêu Object Cycle.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    /// <summary>
    /// Lấy danh sách các bài viết tri thức, hỗ trợ lọc đa chiều, tìm kiếm từ khóa và sắp xếp.
    /// </summary>
    /// <param name="query">Tham số lọc (CategoryId, AspectId, TopicId, AuthorId, Status, SearchTerm, SortBy, IsDescending).</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <returns>Danh sách tóm lược các bài viết dạng ArticleResponseDto.</returns>
    /// <response code="200">Lấy danh sách bài viết thành công.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArticleResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleResponseDto>>>> GetAll(
        [FromQuery] ArticleFilterQuery query,
        CancellationToken cancellationToken = default)
    {
        var articles = await _articleService.GetAllAsync(query, cancellationToken);
        return Ok(ApiResponse<IEnumerable<ArticleResponseDto>>.Ok(articles, "Lấy danh sách bài viết thành công."));
    }

    /// <summary>
    /// Lấy thông tin chi tiết một bài viết theo định danh GUID (kèm chuỗi phân cấp Topic -> Aspect -> Category và tăng lượt xem).
    /// </summary>
    /// <param name="id">Mã định danh GUID của bài viết.</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <returns>Chi tiết bài viết dạng ArticleDetailResponseDto.</returns>
    /// <response code="200">Tìm thấy chi tiết bài viết.</response>
    /// <response code="404">Không tìm thấy bài viết với Id tương ứng.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ArticleDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticleDetailResponseDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleService.GetByIdAsync(id, incrementView: true, cancellationToken);
        if (article == null)
        {
            return NotFound(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Không tìm thấy bài viết",
                $"Không tìm thấy bài viết với mã định danh '{id}'."));
        }

        return Ok(ApiResponse<ArticleDetailResponseDto>.Ok(article, "Lấy chi tiết bài viết thành công."));
    }

    /// <summary>
    /// Tra cứu thông tin chi tiết bài viết theo đường dẫn chuẩn SEO (Slug).
    /// </summary>
    /// <param name="slug">Chuỗi slug kebab-case của bài viết.</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <returns>Chi tiết bài viết dạng ArticleDetailResponseDto.</returns>
    /// <response code="200">Tìm thấy chi tiết bài viết theo slug.</response>
    /// <response code="404">Không tìm thấy bài viết với slug tương ứng.</response>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ApiResponse<ArticleDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticleDetailResponseDto>>> GetBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleService.GetBySlugAsync(slug, incrementView: true, cancellationToken);
        if (article == null)
        {
            return NotFound(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Không tìm thấy bài viết",
                $"Không tìm thấy bài viết với slug '{slug}'."));
        }

        return Ok(ApiResponse<ArticleDetailResponseDto>.Ok(article, "Lấy chi tiết bài viết theo slug thành công."));
    }

    /// <summary>
    /// Tạo mới một bài viết tri thức Markdown trong hệ sinh thái.
    /// Tự động sinh slug chuẩn SEO từ tiêu đề nếu không truyền và kiểm tra tính duy nhất.
    /// </summary>
    /// <param name="dto">Dữ liệu khởi tạo bài viết.</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <returns>Chi tiết bài viết vừa được tạo kèm Location Header.</returns>
    /// <response code="201">Tạo bài viết mới thành công.</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ hoặc CategoryId/AuthorId không tồn tại.</response>
    /// <response code="409">Đường dẫn Slug của bài viết đã tồn tại trong hệ thống.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ArticleDetailResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ArticleDetailResponseDto>>> Create(
        [FromBody] ArticleCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var created = await _articleService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                ApiResponse<ArticleDetailResponseDto>.Ok(created, "Tạo bài viết mới thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Tham chiếu dữ liệu không hợp lệ",
                ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Xung đột dữ liệu bài viết",
                ex.Message));
        }
    }

    /// <summary>
    /// Cập nhật toàn diện nội dung và thông tin phân loại của bài viết.
    /// </summary>
    /// <param name="id">Mã định danh GUID của bài viết cần cập nhật.</param>
    /// <param name="dto">Dữ liệu cập nhật bài viết.</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <returns>Thông tin chi tiết bài viết sau khi cập nhật.</returns>
    /// <response code="200">Cập nhật bài viết thành công.</response>
    /// <response code="400">Dữ liệu không hợp lệ hoặc CategoryId không tồn tại.</response>
    /// <response code="404">Không tìm thấy bài viết cần cập nhật.</response>
    /// <response code="409">Slug mới bị trùng lặp với bài viết khác.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ArticleDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ArticleDetailResponseDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] ArticleUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await _articleService.UpdateAsync(id, dto, cancellationToken);
            if (updated == null)
            {
                return NotFound(CreateProblemDetails(
                    StatusCodes.Status404NotFound,
                    "Không tìm thấy bài viết",
                    $"Không tìm thấy bài viết với mã '{id}' để cập nhật."));
            }

            return Ok(ApiResponse<ArticleDetailResponseDto>.Ok(updated, "Cập nhật bài viết thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Tham chiếu chuyên mục không hợp lệ",
                ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Xung đột dữ liệu slug",
                ex.Message));
        }
    }

    /// <summary>
    /// Xóa bài viết khỏi hệ thống (mặc định thực hiện xóa mềm Soft Delete để bảo toàn lịch sử).
    /// </summary>
    /// <param name="id">Mã định danh GUID của bài viết cần xóa.</param>
    /// <param name="isHardDelete">Cờ xóa vĩnh viễn khỏi CSDL (mặc định là false - xóa mềm).</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <response code="204">Xóa bài viết thành công.</response>
    /// <response code="404">Không tìm thấy bài viết cần xóa.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        [FromQuery] bool isHardDelete = false,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _articleService.DeleteAsync(id, isHardDelete, cancellationToken);
        if (!deleted)
        {
            return NotFound(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Không tìm thấy bài viết",
                $"Không tìm thấy bài viết với mã '{id}' để xóa."));
        }

        return NoContent();
    }

    private ProblemDetails CreateProblemDetails(int statusCode, string title, string detail)
    {
        return new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = HttpContext?.Request?.Path ?? "/api/v1/articles"
        };
    }
}

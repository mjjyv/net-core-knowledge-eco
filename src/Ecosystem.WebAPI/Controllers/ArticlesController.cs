using System.Security.Claims;
using System.Text.Json;
using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý RESTful CRUD API cho Bài viết tri thức cấp 4 (Articles) trong Hệ sinh thái (Phase 06 &amp; Phase 09).
/// Áp dụng tối ưu EF Core LINQ, Eager Loading (.Include/.ThenInclude), ánh xạ DTO và Resource-based Authorization.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;
    private readonly IAuthorizationService? _authorizationService;

    public ArticlesController(
        IArticleService articleService,
        IAuthorizationService? authorizationService = null)
    {
        _articleService = articleService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Lấy danh sách các bài viết tri thức có phân trang (Skip/Take), lọc đa chiều, tìm kiếm từ khóa và sắp xếp động (kèm Header X-Pagination).
    /// </summary>
    /// <param name="query">Tham số truy vấn (PageNumber, PageSize, CategoryId, AspectId, TopicId, AuthorId, Status, SearchTerm, SortBy, IsDescending).</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <returns>Kết quả phân trang PagedResult&lt;ArticleResponseDto&gt; kèm header X-Pagination.</returns>
    /// <response code="200">Lấy danh sách bài viết phân trang thành công.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ArticleResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ArticleResponseDto>>>> GetAll(
        [FromQuery] ArticleFilterQuery query,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _articleService.GetPagedAsync(query, cancellationToken);

        if (Response != null)
        {
            var paginationHeader = JsonSerializer.Serialize(new
            {
                pagedResult.TotalCount,
                pagedResult.PageSize,
                pagedResult.PageNumber,
                pagedResult.TotalPages,
                pagedResult.HasPreviousPage,
                pagedResult.HasNextPage
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            Response.Headers["X-Pagination"] = paginationHeader;
        }

        return Ok(ApiResponse<PagedResult<ArticleResponseDto>>.Ok(pagedResult, "Lấy danh sách bài viết phân trang thành công."));
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
    [AllowAnonymous]
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
    [AllowAnonymous]
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
    /// Xuất bản gói dữ liệu tự chứa ecosystem://bundle phục vụ đồng bộ Desktop PKM (Phase 14).
    /// </summary>
    /// <param name="id">Mã định danh GUID của bài viết.</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <returns>Gói dữ liệu ArticleBundleDto gồm Markdown, CategoryPath, Tags và Attachments.</returns>
    /// <response code="200">Xuất bản gói đồng bộ thành công.</response>
    /// <response code="404">Không tìm thấy bài viết với Id tương ứng.</response>
    [HttpGet("{id:guid}/bundle")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ArticleBundleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticleBundleDto>>> GetBundle(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = Request != null ? $"{Request.Scheme}://{Request.Host}" : null;
        var bundle = await _articleService.GetBundleByIdAsync(id, baseUrl, cancellationToken);
        if (bundle == null)
        {
            return NotFound(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Không tìm thấy bài viết",
                $"Không tìm thấy bài viết với mã định danh '{id}' để xuất bản gói bundle."));
        }

        return Ok(ApiResponse<ArticleBundleDto>.Ok(bundle, "Xuất bản gói dữ liệu ecosystem://bundle thành công."));
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
    [Authorize(Policy = AuthConstants.Policies.RequireContentCreator)]
    [ProducesResponseType(typeof(ApiResponse<ArticleDetailResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ArticleDetailResponseDto>>> Create(
        [FromBody] ArticleCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (Guid.TryParse(userIdClaim, out var currentUserId))
            {
                if (!User.IsInRole(AuthConstants.Roles.Host) || !dto.AuthorId.HasValue || dto.AuthorId == Guid.Empty)
                {
                    dto.AuthorId = currentUserId;
                }
            }
        }

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
    [Authorize(Policy = AuthConstants.Policies.RequireContentCreator)]
    [ProducesResponseType(typeof(ApiResponse<ArticleDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ArticleDetailResponseDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] ArticleUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        var authFailure = await AuthorizeArticleModificationAsync(id, "cập nhật", cancellationToken);
        if (authFailure != null)
        {
            return authFailure;
        }

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
    [Authorize(Policy = AuthConstants.Policies.RequireContentCreator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        [FromQuery] bool isHardDelete = false,
        CancellationToken cancellationToken = default)
    {
        var authFailure = await AuthorizeArticleModificationAsync(id, "xóa", cancellationToken);
        if (authFailure != null)
        {
            return authFailure;
        }

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

    private async Task<ActionResult?> AuthorizeArticleModificationAsync(
        Guid id,
        string actionDescription,
        CancellationToken cancellationToken)
    {
        if (_authorizationService == null || User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var existingArticle = await _articleService.GetByIdAsync(id, incrementView: false, cancellationToken);
        if (existingArticle == null)
        {
            return NotFound(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Không tìm thấy bài viết",
                $"Không tìm thấy bài viết với mã '{id}' để {actionDescription}."));
        }

        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            existingArticle,
            AuthConstants.Policies.ArticleOwnerOrHost);

        return authResult.Succeeded ? null : Forbid();
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

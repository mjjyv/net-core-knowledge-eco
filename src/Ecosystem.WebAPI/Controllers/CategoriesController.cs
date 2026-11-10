using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Mappers;
using Ecosystem.Application.Models;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý RESTful endpoints cho Chuyên mục cụ thể cấp 3 (Categories).
/// Áp dụng DTO Response chuẩn và Data Annotations Validation.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ITaxonomyService _taxonomyService;
    private readonly IArticleService? _articleService;

    public CategoriesController(
        ITaxonomyService taxonomyService,
        IArticleService? articleService = null)
    {
        _taxonomyService = taxonomyService;
        _articleService = articleService;
    }

    /// <summary>
    /// Lấy chi tiết một Chuyên mục theo định danh GUID.
    /// </summary>
    /// <param name="id">Mã GUID của Chuyên mục.</param>
    /// <returns>Thông tin Chuyên mục dạng CategoryResponseDto.</returns>
    /// <response code="200">Tìm thấy Chuyên mục thành công.</response>
    /// <response code="404">Không tìm thấy Chuyên mục với Id tương ứng.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> GetById([FromRoute] Guid id)
    {
        var category = await _taxonomyService.GetCategoryByIdAsync(id);
        return Ok(ApiResponse<CategoryResponseDto>.Ok(category.ToResponseDto(), $"Lấy Chuyên mục với Id '{id}' thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin một Chuyên mục (Idempotent).
    /// </summary>
    /// <param name="id">Mã GUID trên URL route.</param>
    /// <param name="request">Dữ liệu cập nhật CategoryUpdateDto.</param>
    /// <response code="204">Cập nhật thành công, không trả về body.</response>
    /// <response code="400">ID trên route không khớp ID body hoặc dữ liệu vi phạm validation.</response>
    /// <response code="404">Không tìm thấy Chuyên mục cần cập nhật.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] CategoryUpdateDto request)
    {
        if (id != request.Id)
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "Bất đồng bộ định danh",
                Status = StatusCodes.Status400BadRequest,
                Detail = "ID trên URL route không khớp với ID trong request body.",
                Instance = HttpContext?.Request?.Path ?? $"/api/v1/categories/{id}"
            });
        }

        var updateRequest = new UpdateCategoryRequest
        {
            Id = request.Id,
            Title = request.Title,
            Slug = request.Slug,
            Description = request.Description,
            OrderIndex = request.OrderIndex
        };

        await _taxonomyService.UpdateCategoryAsync(id, updateRequest);
        return NoContent();
    }

    /// <summary>
    /// Lấy danh sách toàn bộ bài viết trực thuộc một Chuyên mục cụ thể (Lab 5 Buổi 6 - Quan hệ 1-N).
    /// </summary>
    /// <param name="id">Mã định danh GUID của Chuyên mục.</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <returns>Danh sách bài viết thuộc chuyên mục dạng ArticleResponseDto.</returns>
    /// <response code="200">Lấy danh sách bài viết thành công.</response>
    /// <response code="404">Không tìm thấy Chuyên mục tương ứng.</response>
    [HttpGet("{id:guid}/articles")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArticleResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleResponseDto>>>> GetArticlesByCategoryId(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (_articleService == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<IEnumerable<ArticleResponseDto>>.Fail("ArticleService chưa được cấu hình."));
        }

        try
        {
            var articles = await _articleService.GetByCategoryIdAsync(id, cancellationToken);
            return Ok(ApiResponse<IEnumerable<ArticleResponseDto>>.Ok(articles, $"Lấy danh sách bài viết của chuyên mục '{id}' thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Type = "https://httpstatuses.com/404",
                Title = "Không tìm thấy Chuyên mục",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message,
                Instance = HttpContext?.Request?.Path ?? $"/api/v1/categories/{id}/articles"
            });
        }
    }

    /// <summary>
    /// Xóa mềm một Chuyên mục (Idempotent).
    /// </summary>
    /// <param name="id">Mã GUID của Chuyên mục cần xóa.</param>
    /// <response code="204">Xóa thành công, không trả về body.</response>
    /// <response code="404">Không tìm thấy Chuyên mục cần xóa.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await _taxonomyService.DeleteCategoryAsync(id);
        return NoContent();
    }
}

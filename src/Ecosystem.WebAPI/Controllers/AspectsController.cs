using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Mappers;
using Ecosystem.Application.Models;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý RESTful endpoints cho Khía cạnh chuyên sâu cấp 2 (Aspects) và các Chuyên mục lồng ghép trực thuộc.
/// Áp dụng DTO Response chuẩn và Data Annotations Validation.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AspectsController : ControllerBase
{
    private readonly ITaxonomyService _taxonomyService;

    public AspectsController(ITaxonomyService taxonomyService)
    {
        _taxonomyService = taxonomyService;
    }

    /// <summary>
    /// Lấy chi tiết một Khía cạnh theo định danh GUID kèm danh sách Chuyên mục (Categories).
    /// </summary>
    /// <param name="id">Mã GUID của Khía cạnh.</param>
    /// <returns>Chi tiết Khía cạnh dạng AspectDetailResponseDto.</returns>
    /// <response code="200">Tìm thấy Khía cạnh thành công.</response>
    /// <response code="404">Không tìm thấy Khía cạnh với Id tương ứng.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AspectDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AspectDetailResponseDto>>> GetById([FromRoute] Guid id)
    {
        var aspect = await _taxonomyService.GetAspectByIdAsync(id);
        return Ok(ApiResponse<AspectDetailResponseDto>.Ok(aspect.ToDetailResponseDto(), $"Lấy Khía cạnh với Id '{id}' thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin một Khía cạnh (Idempotent).
    /// </summary>
    /// <param name="id">Mã GUID trên URL route.</param>
    /// <param name="request">Dữ liệu cập nhật AspectUpdateDto.</param>
    /// <response code="204">Cập nhật thành công, không trả về body.</response>
    /// <response code="400">ID trên route không khớp ID body hoặc dữ liệu vi phạm validation.</response>
    /// <response code="404">Không tìm thấy Khía cạnh cần cập nhật.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] AspectUpdateDto request)
    {
        if (id != request.Id)
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "Bất đồng bộ định danh",
                Status = StatusCodes.Status400BadRequest,
                Detail = "ID trên URL route không khớp với ID trong request body.",
                Instance = HttpContext?.Request?.Path ?? $"/api/v1/aspects/{id}"
            });
        }

        var updateRequest = new UpdateAspectRequest
        {
            Id = request.Id,
            Title = request.Title,
            Slug = request.Slug,
            Description = request.Description,
            OrderIndex = request.OrderIndex
        };

        await _taxonomyService.UpdateAspectAsync(id, updateRequest);
        return NoContent();
    }

    /// <summary>
    /// Xóa mềm một Khía cạnh (Idempotent).
    /// </summary>
    /// <param name="id">Mã GUID của Khía cạnh cần xóa.</param>
    /// <response code="204">Xóa thành công, không trả về body.</response>
    /// <response code="404">Không tìm thấy Khía cạnh cần xóa.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await _taxonomyService.DeleteAspectAsync(id);
        return NoContent();
    }

    #region Nested Sub-Resource: Categories of Aspect

    /// <summary>
    /// Lấy danh sách các Chuyên mục (Categories) thuộc về một Khía cạnh cụ thể (Nested Resource).
    /// </summary>
    /// <param name="aspectId">Mã định danh của Khía cạnh cha.</param>
    /// <returns>Danh sách Chuyên mục dạng CategoryResponseDto.</returns>
    /// <response code="200">Lấy danh sách Chuyên mục thành công.</response>
    /// <response code="404">Không tìm thấy Khía cạnh cha tương ứng.</response>
    [HttpGet("{aspectId:guid}/categories")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponseDto>>>> GetCategoriesByAspectId([FromRoute] Guid aspectId)
    {
        var categories = await _taxonomyService.GetCategoriesByAspectIdAsync(aspectId);
        var dtos = categories.Select(c => c.ToResponseDto());
        return Ok(ApiResponse<IEnumerable<CategoryResponseDto>>.Ok(dtos, $"Lấy danh sách Chuyên mục của Khía cạnh '{aspectId}' thành công."));
    }

    /// <summary>
    /// Tạo mới một Chuyên mục (Category) trực thuộc một Khía cạnh cụ thể (Nested Resource).
    /// </summary>
    /// <param name="aspectId">Mã định danh của Khía cạnh cha.</param>
    /// <param name="request">Dữ liệu tạo mới CategoryCreateDto.</param>
    /// <returns>Chuyên mục vừa tạo dạng CategoryResponseDto.</returns>
    /// <response code="201">Tạo Chuyên mục mới thành công.</response>
    /// <response code="400">Dữ liệu đầu vào vi phạm validation.</response>
    /// <response code="404">Không tìm thấy Khía cạnh cha tương ứng.</response>
    [HttpPost("{aspectId:guid}/categories")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> CreateCategoryForAspect(
        [FromRoute] Guid aspectId,
        [FromBody] CategoryCreateDto request)
    {
        var createRequest = new CreateCategoryRequest
        {
            Title = request.Title,
            Slug = request.Slug,
            Description = request.Description,
            OrderIndex = request.OrderIndex
        };

        var created = await _taxonomyService.CreateCategoryAsync(aspectId, createRequest);
        return CreatedAtAction(
            actionName: "GetById",
            controllerName: "Categories",
            routeValues: new { id = created.Id },
            value: ApiResponse<CategoryResponseDto>.Ok(created.ToResponseDto(), "Tạo Chuyên mục mới trực thuộc Khía cạnh thành công."));
    }

    #endregion
}

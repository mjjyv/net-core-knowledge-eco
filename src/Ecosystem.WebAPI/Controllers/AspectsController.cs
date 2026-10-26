using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Models;
using Ecosystem.Domain.Common;
using Ecosystem.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý RESTful endpoints cho Khía cạnh chuyên sâu cấp 2 (Aspects) và các Chuyên mục lồng ghép trực thuộc.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AspectsController : ControllerBase
{
    private readonly ITaxonomyService _taxonomyService;

    public AspectsController(ITaxonomyService taxonomyService)
    {
        _taxonomyService = taxonomyService;
    }

    /// <summary>
    /// Lấy chi tiết một Khía cạnh theo định danh GUID.
    /// GET /api/v1/aspects/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<Aspect>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Aspect>>> GetById([FromRoute] Guid id)
    {
        var aspect = await _taxonomyService.GetAspectByIdAsync(id);
        return Ok(ApiResponse<Aspect>.Ok(aspect, $"Lấy Khía cạnh với Id '{id}' thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin một Khía cạnh (Idempotent).
    /// PUT /api/v1/aspects/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateAspectRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<object>.Fail("ID trên URL route không khớp với ID trong request body."));
        }

        await _taxonomyService.UpdateAspectAsync(id, request);
        return NoContent();
    }

    /// <summary>
    /// Xóa mềm một Khía cạnh (Idempotent).
    /// DELETE /api/v1/aspects/{id}
    /// </summary>
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
    /// GET /api/v1/aspects/{aspectId}/categories
    /// </summary>
    [HttpGet("{aspectId:guid}/categories")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Category>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Category>>>> GetCategoriesByAspectId([FromRoute] Guid aspectId)
    {
        var categories = await _taxonomyService.GetCategoriesByAspectIdAsync(aspectId);
        return Ok(ApiResponse<IEnumerable<Category>>.Ok(categories, $"Lấy danh sách Chuyên mục của Khía cạnh '{aspectId}' thành công."));
    }

    /// <summary>
    /// Tạo mới một Chuyên mục (Category) trực thuộc một Khía cạnh cụ thể (Nested Resource).
    /// POST /api/v1/aspects/{aspectId}/categories
    /// </summary>
    [HttpPost("{aspectId:guid}/categories")]
    [ProducesResponseType(typeof(ApiResponse<Category>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Category>>> CreateCategoryForAspect(
        [FromRoute] Guid aspectId,
        [FromBody] CreateCategoryRequest request)
    {
        var created = await _taxonomyService.CreateCategoryAsync(aspectId, request);
        return CreatedAtAction(
            actionName: "GetById",
            controllerName: "Categories",
            routeValues: new { id = created.Id },
            value: ApiResponse<Category>.Ok(created, "Tạo Chuyên mục mới trực thuộc Khía cạnh thành công."));
    }

    #endregion
}

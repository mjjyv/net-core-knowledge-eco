using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Models;
using Ecosystem.Domain.Common;
using Ecosystem.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý RESTful endpoints cho Chuyên mục cụ thể cấp 3 (Categories).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ITaxonomyService _taxonomyService;

    public CategoriesController(ITaxonomyService taxonomyService)
    {
        _taxonomyService = taxonomyService;
    }

    /// <summary>
    /// Lấy chi tiết một Chuyên mục theo định danh GUID.
    /// GET /api/v1/categories/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<Category>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Category>>> GetById([FromRoute] Guid id)
    {
        var category = await _taxonomyService.GetCategoryByIdAsync(id);
        return Ok(ApiResponse<Category>.Ok(category, $"Lấy Chuyên mục với Id '{id}' thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin một Chuyên mục (Idempotent).
    /// PUT /api/v1/categories/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<object>.Fail("ID trên URL route không khớp với ID trong request body."));
        }

        await _taxonomyService.UpdateCategoryAsync(id, request);
        return NoContent();
    }

    /// <summary>
    /// Xóa mềm một Chuyên mục (Idempotent).
    /// DELETE /api/v1/categories/{id}
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await _taxonomyService.DeleteCategoryAsync(id);
        return NoContent();
    }
}

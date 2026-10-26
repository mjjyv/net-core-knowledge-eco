using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Models;
using Ecosystem.Domain.Common;
using Ecosystem.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý RESTful endpoints cho Chủ đề lớn cấp 1 (Topics) và các Khía cạnh lồng ghép trực thuộc.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly ITaxonomyService _taxonomyService;

    public TopicsController(ITaxonomyService taxonomyService)
    {
        _taxonomyService = taxonomyService;
    }

    /// <summary>
    /// Lấy danh sách các Chủ đề kiến thức, hỗ trợ tìm kiếm và sắp xếp qua Query Parameters.
    /// GET /api/v1/topics?searchTerm=dotnet&sortBy=title&isDescending=false
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Topic>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Topic>>>> GetAll([FromQuery] TaxonomyFilterQuery query)
    {
        var topics = await _taxonomyService.GetAllTopicsAsync(query);
        return Ok(ApiResponse<IEnumerable<Topic>>.Ok(topics, "Lấy danh sách Chủ đề thành công."));
    }

    /// <summary>
    /// Lấy chi tiết một Chủ đề kiến thức theo định danh GUID.
    /// GET /api/v1/topics/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<Topic>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Topic>>> GetById([FromRoute] Guid id)
    {
        var topic = await _taxonomyService.GetTopicByIdAsync(id);
        return Ok(ApiResponse<Topic>.Ok(topic, $"Lấy Chủ đề với Id '{id}' thành công."));
    }

    /// <summary>
    /// Lấy chi tiết một Chủ đề kiến thức theo Slug chuẩn SEO.
    /// GET /api/v1/topics/slug/{slug}
    /// </summary>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ApiResponse<Topic>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Topic>>> GetBySlug([FromRoute] string slug)
    {
        var topic = await _taxonomyService.GetTopicBySlugAsync(slug);
        return Ok(ApiResponse<Topic>.Ok(topic, $"Lấy Chủ đề với Slug '{slug}' thành công."));
    }

    /// <summary>
    /// Tạo mới một Chủ đề kiến thức.
    /// POST /api/v1/topics
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Topic>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Topic>>> Create([FromBody] CreateTopicRequest request)
    {
        var created = await _taxonomyService.CreateTopicAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ApiResponse<Topic>.Ok(created, "Tạo mới Chủ đề thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin một Chủ đề kiến thức (Idempotent).
    /// PUT /api/v1/topics/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTopicRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<object>.Fail("ID trên URL route không khớp với ID trong request body."));
        }

        await _taxonomyService.UpdateTopicAsync(id, request);
        return NoContent();
    }

    /// <summary>
    /// Xóa mềm một Chủ đề kiến thức (Idempotent).
    /// DELETE /api/v1/topics/{id}
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await _taxonomyService.DeleteTopicAsync(id);
        return NoContent();
    }

    #region Nested Sub-Resource: Aspects of Topic

    /// <summary>
    /// Lấy danh sách các Khía cạnh (Aspects) thuộc về một Chủ đề cụ thể (Nested Resource).
    /// GET /api/v1/topics/{topicId}/aspects
    /// </summary>
    [HttpGet("{topicId:guid}/aspects")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Aspect>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Aspect>>>> GetAspectsByTopicId([FromRoute] Guid topicId)
    {
        var aspects = await _taxonomyService.GetAspectsByTopicIdAsync(topicId);
        return Ok(ApiResponse<IEnumerable<Aspect>>.Ok(aspects, $"Lấy danh sách Khía cạnh của Chủ đề '{topicId}' thành công."));
    }

    /// <summary>
    /// Tạo mới một Khía cạnh (Aspect) trực thuộc một Chủ đề cụ thể (Nested Resource).
    /// POST /api/v1/topics/{topicId}/aspects
    /// </summary>
    [HttpPost("{topicId:guid}/aspects")]
    [ProducesResponseType(typeof(ApiResponse<Aspect>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Aspect>>> CreateAspectForTopic(
        [FromRoute] Guid topicId,
        [FromBody] CreateAspectRequest request)
    {
        var created = await _taxonomyService.CreateAspectAsync(topicId, request);
        return CreatedAtAction(
            actionName: "GetById",
            controllerName: "Aspects",
            routeValues: new { id = created.Id },
            value: ApiResponse<Aspect>.Ok(created, "Tạo Khía cạnh mới trực thuộc Chủ đề thành công."));
    }

    #endregion
}

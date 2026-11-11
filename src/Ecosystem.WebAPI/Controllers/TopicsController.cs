using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Mappers;
using Ecosystem.Application.Models;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý RESTful endpoints cho Chủ đề lớn cấp 1 (Topics) và các Khía cạnh lồng ghép trực thuộc.
/// Áp dụng DTO Response chuẩn, Data Annotations Validation và triệt tiêu Circular References.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class TopicsController : ControllerBase
{
    private readonly ITaxonomyService _taxonomyService;

    public TopicsController(ITaxonomyService taxonomyService)
    {
        _taxonomyService = taxonomyService;
    }

    /// <summary>
    /// Lấy danh sách các Chủ đề kiến thức, hỗ trợ tìm kiếm và sắp xếp qua Query Parameters.
    /// </summary>
    /// <param name="query">Tham số lọc (SearchTerm, SortBy, IsDescending).</param>
    /// <returns>Danh sách thông tin các Chủ đề dạng TopicResponseDto.</returns>
    /// <response code="200">Lấy danh sách Chủ đề thành công.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TopicResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TopicResponseDto>>>> GetAll([FromQuery] TaxonomyFilterQuery query)
    {
        var topics = await _taxonomyService.GetAllTopicsAsync(query);
        var dtos = topics.Select(t => t.ToResponseDto());
        return Ok(ApiResponse<IEnumerable<TopicResponseDto>>.Ok(dtos, "Lấy danh sách Chủ đề thành công."));
    }

    /// <summary>
    /// Lấy chi tiết một Chủ đề kiến thức kèm danh sách Khía cạnh (Aspects) theo định danh GUID.
    /// </summary>
    /// <param name="id">Mã định danh GUID của Chủ đề.</param>
    /// <returns>Chi tiết Chủ đề dạng TopicDetailResponseDto.</returns>
    /// <response code="200">Tìm thấy Chủ đề thành công.</response>
    /// <response code="404">Không tìm thấy Chủ đề với Id tương ứng.</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TopicDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TopicDetailResponseDto>>> GetById([FromRoute] Guid id)
    {
        var topic = await _taxonomyService.GetTopicByIdAsync(id);
        return Ok(ApiResponse<TopicDetailResponseDto>.Ok(topic.ToDetailResponseDto(), $"Lấy Chủ đề với Id '{id}' thành công."));
    }

    /// <summary>
    /// Lấy chi tiết một Chủ đề kiến thức theo Slug chuẩn SEO.
    /// </summary>
    /// <param name="slug">Chuỗi định danh SEO kebab-case của Chủ đề.</param>
    /// <returns>Thông tin Chủ đề dạng TopicResponseDto.</returns>
    /// <response code="200">Tìm thấy Chủ đề theo Slug.</response>
    /// <response code="404">Không tìm thấy Chủ đề với Slug yêu cầu.</response>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TopicResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TopicResponseDto>>> GetBySlug([FromRoute] string slug)
    {
        var topic = await _taxonomyService.GetTopicBySlugAsync(slug);
        return Ok(ApiResponse<TopicResponseDto>.Ok(topic.ToResponseDto(), $"Lấy Chủ đề với Slug '{slug}' thành công."));
    }

    /// <summary>
    /// Tạo mới một Chủ đề kiến thức với xác thực DTO chặt chẽ (phòng chống Over-posting).
    /// </summary>
    /// <param name="request">Dữ liệu tạo mới (Title, Slug, Description, OrderIndex).</param>
    /// <returns>Chủ đề vừa tạo dạng TopicResponseDto.</returns>
    /// <response code="201">Tạo mới Chủ đề thành công.</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ (vi phạm độ dài, định dạng slug, v.v.).</response>
    [HttpPost]
    [Authorize(Policy = AuthConstants.Policies.RequireHost)]
    [ProducesResponseType(typeof(ApiResponse<TopicResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TopicResponseDto>>> Create([FromBody] TopicCreateDto request)
    {
        var createRequest = new CreateTopicRequest
        {
            Title = request.Title,
            Slug = request.Slug,
            Description = request.Description,
            OrderIndex = request.OrderIndex
        };

        var created = await _taxonomyService.CreateTopicAsync(createRequest);
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ApiResponse<TopicResponseDto>.Ok(created.ToResponseDto(), "Tạo mới Chủ đề thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin một Chủ đề kiến thức (Idempotent).
    /// </summary>
    /// <param name="id">Mã GUID trên URL route.</param>
    /// <param name="request">Dữ liệu cập nhật TopicUpdateDto.</param>
    /// <response code="204">Cập nhật thành công, không trả về body.</response>
    /// <response code="400">ID trên route không khớp ID trong request body hoặc dữ liệu vi phạm validation.</response>
    /// <response code="404">Không tìm thấy Chủ đề cần cập nhật.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthConstants.Policies.RequireHost)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] TopicUpdateDto request)
    {
        if (id != request.Id)
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "Bất đồng bộ định danh",
                Status = StatusCodes.Status400BadRequest,
                Detail = "ID trên URL route không khớp với ID trong request body.",
                Instance = HttpContext?.Request?.Path ?? $"/api/v1/topics/{id}"
            });
        }

        var updateRequest = new UpdateTopicRequest
        {
            Id = request.Id,
            Title = request.Title,
            Slug = request.Slug,
            Description = request.Description,
            OrderIndex = request.OrderIndex
        };

        await _taxonomyService.UpdateTopicAsync(id, updateRequest);
        return NoContent();
    }

    /// <summary>
    /// Xóa mềm một Chủ đề kiến thức (Idempotent).
    /// </summary>
    /// <param name="id">Mã GUID của Chủ đề cần xóa.</param>
    /// <response code="204">Xóa thành công, không trả về body.</response>
    /// <response code="404">Không tìm thấy Chủ đề cần xóa.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthConstants.Policies.RequireHost)]
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
    /// </summary>
    /// <param name="topicId">Mã định danh của Chủ đề cha.</param>
    /// <returns>Danh sách Khía cạnh dạng AspectResponseDto.</returns>
    /// <response code="200">Lấy danh sách Khía cạnh thành công.</response>
    /// <response code="404">Không tìm thấy Chủ đề cha tương ứng.</response>
    [HttpGet("{topicId:guid}/aspects")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AspectResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AspectResponseDto>>>> GetAspectsByTopicId([FromRoute] Guid topicId)
    {
        var aspects = await _taxonomyService.GetAspectsByTopicIdAsync(topicId);
        var dtos = aspects.Select(a => a.ToResponseDto());
        return Ok(ApiResponse<IEnumerable<AspectResponseDto>>.Ok(dtos, $"Lấy danh sách Khía cạnh của Chủ đề '{topicId}' thành công."));
    }

    /// <summary>
    /// Tạo mới một Khía cạnh (Aspect) trực thuộc một Chủ đề cụ thể (Nested Resource).
    /// </summary>
    /// <param name="topicId">Mã định danh của Chủ đề cha.</param>
    /// <param name="request">Dữ liệu tạo mới AspectCreateDto.</param>
    /// <returns>Khía cạnh vừa tạo dạng AspectResponseDto.</returns>
    /// <response code="201">Tạo Khía cạnh mới thành công.</response>
    /// <response code="400">Dữ liệu đầu vào vi phạm validation.</response>
    /// <response code="404">Không tìm thấy Chủ đề cha tương ứng.</response>
    [HttpPost("{topicId:guid}/aspects")]
    [Authorize(Policy = AuthConstants.Policies.RequireHost)]
    [ProducesResponseType(typeof(ApiResponse<AspectResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AspectResponseDto>>> CreateAspectForTopic(
        [FromRoute] Guid topicId,
        [FromBody] AspectCreateDto request)
    {
        var createRequest = new CreateAspectRequest
        {
            Title = request.Title,
            Slug = request.Slug,
            Description = request.Description,
            OrderIndex = request.OrderIndex
        };

        var created = await _taxonomyService.CreateAspectAsync(topicId, createRequest);
        return CreatedAtAction(
            actionName: "GetById",
            controllerName: "Aspects",
            routeValues: new { id = created.Id },
            value: ApiResponse<AspectResponseDto>.Ok(created.ToResponseDto(), "Tạo Khía cạnh mới trực thuộc Chủ đề thành công."));
    }

    #endregion
}

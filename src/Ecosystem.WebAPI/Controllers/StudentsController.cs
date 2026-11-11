using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý RESTful CRUD API cho Sinh viên tuân thủ N-Tier Architecture (Buổi 7).
/// Controller siêu mỏng (Lean Controller) chỉ gọi qua IStudentService — không phụ thuộc DbContext.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    /// <summary>
    /// Lấy danh sách sinh viên, hỗ trợ tìm kiếm theo tên (?name=...).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StudentResponseDto>>>> GetAll(
        [FromQuery] string? name = null,
        CancellationToken cancellationToken = default)
    {
        var students = await _studentService.GetAllAsync(name, cancellationToken);
        return Ok(ApiResponse<IEnumerable<StudentResponseDto>>.Ok(students, "Lấy danh sách sinh viên thành công."));
    }

    /// <summary>
    /// Lấy chi tiết một sinh viên theo mã định danh GUID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentResponseDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentService.GetByIdAsync(id, cancellationToken);
        if (student == null)
        {
            return NotFound(CreateNotFoundProblem(id));
        }

        return Ok(ApiResponse<StudentResponseDto>.Ok(student, "Lấy thông tin sinh viên thành công."));
    }

    /// <summary>
    /// Thêm mới một sinh viên.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentResponseDto>>> Create(
        [FromBody] StudentCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var created = await _studentService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ApiResponse<StudentResponseDto>.Ok(created, "Thêm sinh viên mới thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin sinh viên.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] StudentUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _studentService.UpdateAsync(id, dto, cancellationToken);
        return updated ? NoContent() : NotFound(CreateNotFoundProblem(id));
    }

    /// <summary>
    /// Xóa một sinh viên.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _studentService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound(CreateNotFoundProblem(id));
    }

    private ProblemDetails CreateNotFoundProblem(Guid id) => new()
    {
        Type = "https://httpstatuses.com/404",
        Title = "Không tìm thấy sinh viên",
        Status = StatusCodes.Status404NotFound,
        Detail = $"Không tìm thấy sinh viên với Id '{id}'.",
        Instance = HttpContext?.Request?.Path ?? $"/api/v1/students/{id}"
    };
}

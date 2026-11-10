using Ecosystem.Application.DTOs;
using Ecosystem.Domain.Common;
using Ecosystem.Domain.Entities;
using Ecosystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý RESTful CRUD API cho Sinh viên trực tiếp với EF Core ApplicationDbContext (Lab 1-5 Buổi 6).
/// Áp dụng .AsNoTracking(), LINQ Filtering (.Where), DTO Projection (.Select) và Async/Await.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy danh sách sinh viên từ cơ sở dữ liệu, hỗ trợ tìm kiếm theo tên (?name=...).
    /// </summary>
    /// <param name="name">Từ khóa tìm kiếm theo tên sinh viên (không phân biệt hoa thường).</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <returns>Danh sách sinh viên dạng StudentResponseDto.</returns>
    /// <response code="200">Lấy danh sách sinh viên thành công.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StudentResponseDto>>>> GetAll(
        [FromQuery] string? name = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Students.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term));
        }

        var students = await query
            .OrderBy(s => s.Name)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Major = s.Major,
                GPA = s.GPA,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<IEnumerable<StudentResponseDto>>.Ok(students, "Lấy danh sách sinh viên thành công."));
    }

    /// <summary>
    /// Lấy chi tiết một sinh viên theo mã định danh GUID.
    /// </summary>
    /// <param name="id">Mã định danh GUID của sinh viên.</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <returns>Thông tin sinh viên dạng StudentResponseDto.</returns>
    /// <response code="200">Tìm thấy sinh viên.</response>
    /// <response code="404">Không tìm thấy sinh viên với Id tương ứng.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentResponseDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var student = await _context.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Major = s.Major,
                GPA = s.GPA,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (student == null)
        {
            return NotFound(new ProblemDetails
            {
                Type = "https://httpstatuses.com/404",
                Title = "Không tìm thấy sinh viên",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Không tìm thấy sinh viên với Id '{id}'.",
                Instance = HttpContext?.Request?.Path ?? $"/api/v1/students/{id}"
            });
        }

        return Ok(ApiResponse<StudentResponseDto>.Ok(student, "Lấy thông tin sinh viên thành công."));
    }

    /// <summary>
    /// Thêm mới một sinh viên vào cơ sở dữ liệu.
    /// </summary>
    /// <param name="dto">Thông tin sinh viên cần tạo.</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <returns>Thông tin sinh viên vừa tạo kèm Location Header.</returns>
    /// <response code="201">Thêm sinh viên mới thành công.</response>
    /// <response code="400">Dữ liệu đầu vào vi phạm Validation.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentResponseDto>>> Create(
        [FromBody] StudentCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var student = new Student(dto.Name.Trim(), dto.Major.Trim(), dto.GPA)
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Students.AddAsync(student, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new StudentResponseDto
        {
            Id = student.Id,
            Name = student.Name,
            Major = student.Major,
            GPA = student.GPA,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            ApiResponse<StudentResponseDto>.Ok(response, "Thêm sinh viên mới thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin sinh viên thông qua cơ chế Change Tracking của EF Core.
    /// </summary>
    /// <param name="id">Mã GUID của sinh viên cần cập nhật.</param>
    /// <param name="dto">Thông tin cập nhật.</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <response code="204">Cập nhật sinh viên thành công.</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
    /// <response code="404">Không tìm thấy sinh viên cần cập nhật.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] StudentUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (student == null)
        {
            return NotFound(new ProblemDetails
            {
                Type = "https://httpstatuses.com/404",
                Title = "Không tìm thấy sinh viên",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Không tìm thấy sinh viên với Id '{id}' để cập nhật.",
                Instance = HttpContext?.Request?.Path ?? $"/api/v1/students/{id}"
            });
        }

        student.Name = dto.Name.Trim();
        student.Major = dto.Major.Trim();
        student.GPA = dto.GPA;
        student.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Xóa một sinh viên khỏi cơ sở dữ liệu.
    /// </summary>
    /// <param name="id">Mã GUID của sinh viên cần xóa.</param>
    /// <param name="cancellationToken">Token hủy bất đồng bộ.</param>
    /// <response code="204">Xóa sinh viên thành công.</response>
    /// <response code="404">Không tìm thấy sinh viên cần xóa.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (student == null)
        {
            return NotFound(new ProblemDetails
            {
                Type = "https://httpstatuses.com/404",
                Title = "Không tìm thấy sinh viên",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Không tìm thấy sinh viên với Id '{id}' để xóa.",
                Instance = HttpContext?.Request?.Path ?? $"/api/v1/students/{id}"
            });
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

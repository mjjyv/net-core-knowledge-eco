using System.Diagnostics;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Common;
using Ecosystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Controller chẩn đoán hệ thống: kiểm chứng DI Lifetimes, Middleware bắt lỗi, Custom Model Validation và Kết nối Database (Buổi 2, 4, 5).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class DiagnosticsController : ControllerBase
{
    private readonly ITransientService _transient1;
    private readonly ITransientService _transient2;
    private readonly IScopedService _scoped1;
    private readonly IScopedService _scoped2;
    private readonly ISingletonService _singleton1;
    private readonly ISingletonService _singleton2;
    private readonly ApplicationDbContext? _dbContext;

    public DiagnosticsController(
        ITransientService transient1,
        ITransientService transient2,
        IScopedService scoped1,
        IScopedService scoped2,
        ISingletonService singleton1,
        ISingletonService singleton2,
        ApplicationDbContext? dbContext = null)
    {
        _transient1 = transient1;
        _transient2 = transient2;
        _scoped1 = scoped1;
        _scoped2 = scoped2;
        _singleton1 = singleton1;
        _singleton2 = singleton2;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Minh chứng sự khác biệt của 3 vòng đời Native DI (Transient, Scoped, Singleton) trong cùng một HTTP Request.
    /// </summary>
    [HttpGet("lifetimes")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetLifetimes()
    {
        var result = new
        {
            Transient = new
            {
                Description = "Instance mới được tạo ra cho mỗi lần inject.",
                Instance1 = _transient1.InstanceId,
                Instance2 = _transient2.InstanceId,
                IsEqual = _transient1.InstanceId == _transient2.InstanceId // Kỳ vọng: false
            },
            Scoped = new
            {
                Description = "Cùng một instance được dùng chung trong phạm vi một HTTP request.",
                Instance1 = _scoped1.InstanceId,
                Instance2 = _scoped2.InstanceId,
                IsEqual = _scoped1.InstanceId == _scoped2.InstanceId // Kỳ vọng: true
            },
            Singleton = new
            {
                Description = "Một instance duy nhất được dùng chung vĩnh viễn trong toàn bộ ứng dụng.",
                Instance1 = _singleton1.InstanceId,
                Instance2 = _singleton2.InstanceId,
                IsEqual = _singleton1.InstanceId == _singleton2.InstanceId // Kỳ vọng: true
            }
        };

        return Ok(ApiResponse<object>.Ok(result, "Kiểm chứng vòng đời Native Dependency Injection thành công."));
    }

    /// <summary>
    /// Endpoint cố tình phát sinh ngoại lệ để kiểm chứng GlobalExceptionMiddleware chuẩn RFC 7807.
    /// </summary>
    [HttpGet("error-demo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult ThrowErrorDemo([FromQuery] string? type)
    {
        return type?.ToLowerInvariant() switch
        {
            "notfound" => throw new KeyNotFoundException("Không tìm thấy tài nguyên mẫu theo yêu cầu."),
            "argument" => throw new ArgumentException("Tham số truyền vào không hợp lệ."),
            "unauthorized" => throw new UnauthorizedAccessException("Bạn không có quyền thực thi tác vụ này."),
            _ => throw new InvalidOperationException("Lỗi hệ thống cố tình phát sinh để kiểm chứng Middleware.")
        };
    }

    /// <summary>
    /// Kiểm thử Custom Validation Attribute [MustBeEven] (Bài tập 4 - Buổi 4).
    /// </summary>
    /// <param name="dto">Đối tượng kiểm thử chứa trường Value bắt buộc phải là số chẵn.</param>
    /// <returns>Đối tượng đã được xác thực thành công.</returns>
    /// <response code="200">Xác thực thành công, giá trị là số chẵn.</response>
    /// <response code="400">Dữ liệu không hợp lệ (giá trị là số lẻ hoặc thiếu thuộc tính).</response>
    [HttpPost("validate-even")]
    [ProducesResponseType(typeof(ApiResponse<EvenNumberDemoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult ValidateEvenNumber([FromBody] EvenNumberDemoDto dto)
    {
        return Ok(ApiResponse<EvenNumberDemoDto>.Ok(dto, $"Giá trị {dto.Value} là số chẵn hợp lệ."));
    }

    /// <summary>
    /// Kiểm tra trạng thái kết nối cơ sở dữ liệu và đếm số lượng bản ghi các bảng chính (Phase 05).
    /// </summary>
    [HttpGet("database")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDatabaseStatusAsync(CancellationToken cancellationToken = default)
    {
        if (_dbContext == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object>.Fail("ApplicationDbContext chưa được cấu hình."));
        }

        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object>.Fail("Không thể kết nối tới cơ sở dữ liệu."));
        }

        var provider = _dbContext.Database.ProviderName;
        var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);

        var counts = new
        {
            Topics = await _dbContext.Topics.CountAsync(cancellationToken),
            Aspects = await _dbContext.Aspects.CountAsync(cancellationToken),
            Categories = await _dbContext.Categories.CountAsync(cancellationToken),
            Articles = await _dbContext.Articles.CountAsync(cancellationToken),
            Users = await _dbContext.Users.CountAsync(cancellationToken),
            Students = await _dbContext.Students.CountAsync(cancellationToken),
            ClassroomSessions = await _dbContext.ClassroomSessions.CountAsync(cancellationToken)
        };

        var result = new
        {
            CanConnect = canConnect,
            Provider = provider,
            AppliedMigrations = appliedMigrations,
            RecordCounts = counts
        };

        return Ok(ApiResponse<object>.Ok(result, "Kết nối cơ sở dữ liệu hoạt động bình thường."));
    }
}

using System.Diagnostics;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly ITransientService _transient1;
    private readonly ITransientService _transient2;
    private readonly IScopedService _scoped1;
    private readonly IScopedService _scoped2;
    private readonly ISingletonService _singleton1;
    private readonly ISingletonService _singleton2;

    public DiagnosticsController(
        ITransientService transient1,
        ITransientService transient2,
        IScopedService scoped1,
        IScopedService scoped2,
        ISingletonService singleton1,
        ISingletonService singleton2)
    {
        _transient1 = transient1;
        _transient2 = transient2;
        _scoped1 = scoped1;
        _scoped2 = scoped2;
        _singleton1 = singleton1;
        _singleton2 = singleton2;
    }

    /// <summary>
    /// Minh chứng sự khác biệt của 3 vòng đời Native DI (Transient, Scoped, Singleton) trong cùng một HTTP Request.
    /// </summary>
    [HttpGet("lifetimes")]
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
}

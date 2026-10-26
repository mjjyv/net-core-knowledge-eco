using System.Net;
using System.Text.Json;
using Ecosystem.Domain.Exceptions;

namespace Ecosystem.WebAPI.Middleware;

/// <summary>
/// Middleware xử lý ngoại lệ tập trung toàn cục (RFC 7807 ProblemDetails).
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Đã xảy ra ngoại lệ chưa được xử lý tại đường dẫn: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            EntityNotFoundException or KeyNotFoundException => (HttpStatusCode.NotFound, "Tài nguyên không tìm thấy"),
            ArgumentException or DomainException => (HttpStatusCode.BadRequest, "Yêu cầu không hợp lệ"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Không có quyền truy cập"),
            _ => (HttpStatusCode.InternalServerError, "Lỗi máy chủ nội bộ")
        };

        context.Response.ContentType = "application/problem+json; charset=utf-8";
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title = title,
            status = (int)statusCode,
            detail = exception.Message,
            instance = context.Request.Path.Value,
            timestamp = DateTime.UtcNow
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
    }
}

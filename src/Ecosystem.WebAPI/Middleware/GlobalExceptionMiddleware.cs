using System.Net;
using System.Text.Json;
using Ecosystem.Domain.Exceptions;

namespace Ecosystem.WebAPI.Middleware;

/// <summary>
/// Middleware xử lý ngoại lệ tập trung toàn cục (RFC 7807 ProblemDetails - Buổi 12).
/// Tự động gắn RequestId để truy vết và ẩn thông tin nhạy cảm của lỗi 500 ở môi trường Production.
/// </summary>
public class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment? _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment? environment = null)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = RequestIdHelper.EnsureRequestId(context);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    ex,
                    "Response has already started before GlobalExceptionMiddleware could write ProblemDetails. RequestId={RequestId}",
                    requestId);
                throw;
            }

            await HandleExceptionAsync(context, ex, requestId);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, string requestId)
    {
        var (statusCode, title, isServerError) = exception switch
        {
            EntityNotFoundException or KeyNotFoundException => (HttpStatusCode.NotFound, "Tài nguyên không tìm thấy", false),
            ArgumentException or DomainException or FormatException => (HttpStatusCode.BadRequest, "Yêu cầu không hợp lệ", false),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Không có quyền truy cập", false),
            OperationCanceledException => ((HttpStatusCode)499, "Yêu cầu đã bị hủy bởi client", false),
            _ => (HttpStatusCode.InternalServerError, "Lỗi máy chủ nội bộ", true)
        };

        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";

        if (isServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled server exception ({StatusCode}). RequestId={RequestId}, Method={Method}, Path={Path}",
                (int)statusCode,
                requestId,
                method,
                path);
        }
        else if ((int)statusCode == 499)
        {
            _logger.LogInformation(
                "Request was canceled by client. RequestId={RequestId}, Method={Method}, Path={Path}",
                requestId,
                method,
                path);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Handled client/domain exception ({StatusCode}). RequestId={RequestId}, Method={Method}, Path={Path}",
                (int)statusCode,
                requestId,
                method,
                path);
        }

        var isDevelopment = _environment == null || _environment.IsDevelopment();
        var detail = isServerError && !isDevelopment
            ? "Đã xảy ra lỗi hệ thống nội bộ. Vui lòng thử lại sau hoặc liên hệ quản trị viên kèm mã RequestId."
            : exception.Message;

        context.Response.ContentType = "application/problem+json; charset=utf-8";
        context.Response.StatusCode = (int)statusCode;
        context.Response.Headers[RequestIdHelper.HeaderName] = requestId;

        var problemDetails = new Dictionary<string, object?>
        {
            ["type"] = $"https://httpstatuses.com/{(int)statusCode}",
            ["title"] = title,
            ["status"] = (int)statusCode,
            ["detail"] = detail,
            ["instance"] = path,
            ["traceId"] = requestId,
            ["requestId"] = requestId,
            ["timestamp"] = DateTime.UtcNow
        };

        if (isDevelopment && isServerError)
        {
            problemDetails["exceptionType"] = exception.GetType().Name;
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
    }
}

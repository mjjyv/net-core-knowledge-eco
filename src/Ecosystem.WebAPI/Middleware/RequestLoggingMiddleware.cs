namespace Ecosystem.WebAPI.Middleware;

/// <summary>
/// Middleware thiết lập Structured Logging Scope gắn RequestId/Method/Path cho toàn bộ vòng đời HTTP Request (Buổi 12).
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = RequestIdHelper.EnsureRequestId(context);
        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path.Value ?? "/";

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["RequestMethod"] = requestMethod,
            ["RequestPath"] = requestPath
        });

        _logger.LogInformation(
            "HTTP Request started: {RequestId} {Method} {Path}",
            requestId,
            requestMethod,
            requestPath);

        try
        {
            await _next(context);
        }
        finally
        {
            _logger.LogInformation(
                "HTTP Response completed: {RequestId} {Method} {Path} -> {StatusCode}",
                requestId,
                requestMethod,
                requestPath,
                context.Response.StatusCode);
        }
    }
}

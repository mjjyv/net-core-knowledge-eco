using System.Diagnostics;

namespace Ecosystem.WebAPI.Middleware;

/// <summary>
/// Middleware ghi log giám sát luồng HTTP request và đo lường thời gian đáp ứng (Latency).
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
        var stopwatch = Stopwatch.StartNew();
        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path;

        _logger.LogInformation("HTTP Request: {Method} {Path}", requestMethod, requestPath);

        context.Response.OnStarting(() =>
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            context.Response.Headers["X-Response-Time-Ms"] = elapsedMs.ToString();
            return Task.CompletedTask;
        });

        await _next(context);

        if (!stopwatch.IsRunning)
        {
            stopwatch.Stop();
        }

        _logger.LogInformation(
            "HTTP Response: {Method} {Path} -> {StatusCode} in {Elapsed}ms",
            requestMethod,
            requestPath,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}

using System.Diagnostics;
using Ecosystem.Application.Common;
using Microsoft.Extensions.Options;

namespace Ecosystem.WebAPI.Middleware;

/// <summary>
/// Middleware đo thời gian xử lý mọi HTTP request bằng Stopwatch trong khối try-finally,
/// gắn Header X-Response-Time-Ms và phát cảnh báo LogWarning khi vượt ngưỡng SlowRequestThresholdMs (Buổi 12).
/// </summary>
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;
    private readonly IOptionsMonitor<ObservabilityOptions>? _observabilityOptions;

    public RequestTimingMiddleware(
        RequestDelegate next,
        ILogger<RequestTimingMiddleware> logger,
        IOptionsMonitor<ObservabilityOptions>? observabilityOptions = null)
    {
        _next = next;
        _logger = logger;
        _observabilityOptions = observabilityOptions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = RequestIdHelper.EnsureRequestId(context);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Response-Time-Ms"] = stopwatch.ElapsedMilliseconds.ToString();
            return Task.CompletedTask;
        });

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            var thresholdMs = _observabilityOptions?.CurrentValue.SlowRequestThresholdMs ?? 500;
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? "/";
            var statusCode = context.Response.StatusCode;

            if (elapsedMs >= thresholdMs)
            {
                _logger.LogWarning(
                    "Slow HTTP Request detected: {RequestId} {Method} {Path} -> {StatusCode} took {ElapsedMs}ms (Threshold: {ThresholdMs}ms)",
                    requestId,
                    method,
                    path,
                    statusCode,
                    elapsedMs,
                    thresholdMs);
            }
            else
            {
                _logger.LogInformation(
                    "Request timing: {RequestId} {Method} {Path} -> {StatusCode} in {ElapsedMs}ms",
                    requestId,
                    method,
                    path,
                    statusCode,
                    elapsedMs);
            }
        }
    }
}

namespace Ecosystem.WebAPI.Middleware;

/// <summary>
/// Tiện ích nội bộ quản lý Request ID / Correlation ID đồng bộ giữa HttpContext, Response Header và Structured Logging.
/// </summary>
internal static class RequestIdHelper
{
    public const string HeaderName = "X-Request-Id";

    public static string EnsureRequestId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var incoming)
            && !string.IsNullOrWhiteSpace(incoming.ToString()))
        {
            var sanitized = SanitizeHeaderValue(incoming.ToString());
            if (!string.IsNullOrEmpty(sanitized))
            {
                context.TraceIdentifier = sanitized;
            }
        }

        if (string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            context.TraceIdentifier = Guid.NewGuid().ToString("N");
        }

        if (!context.Response.HasStarted)
        {
            context.Response.Headers[HeaderName] = context.TraceIdentifier;
        }

        return context.TraceIdentifier;
    }

    private static string SanitizeHeaderValue(string rawValue)
    {
        // Loại bỏ CR/LF/Tab để chống lỗ hổng Log Forging và giới hạn độ dài an toàn
        var cleaned = rawValue
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal)
            .Replace("\t", string.Empty, StringComparison.Ordinal)
            .Trim();

        return cleaned.Length > 64 ? cleaned[..64] : cleaned;
    }
}

using Ecosystem.WebAPI.Middleware;

namespace Ecosystem.WebAPI.Extensions;

/// <summary>
/// Các phương thức mở rộng cấu hình Middleware Pipeline của ứng dụng.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Kích hoạt các Middleware tùy biến theo đúng thứ tự: Bắt lỗi tập trung -&gt; Structured Request Logging -&gt; Đo thời gian &amp; cảnh báo chậm.
    /// </summary>
    public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
    {
        // 1. Bắt ngoại lệ tập trung (ở đầu pipeline để bảo vệ mọi middleware tiếp theo)
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // 2. Thiết lập RequestId Scope và ghi Structured Log cho Request/Response
        app.UseMiddleware<RequestLoggingMiddleware>();

        // 3. Đo thời gian đáp ứng (try-finally) và cảnh báo LogWarning khi vượt ngưỡng
        app.UseMiddleware<RequestTimingMiddleware>();

        return app;
    }

    /// <summary>
    /// Kích hoạt giao diện tài liệu Swagger và Swagger UI nâng cao (persist authorization, request duration, search filter).
    /// </summary>
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "The Ecosystem Web API v1");
            c.RoutePrefix = "swagger";
            c.DisplayRequestDuration();
            c.EnablePersistAuthorization();
            c.EnableFilter();
        });

        return app;
    }
}

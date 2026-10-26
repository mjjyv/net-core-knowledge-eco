using Ecosystem.WebAPI.Middleware;

namespace Ecosystem.WebAPI.Extensions;

/// <summary>
/// Các phương thức mở rộng cấu hình Middleware Pipeline của ứng dụng.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Kích hoạt các Middleware tùy biến theo đúng thứ tự: Bắt lỗi tập trung -> Ghi log thời gian.
    /// </summary>
    public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
    {
        // 1. Bắt ngoại lệ tập trung (ở đầu pipeline để bảo vệ mọi middleware tiếp theo)
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // 2. Ghi log request và đo lường thời gian đáp ứng
        app.UseMiddleware<RequestLoggingMiddleware>();

        return app;
    }

    /// <summary>
    /// Kích hoạt giao diện tài liệu Swagger và Swagger UI.
    /// </summary>
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "The Ecosystem Web API v1");
            c.RoutePrefix = "swagger";
        });

        return app;
    }
}

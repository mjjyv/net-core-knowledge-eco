using Ecosystem.Application.Common;
using Ecosystem.Domain.Common;
using Ecosystem.Infrastructure.Data;
using Ecosystem.Infrastructure.Data.Seed;
using Ecosystem.Infrastructure.Extensions;
using Ecosystem.WebAPI.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký Services & Options Pattern vào DI Container
builder.Services.AddControllers();
builder.Services.AddConfigurationAndOptions(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddApplicationServices();
builder.Services.AddLifetimeDemonstrationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

var features = app.Services.GetRequiredService<IOptions<EcosystemFeaturesOptions>>().Value;

// 2. Cấu hình HTTP Request Pipeline (Middleware Architecture)
app.UseCustomMiddlewares();

// Đảm bảo thư mục wwwroot tồn tại và kích hoạt phục vụ tệp tĩnh (Buổi 10)
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "wwwroot"));
app.UseStaticFiles();

if (app.Environment.IsDevelopment() && features.EnableSwagger)
{
    app.UseSwaggerDocumentation();
}

if (app.Environment.IsDevelopment() || features.EnableAutoMigration)
{
    // Tự động áp dụng Migrations và Seed Data ban đầu (Bài tập 4 & 5 - Buổi 5)
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
        await DatabaseSeeder.SeedInitialDataAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi tự động chạy Migration hoặc Seed Data.");
    }
}

app.UseHttpsRedirection();

// Thứ tự Middleware bắt buộc: Authentication phải đứng trước Authorization (Buổi 8)
app.UseAuthentication();
app.UseAuthorization();

// 3. Map Endpoints & Controllers
app.MapControllers();

// Endpoint kiểm tra sức khỏe hệ thống (Health Check)
app.MapGet("/health", () => Results.Ok(ApiResponse<object>.Ok(new
{
    status = "Healthy",
    service = "TheEcosystem.WebAPI",
    phase = "Phase 10: File Upload, Configuration & Options Pattern",
    environment = app.Environment.EnvironmentName,
    serverTimeUtc = DateTime.UtcNow
}, "Hệ thống đang hoạt động bình thường.")))
.AllowAnonymous();

app.Run();

/// <summary>
/// Partial class công khai phục vụ WebApplicationFactory&lt;Program&gt; trong Integration Testing.
/// </summary>
public partial class Program
{
}

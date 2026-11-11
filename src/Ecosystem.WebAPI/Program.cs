using Ecosystem.Domain.Common;
using Ecosystem.Infrastructure.Data;
using Ecosystem.Infrastructure.Data.Seed;
using Ecosystem.Infrastructure.Extensions;
using Ecosystem.WebAPI.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký Services vào DI Container
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddApplicationServices();
builder.Services.AddLifetimeDemonstrationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// 2. Cấu hình HTTP Request Pipeline (Middleware Architecture)
app.UseCustomMiddlewares();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();

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

// 3. Map Endpoints & Controllers
app.MapControllers();

// Endpoint kiểm tra sức khỏe hệ thống (Health Check)
app.MapGet("/health", () => Results.Ok(ApiResponse<object>.Ok(new
{
    status = "Healthy",
    service = "TheEcosystem.WebAPI",
    phase = "Phase 07: N-Tier Architecture, Repository & Unit of Work",
    environment = app.Environment.EnvironmentName,
    serverTimeUtc = DateTime.UtcNow
}, "Hệ thống đang hoạt động bình thường.")));

app.Run();

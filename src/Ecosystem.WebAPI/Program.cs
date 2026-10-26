using Ecosystem.Domain.Common;
using Ecosystem.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký Services vào DI Container
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddApplicationServices();
builder.Services.AddLifetimeDemonstrationServices();

var app = builder.Build();

// 2. Cấu hình HTTP Request Pipeline (Middleware Architecture)
app.UseCustomMiddlewares();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseHttpsRedirection();

// 3. Map Endpoints & Controllers
app.MapControllers();

// Endpoint kiểm tra sức khỏe hệ thống (Health Check)
app.MapGet("/health", () => Results.Ok(ApiResponse<object>.Ok(new
{
    status = "Healthy",
    service = "TheEcosystem.WebAPI",
    phase = "Phase 03: Controllers & RESTful Routing (Taxonomy CRUD)",
    environment = app.Environment.EnvironmentName,
    serverTimeUtc = DateTime.UtcNow
}, "Hệ thống đang hoạt động bình thường.")));

app.Run();

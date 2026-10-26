using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Services;
using Ecosystem.Domain.Common;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ vào DI Container (Phase 01 & Buổi 1)
builder.Services.AddSingleton<IStudentManager, StudentManager>();

var app = builder.Build();

app.UseHttpsRedirection();

// Endpoint kiểm tra sức khỏe hệ thống (Health Check)
app.MapGet("/health", () => Results.Ok(ApiResponse<object>.Ok(new
{
    status = "Healthy",
    service = "TheEcosystem.WebAPI",
    phase = "Phase 01: Solution Setup & Domain Entities",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow
}, "Hệ thống đang hoạt động bình thường.")));

// Endpoint minh họa bài lab Buổi 1 qua HTTP
app.MapGet("/api/v1/students", async (IStudentManager studentManager) =>
{
    var students = await studentManager.GetAllAsync();
    return Results.Ok(ApiResponse<object>.Ok(students, "Lấy danh sách sinh viên thành công."));
});

app.MapGet("/api/v1/students/top/{count:int}", async (int count, IStudentManager studentManager) =>
{
    var topStudents = await studentManager.GetTopStudentsAsync(count);
    return Results.Ok(ApiResponse<object>.Ok(topStudents, $"Lấy top {count} sinh viên GPA cao nhất thành công."));
});

app.Run();

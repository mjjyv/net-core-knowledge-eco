using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.Infrastructure.Data.Seed;

/// <summary>
/// Dịch vụ khởi tạo dữ liệu mẫu ban đầu cho Cơ sở Dữ liệu (Bài tập 4 &amp; 5 - Buổi 5, Buổi 8).
/// </summary>
public static class DatabaseSeeder
{
    public static Task SeedAsync(ApplicationDbContext context) => SeedInitialDataAsync(context);

    public static async Task SeedInitialDataAsync(ApplicationDbContext context)
    {
        // 1. Seed Tài khoản mẫu chuẩn BCrypt (Host Admin, Presenter Giảng viên &amp; Audience User)
        if (!await context.Users.IgnoreQueryFilters().AnyAsync())
        {
            var adminUser = new User
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Username = "admin_demo",
                Email = "admin@ecosystem.vn",
                FullName = "Hệ Thống Quản Trị Viên",
                PasswordHash = HashPassword("Admin@123456"),
                Role = UserRole.Host,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var presenterUser = new User
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Username = "presenter_demo",
                Email = "presenter@ecosystem.vn",
                FullName = "Giảng Viên Trần Minh",
                PasswordHash = HashPassword("Presenter@123456"),
                Role = UserRole.Presenter,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var standardUser = new User
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Username = "user_demo",
                Email = "user@ecosystem.vn",
                FullName = "Thành Viên Cộng Đồng",
                PasswordHash = HashPassword("User@123456"),
                Role = UserRole.Audience,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(adminUser, presenterUser, standardUser);
        }

        // 2. Seed Cây Tri Thức (Taxonomy Hierarchy 4 cấp)
        if (!await context.Topics.IgnoreQueryFilters().AnyAsync())
        {
            var topic1 = new Topic
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "Lập Trình .NET & ASP.NET Core",
                Slug = "lap-trinh-dotnet-aspnet-core",
                Description = "Nền tảng C# hiện đại, Clean Architecture và RESTful Web API.",
                OrderIndex = 1,
                CreatedAt = DateTime.UtcNow
            };

            var topic2 = new Topic
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Title = "Kiến Trúc Hệ Thống Phân Tán",
                Slug = "kien-truc-he-thong-phan-tan",
                Description = "Thiết kế Microservices, Message Queues và Cache hiệu năng cao.",
                OrderIndex = 2,
                CreatedAt = DateTime.UtcNow
            };

            var aspect1 = new Aspect
            {
                Id = Guid.Parse("11111111-aaaa-1111-1111-111111111111"),
                TopicId = topic1.Id,
                Title = "Kiến Trúc Backend C#",
                Slug = "kien-truc-backend-csharp",
                Description = "Triển khai Clean Architecture, Dependency Injection và xử lý bất đồng bộ.",
                OrderIndex = 1,
                CreatedAt = DateTime.UtcNow
            };

            var aspect2 = new Aspect
            {
                Id = Guid.Parse("22222222-aaaa-2222-2222-222222222222"),
                TopicId = topic2.Id,
                Title = "Cộng Tác Thời Gian Thực SignalR",
                Slug = "cong-tac-thoi-gian-thuc-signalr",
                Description = "Đồng bộ con trỏ chuột và lớp học tương tác thời gian thực.",
                OrderIndex = 1,
                CreatedAt = DateTime.UtcNow
            };

            var category1 = new Category
            {
                Id = Guid.Parse("11111111-aaaa-c111-1111-111111111111"),
                AspectId = aspect1.Id,
                Title = "Clean Architecture & CQRS",
                Slug = "clean-architecture-cqrs",
                Description = "Phân tách Domain, Application, Infrastructure và WebAPI.",
                OrderIndex = 1,
                CreatedAt = DateTime.UtcNow
            };

            var category2 = new Category
            {
                Id = Guid.Parse("11111111-aaaa-c222-1111-111111111111"),
                AspectId = aspect1.Id,
                Title = "Entity Framework Core Advanced",
                Slug = "entity-framework-core-advanced",
                Description = "Tối ưu truy vấn LINQ, Code First Migrations và Change Tracker.",
                OrderIndex = 2,
                CreatedAt = DateTime.UtcNow
            };

            var category3 = new Category
            {
                Id = Guid.Parse("22222222-aaaa-c111-2222-222222222222"),
                AspectId = aspect2.Id,
                Title = "Presence Cursors & Classroom Hub",
                Slug = "presence-cursors-classroom-hub",
                Description = "Quản lý phòng học và giao tiếp SignalR độ trễ thấp.",
                OrderIndex = 1,
                CreatedAt = DateTime.UtcNow
            };

            var article1 = new Article
            {
                Id = Guid.Parse("11111111-aaaa-c111-a001-111111111111"),
                CategoryId = category1.Id,
                AuthorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Title = "Hướng Dẫn Thiết Kế Solution Chuẩn Clean Architecture Trong .NET 8",
                Slug = "huong-dan-thiet-ke-solution-chuan-clean-architecture-trong-net-8",
                Summary = "Bài viết tổng quan về nguyên lý Dependency Inversion và phân tách trách nhiệm giữa 4 tầng.",
                MarkdownContent = "# Kiến Trúc Clean Architecture\n\nPhân tách Domain trung tâm, Application điều phối, Infrastructure hạ tầng và WebAPI giao diện.",
                Status = ArticleStatus.Published,
                ViewCount = 128,
                ReadingTimeMinutes = 8,
                TagsJson = "[\"dotnet8\", \"clean-architecture\", \"csharp\"]",
                CreatedAt = DateTime.UtcNow
            };

            var article2 = new Article
            {
                Id = Guid.Parse("22222222-aaaa-c111-a002-222222222222"),
                CategoryId = category2.Id,
                AuthorId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Title = "Hướng Dẫn EF Core LINQ Và N-Tier Architecture Trong .NET 8",
                Slug = "huong-dan-ef-core-linq-va-n-tier-architecture",
                Summary = "Kỹ thuật tối ưu truy vấn EF Core, sử dụng AsNoTracking và phân tầng dịch vụ sạch sẽ.",
                MarkdownContent = "# EF Core & N-Tier Architecture\n\nÁp dụng Repository Pattern và Unit of Work kết hợp LINQ Projection.",
                Status = ArticleStatus.Published,
                ViewCount = 85,
                ReadingTimeMinutes = 6,
                TagsJson = "[\"ef-core\", \"linq\", \"ntier\"]",
                CreatedAt = DateTime.UtcNow
            };

            await context.Topics.AddRangeAsync(topic1, topic2);
            await context.Aspects.AddRangeAsync(aspect1, aspect2);
            await context.Categories.AddRangeAsync(category1, category2, category3);
            await context.Articles.AddRangeAsync(article1, article2);
        }

        // 3. Seed Sinh viên mẫu (Lab Buổi 1)
        if (!await context.Students.IgnoreQueryFilters().AnyAsync())
        {
            var student1 = new Student("Nguyễn Văn An", "Công nghệ thông tin", 3.8)
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333331"),
                CreatedAt = DateTime.UtcNow
            };

            var student2 = new Student("Trần Thị Bình", "Khoa học máy tính", 3.9)
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333332"),
                CreatedAt = DateTime.UtcNow
            };

            await context.Students.AddRangeAsync(student1, student2);
        }

        await context.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }
}

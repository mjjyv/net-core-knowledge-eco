using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Exceptions;

namespace Ecosystem.Application.Services;

/// <summary>
/// Dịch vụ cung cấp dữ liệu tổng quan Cây Tri Thức cho The Ecosystem, vận dụng DI và async/await.
/// </summary>
public class TaxonomyOverviewService : ITaxonomyOverviewService
{
    private readonly List<TopicOverviewDto> _topics;

    public TaxonomyOverviewService()
    {
        // Khởi tạo cấu trúc mẫu chuẩn hóa của Hệ sinh thái tri thức The Ecosystem
        _topics = new List<TopicOverviewDto>
        {
            new(
                Id: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title: "Lập trình .NET & ASP.NET Core",
                Slug: "lap-trinh-dotnet-aspnet-core",
                Description: "Toàn bộ lộ trình từ nền tảng C# đến xây dựng RESTful Web API và EF Core.",
                Aspects: new List<AspectOverviewDto>
                {
                    new(
                        Id: Guid.NewGuid(),
                        Title: "Nền tảng C# Hiện đại",
                        Slug: "nen-tang-csharp-hien-dai",
                        Categories: new List<CategoryOverviewDto>
                        {
                            new(Guid.NewGuid(), "OOP & Interface-based Design", "oop-interface-based-design", 5),
                            new(Guid.NewGuid(), "LINQ to Objects & Generic Collections", "linq-to-objects-generic-collections", 8)
                        }
                    ),
                    new(
                        Id: Guid.NewGuid(),
                        Title: "ASP.NET Core Web API",
                        Slug: "aspnet-core-web-api",
                        Categories: new List<CategoryOverviewDto>
                        {
                            new(Guid.NewGuid(), "Middleware Pipeline & Dependency Injection", "middleware-pipeline-di", 4),
                            new(Guid.NewGuid(), "RESTful Routing & Model Validation", "restful-routing-model-validation", 6)
                        }
                    )
                }
            ),
            new(
                Id: Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Title: "Kiến trúc Hệ thống & Thời Gian Thực",
                Slug: "kien-truc-he-thong-thoi-gian-thuc",
                Description: "Cộng tác thời gian thực qua SignalR, Clean Architecture và triết lý Local-First.",
                Aspects: new List<AspectOverviewDto>
                {
                    new(
                        Id: Guid.NewGuid(),
                        Title: "Cộng tác Đa người dùng qua SignalR",
                        Slug: "cong-tac-da-nguoi-dung-signalr",
                        Categories: new List<CategoryOverviewDto>
                        {
                            new(Guid.NewGuid(), "Con trỏ hiện diện & Bong bóng chat nổi", "presence-cursors-floating-chat", 3),
                            new(Guid.NewGuid(), "Điều phối Lớp học & Chế độ Spotlight", "classroom-session-spotlight-mode", 4)
                        }
                    )
                }
            )
        };
    }

    public async Task<IReadOnlyList<TopicOverviewDto>> GetTaxonomyHierarchyAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        return _topics.AsReadOnly();
    }

    public async Task<TopicOverviewDto> GetTopicBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        await Task.Delay(10, cancellationToken);

        var topic = _topics.FirstOrDefault(t => t.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
        if (topic == null)
        {
            throw new EntityNotFoundException("Topic", slug);
        }

        return topic;
    }
}

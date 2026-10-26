using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Models;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Exceptions;

namespace Ecosystem.Application.Services;

/// <summary>
/// Dịch vụ quản lý Cây Tri Thức chuẩn The Ecosystem với lưu trữ in-memory an toàn đa luồng.
/// </summary>
public class TaxonomyService : ITaxonomyService
{
    private readonly object _lock = new();
    private readonly List<Topic> _topics = new();
    private readonly List<Aspect> _aspects = new();
    private readonly List<Category> _categories = new();

    public TaxonomyService()
    {
        SeedInitialTaxonomy();
    }

    #region Topics CRUD

    public Task<IEnumerable<Topic>> GetAllTopicsAsync(TaxonomyFilterQuery? query = null)
    {
        lock (_lock)
        {
            var result = _topics.Where(t => !t.IsDeleted).AsQueryable();

            if (query != null)
            {
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var term = query.SearchTerm.Trim().ToLowerInvariant();
                    result = result.Where(t =>
                        t.Title.ToLowerInvariant().Contains(term) ||
                        t.Description.ToLowerInvariant().Contains(term) ||
                        t.Slug.ToLowerInvariant().Contains(term));
                }

                result = (query.SortBy?.ToLowerInvariant(), query.IsDescending) switch
                {
                    ("title", true) => result.OrderByDescending(t => t.Title),
                    ("title", false) => result.OrderBy(t => t.Title),
                    ("createdat", true) => result.OrderByDescending(t => t.CreatedAt),
                    ("createdat", false) => result.OrderBy(t => t.CreatedAt),
                    (_, true) => result.OrderByDescending(t => t.OrderIndex).ThenByDescending(t => t.CreatedAt),
                    _ => result.OrderBy(t => t.OrderIndex).ThenBy(t => t.CreatedAt)
                };
            }
            else
            {
                result = result.OrderBy(t => t.OrderIndex);
            }

            return Task.FromResult<IEnumerable<Topic>>(result.ToList());
        }
    }

    public Task<Topic> GetTopicByIdAsync(Guid id)
    {
        lock (_lock)
        {
            var topic = _topics.FirstOrDefault(t => t.Id == id && !t.IsDeleted);
            if (topic == null)
            {
                throw new EntityNotFoundException(nameof(Topic), id);
            }
            return Task.FromResult(topic);
        }
    }

    public Task<Topic> GetTopicBySlugAsync(string slug)
    {
        lock (_lock)
        {
            var topic = _topics.FirstOrDefault(t =>
                t.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) && !t.IsDeleted);
            if (topic == null)
            {
                throw new EntityNotFoundException(nameof(Topic), slug);
            }
            return Task.FromResult(topic);
        }
    }

    public Task<Topic> CreateTopicAsync(CreateTopicRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Tiêu đề Chủ đề (Topic) không được để trống.");
        }

        lock (_lock)
        {
            var slug = !string.IsNullOrWhiteSpace(request.Slug)
                ? GenerateSlug(request.Slug)
                : GenerateSlug(request.Title);

            if (_topics.Any(t => t.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) && !t.IsDeleted))
            {
                throw new DomainException($"Slug '{slug}' đã tồn tại trên một Chủ đề khác.");
            }

            var topic = new Topic
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Slug = slug,
                Description = request.Description?.Trim() ?? string.Empty,
                OrderIndex = request.OrderIndex,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _topics.Add(topic);
            return Task.FromResult(topic);
        }
    }

    public Task<Topic> UpdateTopicAsync(Guid id, UpdateTopicRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Tiêu đề Chủ đề (Topic) không được để trống.");
        }

        lock (_lock)
        {
            var topic = _topics.FirstOrDefault(t => t.Id == id && !t.IsDeleted);
            if (topic == null)
            {
                throw new EntityNotFoundException(nameof(Topic), id);
            }

            var slug = !string.IsNullOrWhiteSpace(request.Slug)
                ? GenerateSlug(request.Slug)
                : GenerateSlug(request.Title);

            if (_topics.Any(t => t.Id != id && t.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) && !t.IsDeleted))
            {
                throw new DomainException($"Slug '{slug}' đã tồn tại trên một Chủ đề khác.");
            }

            topic.Title = request.Title.Trim();
            topic.Slug = slug;
            topic.Description = request.Description?.Trim() ?? string.Empty;
            topic.OrderIndex = request.OrderIndex;
            topic.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult(topic);
        }
    }

    public Task DeleteTopicAsync(Guid id)
    {
        lock (_lock)
        {
            var topic = _topics.FirstOrDefault(t => t.Id == id && !t.IsDeleted);
            if (topic == null)
            {
                throw new EntityNotFoundException(nameof(Topic), id);
            }

            topic.IsDeleted = true;
            topic.UpdatedAt = DateTime.UtcNow;

            // Xóa mềm các Aspects trực thuộc
            foreach (var aspect in _aspects.Where(a => a.TopicId == id && !a.IsDeleted))
            {
                aspect.IsDeleted = true;
                aspect.UpdatedAt = DateTime.UtcNow;

                // Xóa mềm các Categories trực thuộc Aspect
                foreach (var cat in _categories.Where(c => c.AspectId == aspect.Id && !c.IsDeleted))
                {
                    cat.IsDeleted = true;
                    cat.UpdatedAt = DateTime.UtcNow;
                }
            }

            return Task.CompletedTask;
        }
    }

    #endregion

    #region Aspects CRUD

    public Task<IEnumerable<Aspect>> GetAspectsByTopicIdAsync(Guid topicId)
    {
        lock (_lock)
        {
            // Kiểm tra Topic tồn tại
            var topicExists = _topics.Any(t => t.Id == topicId && !t.IsDeleted);
            if (!topicExists)
            {
                throw new EntityNotFoundException(nameof(Topic), topicId);
            }

            var aspects = _aspects
                .Where(a => a.TopicId == topicId && !a.IsDeleted)
                .OrderBy(a => a.OrderIndex)
                .ToList();

            return Task.FromResult<IEnumerable<Aspect>>(aspects);
        }
    }

    public Task<Aspect> GetAspectByIdAsync(Guid id)
    {
        lock (_lock)
        {
            var aspect = _aspects.FirstOrDefault(a => a.Id == id && !a.IsDeleted);
            if (aspect == null)
            {
                throw new EntityNotFoundException(nameof(Aspect), id);
            }
            return Task.FromResult(aspect);
        }
    }

    public Task<Aspect> CreateAspectAsync(Guid topicId, CreateAspectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Tiêu đề Khía cạnh (Aspect) không được để trống.");
        }

        lock (_lock)
        {
            var topic = _topics.FirstOrDefault(t => t.Id == topicId && !t.IsDeleted);
            if (topic == null)
            {
                throw new EntityNotFoundException(nameof(Topic), topicId);
            }

            var slug = !string.IsNullOrWhiteSpace(request.Slug)
                ? GenerateSlug(request.Slug)
                : GenerateSlug(request.Title);

            if (_aspects.Any(a => a.TopicId == topicId && a.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) && !a.IsDeleted))
            {
                throw new DomainException($"Slug '{slug}' đã tồn tại trong Khía cạnh của Chủ đề này.");
            }

            var aspect = new Aspect
            {
                Id = Guid.NewGuid(),
                TopicId = topicId,
                Title = request.Title.Trim(),
                Slug = slug,
                Description = request.Description?.Trim() ?? string.Empty,
                OrderIndex = request.OrderIndex,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _aspects.Add(aspect);
            return Task.FromResult(aspect);
        }
    }

    public Task<Aspect> UpdateAspectAsync(Guid id, UpdateAspectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Tiêu đề Khía cạnh (Aspect) không được để trống.");
        }

        lock (_lock)
        {
            var aspect = _aspects.FirstOrDefault(a => a.Id == id && !a.IsDeleted);
            if (aspect == null)
            {
                throw new EntityNotFoundException(nameof(Aspect), id);
            }

            var slug = !string.IsNullOrWhiteSpace(request.Slug)
                ? GenerateSlug(request.Slug)
                : GenerateSlug(request.Title);

            if (_aspects.Any(a => a.Id != id && a.TopicId == aspect.TopicId && a.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) && !a.IsDeleted))
            {
                throw new DomainException($"Slug '{slug}' đã tồn tại trong Khía cạnh của Chủ đề này.");
            }

            aspect.Title = request.Title.Trim();
            aspect.Slug = slug;
            aspect.Description = request.Description?.Trim() ?? string.Empty;
            aspect.OrderIndex = request.OrderIndex;
            aspect.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult(aspect);
        }
    }

    public Task DeleteAspectAsync(Guid id)
    {
        lock (_lock)
        {
            var aspect = _aspects.FirstOrDefault(a => a.Id == id && !a.IsDeleted);
            if (aspect == null)
            {
                throw new EntityNotFoundException(nameof(Aspect), id);
            }

            aspect.IsDeleted = true;
            aspect.UpdatedAt = DateTime.UtcNow;

            // Xóa mềm các Categories trực thuộc Aspect
            foreach (var cat in _categories.Where(c => c.AspectId == id && !c.IsDeleted))
            {
                cat.IsDeleted = true;
                cat.UpdatedAt = DateTime.UtcNow;
            }

            return Task.CompletedTask;
        }
    }

    #endregion

    #region Categories CRUD

    public Task<IEnumerable<Category>> GetCategoriesByAspectIdAsync(Guid aspectId)
    {
        lock (_lock)
        {
            var aspectExists = _aspects.Any(a => a.Id == aspectId && !a.IsDeleted);
            if (!aspectExists)
            {
                throw new EntityNotFoundException(nameof(Aspect), aspectId);
            }

            var categories = _categories
                .Where(c => c.AspectId == aspectId && !c.IsDeleted)
                .OrderBy(c => c.OrderIndex)
                .ToList();

            return Task.FromResult<IEnumerable<Category>>(categories);
        }
    }

    public Task<Category> GetCategoryByIdAsync(Guid id)
    {
        lock (_lock)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id && !c.IsDeleted);
            if (category == null)
            {
                throw new EntityNotFoundException(nameof(Category), id);
            }
            return Task.FromResult(category);
        }
    }

    public Task<Category> CreateCategoryAsync(Guid aspectId, CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Tiêu đề Chuyên mục (Category) không được để trống.");
        }

        lock (_lock)
        {
            var aspect = _aspects.FirstOrDefault(a => a.Id == aspectId && !a.IsDeleted);
            if (aspect == null)
            {
                throw new EntityNotFoundException(nameof(Aspect), aspectId);
            }

            var slug = !string.IsNullOrWhiteSpace(request.Slug)
                ? GenerateSlug(request.Slug)
                : GenerateSlug(request.Title);

            if (_categories.Any(c => c.AspectId == aspectId && c.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) && !c.IsDeleted))
            {
                throw new DomainException($"Slug '{slug}' đã tồn tại trong Chuyên mục của Khía cạnh này.");
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                AspectId = aspectId,
                Title = request.Title.Trim(),
                Slug = slug,
                Description = request.Description?.Trim() ?? string.Empty,
                OrderIndex = request.OrderIndex,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _categories.Add(category);
            return Task.FromResult(category);
        }
    }

    public Task<Category> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Tiêu đề Chuyên mục (Category) không được để trống.");
        }

        lock (_lock)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id && !c.IsDeleted);
            if (category == null)
            {
                throw new EntityNotFoundException(nameof(Category), id);
            }

            var slug = !string.IsNullOrWhiteSpace(request.Slug)
                ? GenerateSlug(request.Slug)
                : GenerateSlug(request.Title);

            if (_categories.Any(c => c.Id != id && c.AspectId == category.AspectId && c.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) && !c.IsDeleted))
            {
                throw new DomainException($"Slug '{slug}' đã tồn tại trong Chuyên mục của Khía cạnh này.");
            }

            category.Title = request.Title.Trim();
            category.Slug = slug;
            category.Description = request.Description?.Trim() ?? string.Empty;
            category.OrderIndex = request.OrderIndex;
            category.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult(category);
        }
    }

    public Task DeleteCategoryAsync(Guid id)
    {
        lock (_lock)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id && !c.IsDeleted);
            if (category == null)
            {
                throw new EntityNotFoundException(nameof(Category), id);
            }

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;

            return Task.CompletedTask;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Chuyển đổi chuỗi tiếng Việt hoặc bất kỳ tiêu đề nào sang chuẩn slug URL (kebab-case).
    /// </summary>
    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // Bỏ dấu tiếng Việt
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        var clean = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        clean = clean.Replace("đ", "d");

        // Thay ký tự đặc biệt bằng gạch ngang
        clean = Regex.Replace(clean, @"[^a-z0-9\s-]", "");
        clean = Regex.Replace(clean, @"\s+", "-").Trim('-');

        return clean;
    }

    private void SeedInitialTaxonomy()
    {
        // Topic 1
        var topic1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var topic1 = new Topic
        {
            Id = topic1Id,
            Title = "Lập trình .NET & ASP.NET Core",
            Slug = "lap-trinh-dotnet-aspnet-core",
            Description = "Toàn bộ lộ trình từ nền tảng C# đến xây dựng RESTful Web API và EF Core.",
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-10)
        };
        _topics.Add(topic1);

        var aspect11Id = Guid.Parse("11111111-aaaa-1111-1111-111111111111");
        var aspect11 = new Aspect
        {
            Id = aspect11Id,
            TopicId = topic1Id,
            Title = "Nền tảng C# Hiện đại",
            Slug = "nen-tang-csharp-hien-dai",
            Description = "Kiến thức ngôn ngữ C# mới nhất: OOP, Collections, LINQ, Async/Await.",
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-10)
        };
        _aspects.Add(aspect11);

        _categories.Add(new Category
        {
            Id = Guid.Parse("11111111-aaaa-c111-1111-111111111111"),
            AspectId = aspect11Id,
            Title = "OOP & Interface-based Design",
            Slug = "oop-interface-based-design",
            Description = "Thiết kế hướng đối tượng và kiến trúc dựa trên Interface.",
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-10)
        });

        _categories.Add(new Category
        {
            Id = Guid.Parse("11111111-aaaa-c222-1111-111111111111"),
            AspectId = aspect11Id,
            Title = "LINQ to Objects & Generic Collections",
            Slug = "linq-to-objects-generic-collections",
            Description = "Thao tác mảng dữ liệu chuyên sâu với LINQ và Collections.",
            OrderIndex = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-10)
        });

        var aspect12Id = Guid.Parse("11111111-bbbb-1111-1111-111111111111");
        var aspect12 = new Aspect
        {
            Id = aspect12Id,
            TopicId = topic1Id,
            Title = "ASP.NET Core Web API",
            Slug = "aspnet-core-web-api",
            Description = "Xây dựng Web API Pipeline, Middleware, Routing và Validation.",
            OrderIndex = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-9),
            UpdatedAt = DateTime.UtcNow.AddDays(-9)
        };
        _aspects.Add(aspect12);

        _categories.Add(new Category
        {
            Id = Guid.Parse("11111111-bbbb-c111-1111-111111111111"),
            AspectId = aspect12Id,
            Title = "Middleware Pipeline & Dependency Injection",
            Slug = "middleware-pipeline-di",
            Description = "Kiến trúc đường ống và cơ chế tiêm phụ thuộc của ASP.NET Core.",
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-9),
            UpdatedAt = DateTime.UtcNow.AddDays(-9)
        });

        _categories.Add(new Category
        {
            Id = Guid.Parse("11111111-bbbb-c222-1111-111111111111"),
            AspectId = aspect12Id,
            Title = "RESTful Routing & Model Validation",
            Slug = "restful-routing-model-validation",
            Description = "Định tuyến RESTful, Model Binding và kiểm tra tính hợp lệ dữ liệu.",
            OrderIndex = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-9),
            UpdatedAt = DateTime.UtcNow.AddDays(-9)
        });

        // Topic 2
        var topic2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var topic2 = new Topic
        {
            Id = topic2Id,
            Title = "Kiến trúc Hệ thống & Thời Gian Thực",
            Slug = "kien-truc-he-thong-thoi-gian-thuc",
            Description = "Cộng tác thời gian thực qua SignalR, Clean Architecture và triết lý Local-First.",
            OrderIndex = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            UpdatedAt = DateTime.UtcNow.AddDays(-8)
        };
        _topics.Add(topic2);

        var aspect21Id = Guid.Parse("22222222-aaaa-2222-2222-222222222222");
        var aspect21 = new Aspect
        {
            Id = aspect21Id,
            TopicId = topic2Id,
            Title = "Cộng tác Đa người dùng qua SignalR",
            Slug = "cong-tac-da-nguoi-dung-signalr",
            Description = "Xây dựng hub thời gian thực cho con trỏ chuột và tin nhắn nổi.",
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            UpdatedAt = DateTime.UtcNow.AddDays(-8)
        };
        _aspects.Add(aspect21);

        _categories.Add(new Category
        {
            Id = Guid.Parse("22222222-aaaa-c111-2222-222222222222"),
            AspectId = aspect21Id,
            Title = "Con trỏ hiện diện & Bong bóng chat nổi",
            Slug = "presence-cursors-floating-chat",
            Description = "Đồng bộ tọa độ chuột và bong bóng chat ngữ cảnh nổi.",
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            UpdatedAt = DateTime.UtcNow.AddDays(-8)
        });

        _categories.Add(new Category
        {
            Id = Guid.Parse("22222222-aaaa-c222-2222-222222222222"),
            AspectId = aspect21Id,
            Title = "Điều phối Lớp học & Chế độ Spotlight",
            Slug = "classroom-session-spotlight-mode",
            Description = "Chế độ giáo viên điều phối Follow Me và kiểm soát tương tác.",
            OrderIndex = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            UpdatedAt = DateTime.UtcNow.AddDays(-8)
        });
    }

    #endregion
}

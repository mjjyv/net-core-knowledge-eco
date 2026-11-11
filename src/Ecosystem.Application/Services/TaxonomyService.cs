using Ecosystem.Application.Common;
using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Models;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Exceptions;
using Ecosystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecosystem.Application.Services;

/// <summary>
/// Dịch vụ quản lý Cây Tri Thức 3 cấp (Topic -> Aspect -> Category) chuẩn Clean &amp; N-Tier Architecture.
/// Sử dụng IUnitOfWork kết hợp Repositories để lưu trữ bền vững trong CSDL và đảm bảo toàn vẹn Transaction.
/// </summary>
public class TaxonomyService : ITaxonomyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaxonomyService> _logger;

    public TaxonomyService(IUnitOfWork unitOfWork, ILogger<TaxonomyService>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _logger = logger ?? NullLogger<TaxonomyService>.Instance;
    }

    #region Topics CRUD

    public async Task<IEnumerable<Topic>> GetAllTopicsAsync(TaxonomyFilterQuery? query = null)
    {
        return await _unitOfWork.Topics.GetFilteredAsync(
            query?.SearchTerm,
            query?.SortBy,
            query?.IsDescending ?? false);
    }

    public async Task<Topic> GetTopicByIdAsync(Guid id)
    {
        var topic = await _unitOfWork.Topics.GetByIdWithAspectsAsync(id);
        if (topic == null)
        {
            throw new EntityNotFoundException(nameof(Topic), id);
        }
        return topic;
    }

    public async Task<Topic> GetTopicBySlugAsync(string slug)
    {
        var topic = await _unitOfWork.Topics.GetBySlugWithHierarchyAsync(slug);
        if (topic == null)
        {
            throw new EntityNotFoundException(nameof(Topic), slug);
        }
        return topic;
    }

    public async Task<Topic> CreateTopicAsync(CreateTopicRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Tiêu đề Chủ đề (Topic) không được để trống.");
        }

        var slug = !string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Slug)
            : GenerateSlug(request.Title);

        if (await _unitOfWork.Topics.SlugExistsAsync(slug))
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

        await _unitOfWork.Topics.AddAsync(topic);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Đã tạo Chủ đề mới {TopicId} với Slug '{Slug}'", topic.Id, topic.Slug);
        return topic;
    }

    public async Task<Topic> UpdateTopicAsync(Guid id, UpdateTopicRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Tiêu đề Chủ đề (Topic) không được để trống.");
        }

        var topic = await _unitOfWork.Topics.GetByIdAsync(id);
        if (topic == null)
        {
            throw new EntityNotFoundException(nameof(Topic), id);
        }

        var slug = !string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Slug)
            : GenerateSlug(request.Title);

        if (await _unitOfWork.Topics.SlugExistsAsync(slug, excludeId: id))
        {
            throw new DomainException($"Slug '{slug}' đã tồn tại trên một Chủ đề khác.");
        }

        topic.Title = request.Title.Trim();
        topic.Slug = slug;
        topic.Description = request.Description?.Trim() ?? string.Empty;
        topic.OrderIndex = request.OrderIndex;

        _unitOfWork.Topics.Update(topic);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Đã cập nhật Chủ đề {TopicId}", id);
        return topic;
    }

    public async Task DeleteTopicAsync(Guid id)
    {
        var topic = await _unitOfWork.Topics.GetByIdAsync(id);
        if (topic == null)
        {
            throw new EntityNotFoundException(nameof(Topic), id);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            _unitOfWork.Topics.SoftDelete(topic);

            var aspects = await _unitOfWork.Aspects.GetByTopicIdAsync(id);
            foreach (var aspect in aspects)
            {
                _unitOfWork.Aspects.SoftDelete(aspect);

                var categories = await _unitOfWork.Categories.GetByAspectIdAsync(aspect.Id);
                foreach (var cat in categories)
                {
                    _unitOfWork.Categories.SoftDelete(cat);
                }
            }

            await _unitOfWork.CommitTransactionAsync();
            _logger.LogInformation("Đã xóa mềm Chủ đề {TopicId} cùng toàn bộ Aspects và Categories trực thuộc", id);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    #endregion

    #region Aspects CRUD

    public async Task<IEnumerable<Aspect>> GetAspectsByTopicIdAsync(Guid topicId)
    {
        var topicExists = await _unitOfWork.Topics.AnyAsync(t => t.Id == topicId);
        if (!topicExists)
        {
            throw new EntityNotFoundException(nameof(Topic), topicId);
        }

        return await _unitOfWork.Aspects.GetByTopicIdAsync(topicId);
    }

    public async Task<Aspect> GetAspectByIdAsync(Guid id)
    {
        var aspect = await _unitOfWork.Aspects.GetByIdWithCategoriesAsync(id);
        if (aspect == null)
        {
            throw new EntityNotFoundException(nameof(Aspect), id);
        }
        return aspect;
    }

    public async Task<Aspect> CreateAspectAsync(Guid topicId, CreateAspectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Tiêu đề Khía cạnh (Aspect) không được để trống.");
        }

        var topicExists = await _unitOfWork.Topics.AnyAsync(t => t.Id == topicId);
        if (!topicExists)
        {
            throw new EntityNotFoundException(nameof(Topic), topicId);
        }

        var slug = !string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Slug)
            : GenerateSlug(request.Title);

        if (await _unitOfWork.Aspects.SlugExistsAsync(slug))
        {
            throw new DomainException($"Slug '{slug}' đã tồn tại trong hệ thống.");
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

        await _unitOfWork.Aspects.AddAsync(aspect);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Đã tạo Khía cạnh {AspectId} thuộc Chủ đề {TopicId}", aspect.Id, topicId);
        return aspect;
    }

    public async Task<Aspect> UpdateAspectAsync(Guid id, UpdateAspectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Tiêu đề Khía cạnh (Aspect) không được để trống.");
        }

        var aspect = await _unitOfWork.Aspects.GetByIdAsync(id);
        if (aspect == null)
        {
            throw new EntityNotFoundException(nameof(Aspect), id);
        }

        var slug = !string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Slug)
            : GenerateSlug(request.Title);

        if (await _unitOfWork.Aspects.SlugExistsAsync(slug, excludeId: id))
        {
            throw new DomainException($"Slug '{slug}' đã tồn tại trên một Khía cạnh khác.");
        }

        aspect.Title = request.Title.Trim();
        aspect.Slug = slug;
        aspect.Description = request.Description?.Trim() ?? string.Empty;
        aspect.OrderIndex = request.OrderIndex;

        _unitOfWork.Aspects.Update(aspect);
        await _unitOfWork.SaveChangesAsync();

        return aspect;
    }

    public async Task DeleteAspectAsync(Guid id)
    {
        var aspect = await _unitOfWork.Aspects.GetByIdAsync(id);
        if (aspect == null)
        {
            throw new EntityNotFoundException(nameof(Aspect), id);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            _unitOfWork.Aspects.SoftDelete(aspect);

            var categories = await _unitOfWork.Categories.GetByAspectIdAsync(id);
            foreach (var cat in categories)
            {
                _unitOfWork.Categories.SoftDelete(cat);
            }

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    #endregion

    #region Categories CRUD

    public async Task<IEnumerable<Category>> GetCategoriesByAspectIdAsync(Guid aspectId)
    {
        var aspectExists = await _unitOfWork.Aspects.AnyAsync(a => a.Id == aspectId);
        if (!aspectExists)
        {
            throw new EntityNotFoundException(nameof(Aspect), aspectId);
        }

        return await _unitOfWork.Categories.GetByAspectIdAsync(aspectId);
    }

    public async Task<Category> GetCategoryByIdAsync(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            throw new EntityNotFoundException(nameof(Category), id);
        }
        return category;
    }

    public async Task<Category> CreateCategoryAsync(Guid aspectId, CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Tiêu đề Chuyên mục (Category) không được để trống.");
        }

        var aspectExists = await _unitOfWork.Aspects.AnyAsync(a => a.Id == aspectId);
        if (!aspectExists)
        {
            throw new EntityNotFoundException(nameof(Aspect), aspectId);
        }

        var slug = !string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Slug)
            : GenerateSlug(request.Title);

        if (await _unitOfWork.Categories.SlugExistsAsync(slug))
        {
            throw new DomainException($"Slug '{slug}' đã tồn tại trong hệ thống.");
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

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Đã tạo Chuyên mục {CategoryId} thuộc Khía cạnh {AspectId}", category.Id, aspectId);
        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Tiêu đề Chuyên mục (Category) không được để trống.");
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            throw new EntityNotFoundException(nameof(Category), id);
        }

        var slug = !string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Slug)
            : GenerateSlug(request.Title);

        if (await _unitOfWork.Categories.SlugExistsAsync(slug, excludeId: id))
        {
            throw new DomainException($"Slug '{slug}' đã tồn tại trên một Chuyên mục khác.");
        }

        category.Title = request.Title.Trim();
        category.Slug = slug;
        category.Description = request.Description?.Trim() ?? string.Empty;
        category.OrderIndex = request.OrderIndex;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return category;
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            throw new EntityNotFoundException(nameof(Category), id);
        }

        _unitOfWork.Categories.SoftDelete(category);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Chuyển đổi chuỗi tiếng Việt hoặc bất kỳ tiêu đề nào sang chuẩn slug URL (kebab-case).
    /// </summary>
    public static string GenerateSlug(string text) => SlugHelper.GenerateSlug(text);

    #endregion
}

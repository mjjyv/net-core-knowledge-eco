using System.Text.RegularExpressions;
using AutoMapper;
using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Mappers;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecosystem.Application.Services;

/// <summary>
/// Dịch vụ nghiệp vụ quản trị Bài viết tri thức (Article) thuộc tầng Application (Clean &amp; N-Tier Architecture).
/// Chỉ phụ thuộc vào IUnitOfWork và IMapper — hoàn toàn tách biệt khỏi Entity Framework Core và DbContext.
/// </summary>
public class ArticleService : IArticleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ArticleService> _logger;

    public ArticleService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ArticleService>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger ?? NullLogger<ArticleService>.Instance;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ArticleResponseDto>> GetAllAsync(ArticleFilterQuery query, CancellationToken cancellationToken = default)
    {
        var articles = await _unitOfWork.Articles.GetFilteredAsync(
            query.CategoryId,
            query.AspectId,
            query.TopicId,
            query.AuthorId,
            query.Status,
            query.SearchTerm,
            query.SortBy,
            query.IsDescending,
            cancellationToken);

        return _mapper.Map<IEnumerable<ArticleResponseDto>>(articles);
    }

    /// <inheritdoc />
    public async Task<PagedResult<ArticleResponseDto>> GetPagedAsync(ArticleFilterQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Articles.GetPagedAsync(
            query.CategoryId,
            query.AspectId,
            query.TopicId,
            query.AuthorId,
            query.Status,
            query.SearchTerm,
            query.SortBy,
            query.IsDescending,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<ArticleResponseDto>>(items);
        return new PagedResult<ArticleResponseDto>(dtos, totalCount, query.PageNumber, query.PageSize);
    }

    /// <inheritdoc />
    public async Task<ArticleDetailResponseDto?> GetByIdAsync(Guid id, bool incrementView = false, CancellationToken cancellationToken = default)
    {
        if (incrementView)
        {
            var articleToUpdate = await _unitOfWork.Articles.GetByIdAsync(id, cancellationToken);
            if (articleToUpdate != null)
            {
                articleToUpdate.ViewCount++;
                _unitOfWork.Articles.Update(articleToUpdate);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        var article = await _unitOfWork.Articles.GetByIdWithHierarchyAsync(id, asNoTracking: true, cancellationToken);
        return article == null ? null : _mapper.Map<ArticleDetailResponseDto>(article);
    }

    /// <inheritdoc />
    public async Task<ArticleDetailResponseDto?> GetBySlugAsync(string slug, bool incrementView = false, CancellationToken cancellationToken = default)
    {
        if (incrementView)
        {
            var articleToUpdate = await _unitOfWork.Articles.GetBySlugWithHierarchyAsync(slug, asNoTracking: false, cancellationToken);
            if (articleToUpdate != null)
            {
                articleToUpdate.ViewCount++;
                _unitOfWork.Articles.Update(articleToUpdate);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        var article = await _unitOfWork.Articles.GetBySlugWithHierarchyAsync(slug, asNoTracking: true, cancellationToken);
        return article == null ? null : _mapper.Map<ArticleDetailResponseDto>(article);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ArticleResponseDto>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken);
        if (!exists)
        {
            throw new KeyNotFoundException($"Không tìm thấy chuyên mục với ID {categoryId}.");
        }

        var articles = await _unitOfWork.Articles.GetByCategoryIdAsync(categoryId, cancellationToken);
        return _mapper.Map<IEnumerable<ArticleResponseDto>>(articles);
    }

    /// <inheritdoc />
    public async Task<ArticleDetailResponseDto> CreateAsync(ArticleCreateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Bắt đầu tạo bài viết mới '{Title}' thuộc chuyên mục {CategoryId}", dto.Title, dto.CategoryId);

        // 1. Kiểm tra CategoryId hợp lệ
        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == dto.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            _logger.LogWarning("Không tìm thấy chuyên mục {CategoryId} khi tạo bài viết", dto.CategoryId);
            throw new KeyNotFoundException($"Không tìm thấy chuyên mục với ID {dto.CategoryId}.");
        }

        // 2. Xác định AuthorId
        Guid authorId;
        if (dto.AuthorId.HasValue && dto.AuthorId.Value != Guid.Empty)
        {
            var authorExists = await _unitOfWork.Users.AnyAsync(u => u.Id == dto.AuthorId.Value, cancellationToken);
            if (!authorExists)
            {
                throw new KeyNotFoundException($"Không tìm thấy tác giả với ID {dto.AuthorId.Value}.");
            }
            authorId = dto.AuthorId.Value;
        }
        else
        {
            var defaultAdmin = await _unitOfWork.Users.FirstOrDefaultAsync(_ => true, cancellationToken);
            if (defaultAdmin == null)
            {
                throw new InvalidOperationException("Không tìm thấy người dùng khả dụng trong hệ thống để gán tác giả.");
            }
            authorId = defaultAdmin.Id;
        }

        // 3. Tự động sinh Slug nếu thiếu hoặc chuẩn hóa Slug
        var targetSlug = string.IsNullOrWhiteSpace(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Title)
            : SlugHelper.GenerateSlug(dto.Slug);

        if (string.IsNullOrWhiteSpace(targetSlug))
        {
            targetSlug = Guid.NewGuid().ToString("N")[..8];
        }

        // 4. Kiểm tra trùng Slug (409 Conflict)
        if (await SlugExistsAsync(targetSlug, null, cancellationToken))
        {
            throw new InvalidOperationException($"Slug '{targetSlug}' đã tồn tại trong hệ sinh thái. Vui lòng chọn slug khác hoặc đổi tiêu đề.");
        }

        // 5. Tự động tính Reading Time
        var readingTime = CalculateReadingTime(dto.ReadingTimeMinutes, dto.MarkdownContent);

        var article = new Article
        {
            Id = Guid.NewGuid(),
            CategoryId = dto.CategoryId,
            AuthorId = authorId,
            Title = dto.Title.Trim(),
            Slug = targetSlug,
            Summary = dto.Summary?.Trim() ?? string.Empty,
            MarkdownContent = dto.MarkdownContent,
            Status = dto.Status,
            ViewCount = 0,
            ReadingTimeMinutes = readingTime,
            TagsJson = ArticleMappingExtensions.SerializeTags(dto.Tags),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Articles.AddAsync(article, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Đã lưu thành công bài viết {ArticleId} với Slug '{Slug}'", article.Id, article.Slug);

        var created = await GetByIdAsync(article.Id, incrementView: false, cancellationToken);
        return created ?? _mapper.Map<ArticleDetailResponseDto>(article);
    }

    /// <inheritdoc />
    public async Task<ArticleDetailResponseDto?> UpdateAsync(Guid id, ArticleUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var article = await _unitOfWork.Articles.GetByIdAsync(id, cancellationToken);
        if (article == null)
        {
            return null;
        }

        // 1. Kiểm tra CategoryId hợp lệ
        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == dto.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new KeyNotFoundException($"Không tìm thấy chuyên mục với ID {dto.CategoryId}.");
        }

        // 2. Xử lý Slug
        var targetSlug = string.IsNullOrWhiteSpace(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Title)
            : SlugHelper.GenerateSlug(dto.Slug);

        if (string.IsNullOrWhiteSpace(targetSlug))
        {
            targetSlug = article.Slug;
        }

        // 3. Kiểm tra trùng Slug ngoại trừ chính bài viết này
        if (await SlugExistsAsync(targetSlug, id, cancellationToken))
        {
            throw new InvalidOperationException($"Slug '{targetSlug}' đã tồn tại cho một bài viết khác.");
        }

        // 4. Cập nhật thuộc tính
        article.CategoryId = dto.CategoryId;
        article.Title = dto.Title.Trim();
        article.Slug = targetSlug;
        article.Summary = dto.Summary?.Trim() ?? string.Empty;
        article.MarkdownContent = dto.MarkdownContent;
        article.Status = dto.Status;
        article.ReadingTimeMinutes = CalculateReadingTime(dto.ReadingTimeMinutes, dto.MarkdownContent);
        article.TagsJson = ArticleMappingExtensions.SerializeTags(dto.Tags);

        _unitOfWork.Articles.Update(article);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Đã cập nhật bài viết {ArticleId}", id);

        return await GetByIdAsync(id, incrementView: false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, bool isHardDelete = false, CancellationToken cancellationToken = default)
    {
        if (isHardDelete)
        {
            var article = await _unitOfWork.Articles.GetByIdIgnoreFiltersAsync(id, cancellationToken);
            if (article == null)
            {
                return false;
            }

            _unitOfWork.Articles.Remove(article);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Đã xóa cứng (Hard Delete) bài viết {ArticleId}", id);
            return true;
        }
        else
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(id, cancellationToken);
            if (article == null)
            {
                return false;
            }

            _unitOfWork.Articles.SoftDelete(article);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Đã xóa mềm (Soft Delete) bài viết {ArticleId}", id);
            return true;
        }
    }

    private static readonly Regex AttachmentLinkRegex = new(
        @"(?:/api/v1/files/|/uploads/)([A-Za-z0-9._-]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <inheritdoc />
    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Articles.SlugExistsAsync(slug, excludeId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ArticleBundleDto?> GetBundleByIdAsync(
        Guid id,
        string? baseUrl = null,
        CancellationToken cancellationToken = default)
    {
        var article = await _unitOfWork.Articles.GetByIdWithHierarchyAsync(id, asNoTracking: true, cancellationToken);
        if (article == null)
        {
            return null;
        }

        var topicSlug = article.Category?.Aspect?.Topic?.Slug ?? "general";
        var aspectSlug = article.Category?.Aspect?.Slug ?? "general";
        var categorySlug = article.Category?.Slug ?? "general";
        var normalizedBase = string.IsNullOrWhiteSpace(baseUrl) ? string.Empty : baseUrl.TrimEnd('/');

        var markdown = article.MarkdownContent ?? string.Empty;
        var attachments = AttachmentLinkRegex.Matches(markdown)
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(fileName => new ArticleAttachmentLinkDto
            {
                FileName = fileName,
                DownloadUrl = string.IsNullOrEmpty(normalizedBase)
                    ? $"/api/v1/files/{fileName}"
                    : $"{normalizedBase}/api/v1/files/{fileName}"
            })
            .ToList();

        return new ArticleBundleDto
        {
            ArticleId = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            MarkdownContent = markdown,
            CategoryPath = $"{topicSlug}/{aspectSlug}/{categorySlug}",
            DeepLink = $"ecosystem://article/{article.Slug}",
            Tags = ArticleMappingExtensions.ParseTags(article.TagsJson),
            Attachments = attachments,
            ExportedAt = DateTime.UtcNow
        };
    }

    private static int CalculateReadingTime(int? providedMinutes, string content)
    {
        var readingTime = providedMinutes.GetValueOrDefault();
        if (readingTime > 0)
        {
            return readingTime;
        }

        var wordCount = (content ?? string.Empty)
            .Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Length;
        return Math.Max(1, wordCount / 200);
    }
}

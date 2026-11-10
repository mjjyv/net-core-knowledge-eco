using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Mappers;
using Ecosystem.Domain.Entities;
using Ecosystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.Infrastructure.Services;

/// <summary>
/// Dịch vụ nghiệp vụ quản trị Bài viết tri thức (Article) sử dụng Entity Framework Core LINQ tối ưu (Buổi 6).
/// Áp dụng Eager Loading (.Include/.ThenInclude), .AsNoTracking() cho truy vấn đọc, và triệt tiêu hoàn toàn Object Cycle.
/// </summary>
public class ArticleService : IArticleService
{
    private readonly ApplicationDbContext _context;

    public ArticleService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ArticleResponseDto>> GetAllAsync(ArticleFilterQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = _context.Articles
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .AsQueryable();

        // 1. Lọc theo chuyên mục cấp 3
        if (query.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(a => a.CategoryId == query.CategoryId.Value);
        }

        // 2. Lọc theo khía cạnh cấp 2
        if (query.AspectId.HasValue)
        {
            dbQuery = dbQuery.Where(a => a.Category.AspectId == query.AspectId.Value);
        }

        // 3. Lọc theo chủ đề lớn cấp 1
        if (query.TopicId.HasValue)
        {
            dbQuery = dbQuery.Where(a => a.Category.Aspect.TopicId == query.TopicId.Value);
        }

        // 4. Lọc theo tác giả
        if (query.AuthorId.HasValue)
        {
            dbQuery = dbQuery.Where(a => a.AuthorId == query.AuthorId.Value);
        }

        // 5. Lọc theo trạng thái xuất bản
        if (query.Status.HasValue)
        {
            dbQuery = dbQuery.Where(a => a.Status == query.Status.Value);
        }

        // 6. Tìm kiếm theo từ khóa trong tiêu đề hoặc tóm tắt
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            dbQuery = dbQuery.Where(a => a.Title.ToLower().Contains(term) || a.Summary.ToLower().Contains(term));
        }

        // 7. Sắp xếp động
        var sortBy = query.SortBy?.Trim().ToLowerInvariant();
        dbQuery = sortBy switch
        {
            "title" => query.IsDescending
                ? dbQuery.OrderByDescending(a => a.Title)
                : dbQuery.OrderBy(a => a.Title),
            "views" => query.IsDescending
                ? dbQuery.OrderByDescending(a => a.ViewCount)
                : dbQuery.OrderBy(a => a.ViewCount),
            "readingtime" => query.IsDescending
                ? dbQuery.OrderByDescending(a => a.ReadingTimeMinutes)
                : dbQuery.OrderBy(a => a.ReadingTimeMinutes),
            _ => query.IsDescending
                ? dbQuery.OrderByDescending(a => a.CreatedAt)
                : dbQuery.OrderBy(a => a.CreatedAt)
        };

        var articles = await dbQuery.ToListAsync(cancellationToken);
        return articles.Select(a => a.ToResponseDto());
    }

    /// <inheritdoc />
    public async Task<ArticleDetailResponseDto?> GetByIdAsync(Guid id, bool incrementView = false, CancellationToken cancellationToken = default)
    {
        if (incrementView)
        {
            var articleToUpdate = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
            if (articleToUpdate != null)
            {
                articleToUpdate.ViewCount++;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        var article = await _context.Articles
            .AsNoTracking()
            .Include(a => a.Category)
                .ThenInclude(c => c.Aspect)
                    .ThenInclude(asp => asp.Topic)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return article?.ToDetailResponseDto();
    }

    /// <inheritdoc />
    public async Task<ArticleDetailResponseDto?> GetBySlugAsync(string slug, bool incrementView = false, CancellationToken cancellationToken = default)
    {
        var cleanSlug = slug.Trim().ToLowerInvariant();

        if (incrementView)
        {
            var articleToUpdate = await _context.Articles.FirstOrDefaultAsync(a => a.Slug == cleanSlug, cancellationToken);
            if (articleToUpdate != null)
            {
                articleToUpdate.ViewCount++;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        var article = await _context.Articles
            .AsNoTracking()
            .Include(a => a.Category)
                .ThenInclude(c => c.Aspect)
                    .ThenInclude(asp => asp.Topic)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Slug == cleanSlug, cancellationToken);

        return article?.ToDetailResponseDto();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ArticleResponseDto>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken);
        if (!exists)
        {
            throw new KeyNotFoundException($"Không tìm thấy chuyên mục với ID {categoryId}.");
        }

        var articles = await _context.Articles
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => a.CategoryId == categoryId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return articles.Select(a => a.ToResponseDto());
    }

    /// <inheritdoc />
    public async Task<ArticleDetailResponseDto> CreateAsync(ArticleCreateDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Kiểm tra CategoryId hợp lệ
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new KeyNotFoundException($"Không tìm thấy chuyên mục với ID {dto.CategoryId}.");
        }

        // 2. Xác định AuthorId
        Guid authorId;
        if (dto.AuthorId.HasValue && dto.AuthorId.Value != Guid.Empty)
        {
            var authorExists = await _context.Users.AnyAsync(u => u.Id == dto.AuthorId.Value, cancellationToken);
            if (!authorExists)
            {
                throw new KeyNotFoundException($"Không tìm thấy tác giả với ID {dto.AuthorId.Value}.");
            }
            authorId = dto.AuthorId.Value;
        }
        else
        {
            var defaultAdmin = await _context.Users.FirstOrDefaultAsync(cancellationToken);
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

        // 5. Tự động tính toán Reading Time nếu không nhập
        var readingTime = dto.ReadingTimeMinutes.GetValueOrDefault();
        if (readingTime <= 0)
        {
            var wordCount = dto.MarkdownContent
                .Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Length;
            readingTime = Math.Max(1, wordCount / 200);
        }

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

        await _context.Articles.AddAsync(article, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await GetByIdAsync(article.Id, false, cancellationToken);
        return created!;
    }

    /// <inheritdoc />
    public async Task<ArticleDetailResponseDto?> UpdateAsync(Guid id, ArticleUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (article == null)
        {
            return null;
        }

        // 1. Kiểm tra CategoryId hợp lệ
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId, cancellationToken);
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

        // 4. Tính toán Reading Time
        var readingTime = dto.ReadingTimeMinutes.GetValueOrDefault();
        if (readingTime <= 0)
        {
            var wordCount = dto.MarkdownContent
                .Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Length;
            readingTime = Math.Max(1, wordCount / 200);
        }

        // 5. Cập nhật qua Change Tracking
        article.CategoryId = dto.CategoryId;
        article.Title = dto.Title.Trim();
        article.Slug = targetSlug;
        article.Summary = dto.Summary?.Trim() ?? string.Empty;
        article.MarkdownContent = dto.MarkdownContent;
        article.Status = dto.Status;
        article.ReadingTimeMinutes = readingTime;
        article.TagsJson = ArticleMappingExtensions.SerializeTags(dto.Tags);
        article.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, bool isHardDelete = false, CancellationToken cancellationToken = default)
    {
        if (isHardDelete)
        {
            var article = await _context.Articles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

            if (article == null)
            {
                return false;
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        else
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
            if (article == null)
            {
                return false;
            }

            article.IsDeleted = true;
            article.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var cleanSlug = slug.Trim().ToLowerInvariant();
        var query = _context.Articles.IgnoreQueryFilters().AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return await query.AnyAsync(a => a.Slug.ToLower() == cleanSlug, cancellationToken);
    }
}

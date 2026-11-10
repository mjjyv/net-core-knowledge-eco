using System.Text.Json;
using Ecosystem.Application.DTOs;
using Ecosystem.Domain.Entities;

namespace Ecosystem.Application.Mappers;

/// <summary>
/// Extension methods ánh xạ giữa thực thể Article (Entity) và các lớp DTOs nhằm triệt tiêu Object Cycle.
/// </summary>
public static class ArticleMappingExtensions
{
    public static ArticleResponseDto ToResponseDto(this Article article)
    {
        return new ArticleResponseDto
        {
            Id = article.Id,
            CategoryId = article.CategoryId,
            CategoryTitle = article.Category?.Title ?? string.Empty,
            CategorySlug = article.Category?.Slug ?? string.Empty,
            AuthorId = article.AuthorId,
            AuthorName = article.Author?.FullName ?? article.Author?.Username ?? string.Empty,
            Title = article.Title,
            Slug = article.Slug,
            Summary = article.Summary,
            Status = article.Status,
            ViewCount = article.ViewCount,
            ReadingTimeMinutes = article.ReadingTimeMinutes,
            Tags = ParseTags(article.TagsJson),
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt
        };
    }

    public static ArticleDetailResponseDto ToDetailResponseDto(this Article article)
    {
        return new ArticleDetailResponseDto
        {
            Id = article.Id,
            CategoryId = article.CategoryId,
            CategoryTitle = article.Category?.Title ?? string.Empty,
            CategorySlug = article.Category?.Slug ?? string.Empty,
            AspectId = article.Category?.AspectId ?? Guid.Empty,
            AspectTitle = article.Category?.Aspect?.Title ?? string.Empty,
            AspectSlug = article.Category?.Aspect?.Slug ?? string.Empty,
            TopicId = article.Category?.Aspect?.TopicId ?? Guid.Empty,
            TopicTitle = article.Category?.Aspect?.Topic?.Title ?? string.Empty,
            TopicSlug = article.Category?.Aspect?.Topic?.Slug ?? string.Empty,
            AuthorId = article.AuthorId,
            AuthorName = article.Author?.FullName ?? article.Author?.Username ?? string.Empty,
            Title = article.Title,
            Slug = article.Slug,
            Summary = article.Summary,
            MarkdownContent = article.MarkdownContent,
            Status = article.Status,
            ViewCount = article.ViewCount,
            ReadingTimeMinutes = article.ReadingTimeMinutes,
            Tags = ParseTags(article.TagsJson),
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt
        };
    }

    public static List<string> ParseTags(string? tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(tagsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static string SerializeTags(IEnumerable<string>? tags)
    {
        if (tags == null)
        {
            return "[]";
        }

        var cleanTags = tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return JsonSerializer.Serialize(cleanTags);
    }
}

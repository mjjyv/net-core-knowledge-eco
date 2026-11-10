using Ecosystem.Application.DTOs;
using Ecosystem.Domain.Entities;

namespace Ecosystem.Application.Mappers;

/// <summary>
/// Các phương thức mở rộng chuyển đổi an toàn giữa Domain Entities và DTOs.
/// Bảo vệ hệ thống khỏi lỗ hổng Over-posting và triệt tiêu Circular References.
/// </summary>
public static class TaxonomyMappingExtensions
{
    public static TopicResponseDto ToResponseDto(this Topic topic)
    {
        return new TopicResponseDto
        {
            Id = topic.Id,
            Title = topic.Title,
            Slug = topic.Slug,
            Description = topic.Description,
            OrderIndex = topic.OrderIndex,
            AspectsCount = topic.Aspects?.Count ?? 0,
            CreatedAt = topic.CreatedAt,
            UpdatedAt = topic.UpdatedAt
        };
    }

    public static TopicDetailResponseDto ToDetailResponseDto(this Topic topic)
    {
        return new TopicDetailResponseDto
        {
            Id = topic.Id,
            Title = topic.Title,
            Slug = topic.Slug,
            Description = topic.Description,
            OrderIndex = topic.OrderIndex,
            AspectsCount = topic.Aspects?.Count ?? 0,
            CreatedAt = topic.CreatedAt,
            UpdatedAt = topic.UpdatedAt,
            Aspects = topic.Aspects?.Select(a => a.ToResponseDto()).ToList() ?? new List<AspectResponseDto>()
        };
    }

    public static AspectResponseDto ToResponseDto(this Aspect aspect)
    {
        return new AspectResponseDto
        {
            Id = aspect.Id,
            TopicId = aspect.TopicId,
            Title = aspect.Title,
            Slug = aspect.Slug,
            Description = aspect.Description,
            OrderIndex = aspect.OrderIndex,
            CategoriesCount = aspect.Categories?.Count ?? 0,
            CreatedAt = aspect.CreatedAt,
            UpdatedAt = aspect.UpdatedAt
        };
    }

    public static AspectDetailResponseDto ToDetailResponseDto(this Aspect aspect)
    {
        return new AspectDetailResponseDto
        {
            Id = aspect.Id,
            TopicId = aspect.TopicId,
            Title = aspect.Title,
            Slug = aspect.Slug,
            Description = aspect.Description,
            OrderIndex = aspect.OrderIndex,
            CategoriesCount = aspect.Categories?.Count ?? 0,
            CreatedAt = aspect.CreatedAt,
            UpdatedAt = aspect.UpdatedAt,
            Categories = aspect.Categories?.Select(c => c.ToResponseDto()).ToList() ?? new List<CategoryResponseDto>()
        };
    }

    public static CategoryResponseDto ToResponseDto(this Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            AspectId = category.AspectId,
            Title = category.Title,
            Slug = category.Slug,
            Description = category.Description,
            OrderIndex = category.OrderIndex,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}

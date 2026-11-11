using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Exceptions;
using Ecosystem.Domain.Interfaces;

namespace Ecosystem.Application.Services;

/// <summary>
/// Dịch vụ cung cấp dữ liệu tổng quan Cây Tri Thức từ cơ sở dữ liệu thông qua IUnitOfWork (N-Tier Architecture).
/// </summary>
public class TaxonomyOverviewService : ITaxonomyOverviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public TaxonomyOverviewService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TopicOverviewDto>> GetTaxonomyHierarchyAsync(CancellationToken cancellationToken = default)
    {
        var topics = await _unitOfWork.Topics.GetAllWithFullHierarchyAsync(cancellationToken);
        return topics.Select(MapToOverviewDto).ToList().AsReadOnly();
    }

    public async Task<TopicOverviewDto> GetTopicBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        var topic = await _unitOfWork.Topics.GetBySlugWithHierarchyAsync(slug, cancellationToken);
        if (topic == null)
        {
            throw new EntityNotFoundException("Topic", slug);
        }

        return MapToOverviewDto(topic);
    }

    private static TopicOverviewDto MapToOverviewDto(Topic topic)
    {
        return new TopicOverviewDto(
            Id: topic.Id,
            Title: topic.Title,
            Slug: topic.Slug,
            Description: topic.Description,
            Aspects: topic.Aspects
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.OrderIndex)
                .Select(a => new AspectOverviewDto(
                    Id: a.Id,
                    Title: a.Title,
                    Slug: a.Slug,
                    Categories: a.Categories
                        .Where(c => !c.IsDeleted)
                        .OrderBy(c => c.OrderIndex)
                        .Select(c => new CategoryOverviewDto(
                            Id: c.Id,
                            Title: c.Title,
                            Slug: c.Slug,
                            ArticleCount: c.Articles?.Count(art => !art.IsDeleted) ?? 0
                        ))
                        .ToList()
                ))
                .ToList()
        );
    }
}

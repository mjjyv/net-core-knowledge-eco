using Ecosystem.Application.Services;
using Ecosystem.Domain.Exceptions;
using Xunit;

namespace Ecosystem.UnitTests.Application;

public class TaxonomyOverviewServiceTests
{
    private readonly TaxonomyOverviewService _service = new();

    [Fact]
    public async Task GetTaxonomyHierarchyAsync_ShouldReturnBaselineTopics()
    {
        // Act
        var topics = await _service.GetTaxonomyHierarchyAsync();

        // Assert
        Assert.NotNull(topics);
        Assert.True(topics.Count >= 2, "Hệ thống phải có ít nhất 2 chủ đề lớn cốt lõi.");
        Assert.Contains(topics, t => t.Slug == "lap-trinh-dotnet-aspnet-core");
        Assert.Contains(topics, t => t.Slug == "kien-truc-he-thong-thoi-gian-thuc");
    }

    [Fact]
    public async Task GetTopicBySlugAsync_WhenSlugExists_ShouldReturnTopic()
    {
        // Act
        var topic = await _service.GetTopicBySlugAsync("lap-trinh-dotnet-aspnet-core");

        // Assert
        Assert.NotNull(topic);
        Assert.Equal("Lập trình .NET & ASP.NET Core", topic.Title);
        Assert.NotEmpty(topic.Aspects);
    }

    [Fact]
    public async Task GetTopicBySlugAsync_WhenSlugDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _service.GetTopicBySlugAsync("non-existent-topic-slug"));
    }
}

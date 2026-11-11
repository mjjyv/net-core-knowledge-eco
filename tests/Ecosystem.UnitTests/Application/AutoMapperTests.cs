using AutoMapper;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Mappings;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecosystem.UnitTests.Application;

public class AutoMapperTests
{
    private static MapperConfiguration CreateConfig() =>
        new(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);

    [Fact]
    public void MappingProfile_ConfigurationIsValid()
    {
        // Arrange
        var config = CreateConfig();

        // Assert - Đảm bảo mọi thuộc tính trên DTO đều được map hợp lệ hoặc ignore tường minh
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_ArticleWithHierarchy_FlattensBreadcrumbsAndTagsProperly()
    {
        // Arrange
        var config = CreateConfig();
        var mapper = config.CreateMapper();

        var topic = new Topic { Id = Guid.NewGuid(), Title = "Topic Test", Slug = "topic-test" };
        var aspect = new Aspect { Id = Guid.NewGuid(), TopicId = topic.Id, Topic = topic, Title = "Aspect Test", Slug = "aspect-test" };
        var category = new Category { Id = Guid.NewGuid(), AspectId = aspect.Id, Aspect = aspect, Title = "Category Test", Slug = "category-test" };
        var author = new User { Id = Guid.NewGuid(), Username = "author1", FullName = "Nguyễn Tác Giả" };
        var article = new Article
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Category = category,
            AuthorId = author.Id,
            Author = author,
            Title = "Article Test",
            Slug = "article-test",
            Summary = "Summary",
            MarkdownContent = "# Markdown",
            Status = ArticleStatus.Published,
            TagsJson = "[\"csharp\",\"dotnet\"]"
        };

        // Act
        var dto = mapper.Map<ArticleDetailResponseDto>(article);

        // Assert
        Assert.Equal("Topic Test", dto.TopicTitle);
        Assert.Equal("Aspect Test", dto.AspectTitle);
        Assert.Equal("Category Test", dto.CategoryTitle);
        Assert.Equal("Nguyễn Tác Giả", dto.AuthorName);
        Assert.Equal(2, dto.Tags.Count);
        Assert.Contains("csharp", dto.Tags);
        Assert.Equal("# Markdown", dto.MarkdownContent);
    }
}

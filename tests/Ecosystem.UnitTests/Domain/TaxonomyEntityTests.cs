using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;
using Xunit;

namespace Ecosystem.UnitTests.Domain;

public class TaxonomyEntityTests
{
    [Fact]
    public void Taxonomy_4Tier_Hierarchy_CanBeConstructedProperly()
    {
        // 1. Topic
        var topic = new Topic
        {
            Title = "Lập trình .NET",
            Slug = "lap-trinh-dotnet",
            Description = "Chủ đề lớn về hệ sinh thái .NET"
        };

        // 2. Aspect
        var aspect = new Aspect
        {
            TopicId = topic.Id,
            Topic = topic,
            Title = "Backend Architecture",
            Slug = "backend-architecture"
        };
        topic.Aspects.Add(aspect);

        // 3. Category
        var category = new Category
        {
            AspectId = aspect.Id,
            Aspect = aspect,
            Title = "ASP.NET Core Web API",
            Slug = "aspnet-core-web-api"
        };
        aspect.Categories.Add(category);

        // 4. User (Author)
        var author = new User
        {
            Email = "author@ecosystem.local",
            FullName = "Senior Architect",
            Role = UserRole.Presenter
        };

        // 5. Article
        var article = new Article
        {
            CategoryId = category.Id,
            Category = category,
            AuthorId = author.Id,
            Author = author,
            Title = "Giới thiệu Clean Architecture trong .NET 8",
            Slug = "gioi-thieu-clean-architecture-trong-net-8",
            MarkdownContent = "# Clean Architecture\n\nKiến trúc phân tầng...",
            Status = ArticleStatus.Published,
            TagsJson = "[\"dotnet\", \"clean-architecture\"]"
        };
        category.Articles.Add(article);

        // Assert
        Assert.Equal("lap-trinh-dotnet", topic.Slug);
        Assert.Single(topic.Aspects);
        Assert.Single(aspect.Categories);
        Assert.Single(category.Articles);
        Assert.Equal(ArticleStatus.Published, article.Status);
        Assert.Equal(UserRole.Presenter, author.Role);
    }

    [Fact]
    public void RefreshToken_IsValid_ShouldBeTrue_WhenNotRevokedAndNotExpired()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "secure-token-value",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        // Assert
        Assert.False(token.IsExpired);
        Assert.True(token.IsValid);
    }

    [Fact]
    public void RefreshToken_IsValid_ShouldBeFalse_WhenRevoked()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "secure-token-value",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = true
        };

        // Assert
        Assert.False(token.IsValid);
    }

    [Fact]
    public void ClassroomSession_DefaultValues_ShouldBeValid()
    {
        // Arrange
        var host = new User { Email = "host@ecosystem.local", Role = UserRole.Host };
        var session = new ClassroomSession
        {
            HostId = host.Id,
            Host = host,
            RoomCode = "NET8-CLASS-01",
            Title = "Buổi 1: Giới thiệu Học Phần & Ôn Tập C#"
        };

        // Assert
        Assert.Equal(SessionStatus.Scheduled, session.Status);
        Assert.Equal("NET8-CLASS-01", session.RoomCode);
        Assert.Null(session.StartedAt);
    }
}

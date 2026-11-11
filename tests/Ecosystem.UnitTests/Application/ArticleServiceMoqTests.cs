using System.Linq.Expressions;
using AutoMapper;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Mappings;
using Ecosystem.Application.Services;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;
using Ecosystem.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Ecosystem.UnitTests.Application;

/// <summary>
/// Kiểm thử độc lập tầng ArticleService hoàn toàn không cần cơ sở dữ liệu bằng xUnit + Moq (Buổi 7).
/// </summary>
public class ArticleServiceMoqTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IArticleRepository> _mockArticleRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly IMapper _mapper;
    private readonly ArticleService _service;

    public ArticleServiceMoqTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockArticleRepo = new Mock<IArticleRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _mockUserRepo = new Mock<IUserRepository>();

        _mockUnitOfWork.Setup(u => u.Articles).Returns(_mockArticleRepo.Object);
        _mockUnitOfWork.Setup(u => u.Categories).Returns(_mockCategoryRepo.Object);
        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepo.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _service = new ArticleService(_mockUnitOfWork.Object, _mapper, NullLogger<ArticleService>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_CallsRepositoryAndMapsDtos()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var fakeArticles = new List<Article>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                Category = new Category { Id = categoryId, Title = "EF Core", Slug = "ef-core" },
                Author = new User { Username = "admin", FullName = "Admin" },
                Title = "Bài viết 1",
                Slug = "bai-viet-1",
                Status = ArticleStatus.Published
            }
        };

        _mockArticleRepo
            .Setup(r => r.GetFilteredAsync(
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
                It.IsAny<ArticleStatus?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeArticles);

        // Act
        var result = (await _service.GetAllAsync(new ArticleFilterQuery())).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Bài viết 1", result[0].Title);
        Assert.Equal("EF Core", result[0].CategoryTitle);
        _mockArticleRepo.Verify(r => r.GetFilteredAsync(
            null, null, null, null, null, null, null, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_AddsToRepoAndCallsSaveChangesOnce()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var dto = new ArticleCreateDto
        {
            CategoryId = categoryId,
            AuthorId = authorId,
            Title = "Kiến Trúc N-Tier Với Unit Of Work",
            Summary = "Tóm tắt",
            MarkdownContent = "# N-Tier",
            Status = ArticleStatus.Published,
            Tags = new List<string> { "ntier", "uow" }
        };

        _mockCategoryRepo
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUserRepo
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockArticleRepo
            .Setup(r => r.SlugExistsAsync("kien-truc-n-tier-voi-unit-of-work", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockArticleRepo
            .Setup(r => r.AddAsync(It.IsAny<Article>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockArticleRepo
            .Setup(r => r.GetByIdWithHierarchyAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, bool _, CancellationToken _) => new Article
            {
                Id = id,
                CategoryId = categoryId,
                AuthorId = authorId,
                Title = dto.Title,
                Slug = "kien-truc-n-tier-voi-unit-of-work",
                MarkdownContent = dto.MarkdownContent,
                ReadingTimeMinutes = 1
            });

        // Act
        var created = await _service.CreateAsync(dto);

        // Assert
        Assert.Equal("kien-truc-n-tier-voi-unit-of-work", created.Slug);
        _mockArticleRepo.Verify(r => r.AddAsync(It.IsAny<Article>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenSlugDuplicate_ThrowsInvalidOperationException_AndNeverSaves()
    {
        // Arrange
        _mockCategoryRepo
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUserRepo
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid() });
        _mockArticleRepo
            .Setup(r => r.SlugExistsAsync("duplicate-slug", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new ArticleCreateDto
        {
            CategoryId = Guid.NewGuid(),
            Title = "Test",
            Slug = "duplicate-slug",
            MarkdownContent = "Body"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
        _mockArticleRepo.Verify(r => r.AddAsync(It.IsAny<Article>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_SoftDelete_CallsSoftDeleteAndSaveChanges()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var existing = new Article { Id = articleId, Title = "Delete Me" };
        _mockArticleRepo.Setup(r => r.GetByIdAsync(articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _service.DeleteAsync(articleId, isHardDelete: false);

        // Assert
        Assert.True(result);
        _mockArticleRepo.Verify(r => r.SoftDelete(existing), Times.Once);
        _mockArticleRepo.Verify(r => r.Remove(It.IsAny<Article>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_HardDelete_CallsRemoveAndSaveChanges()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var existing = new Article { Id = articleId, Title = "Hard Delete Me" };
        _mockArticleRepo.Setup(r => r.GetByIdIgnoreFiltersAsync(articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _service.DeleteAsync(articleId, isHardDelete: true);

        // Assert
        Assert.True(result);
        _mockArticleRepo.Verify(r => r.Remove(existing), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

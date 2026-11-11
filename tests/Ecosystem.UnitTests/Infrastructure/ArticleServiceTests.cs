using AutoMapper;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Mappings;
using Ecosystem.Application.Services;
using Ecosystem.Domain.Enums;
using Ecosystem.Infrastructure.Data;
using Ecosystem.Infrastructure.Data.Seed;
using Ecosystem.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecosystem.UnitTests.Infrastructure;

public class ArticleServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly IMapper _mapper;

    private static readonly Guid CategoryCleanArchId = Guid.Parse("11111111-aaaa-c111-1111-111111111111");
    private static readonly Guid CategoryEfCoreId = Guid.Parse("11111111-aaaa-c222-1111-111111111111");
    private static readonly Guid SeededArticleId = Guid.Parse("11111111-aaaa-c111-a001-111111111111");
    private static readonly Guid AdminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public ArticleServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        using var context = new ApplicationDbContext(_options);
        context.Database.EnsureCreated();
        DatabaseSeeder.SeedInitialDataAsync(context).GetAwaiter().GetResult();
    }

    private ArticleService CreateService(ApplicationDbContext context) =>
        new(new UnitOfWork(context), _mapper, NullLogger<ArticleService>.Instance);

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task CreateAsync_WithoutSlug_AutoGeneratesSlugAndReadingTime()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        var service = CreateService(context);

        var dto = new ArticleCreateDto
        {
            CategoryId = CategoryEfCoreId,
            Title = "Tìm hiểu sâu về LINQ và EF Core 8",
            Summary = "Tối ưu truy vấn với Include và AsNoTracking",
            MarkdownContent = string.Join(" ", Enumerable.Repeat("word", 450)), // ~2 minutes
            Status = ArticleStatus.Published,
            Tags = new List<string> { "ef-core", "linq", "ef-core" } // duplicate tag should be deduped
        };

        // Act
        var created = await service.CreateAsync(dto);

        // Assert
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("tim-hieu-sau-ve-linq-va-ef-core-8", created.Slug);
        Assert.Equal(2, created.ReadingTimeMinutes);
        Assert.Equal(AdminUserId, created.AuthorId);
        Assert.Equal(2, created.Tags.Count);

        // Eager loaded hierarchy breadcrumbs
        Assert.Equal("Entity Framework Core Advanced", created.CategoryTitle);
        Assert.Equal("Kiến Trúc Backend C#", created.AspectTitle);
        Assert.Equal("Lập Trình .NET & ASP.NET Core", created.TopicTitle);
    }

    [Fact]
    public async Task CreateAsync_DuplicateSlug_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        var service = CreateService(context);

        var dto = new ArticleCreateDto
        {
            CategoryId = CategoryCleanArchId,
            Title = "Bài viết bị trùng slug",
            Slug = "huong-dan-thiet-ke-solution-chuan-clean-architecture-trong-net-8", // Already seeded
            MarkdownContent = "# Duplicate"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_NonExistentCategory_ThrowsKeyNotFoundException()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        var service = CreateService(context);

        var dto = new ArticleCreateDto
        {
            CategoryId = Guid.NewGuid(),
            Title = "Bài viết sai chuyên mục",
            MarkdownContent = "# Invalid Category"
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task GetByIdAsync_WithIncrementView_IncrementsViewCount()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        var service = CreateService(context);

        var before = await service.GetByIdAsync(SeededArticleId, incrementView: false);
        Assert.NotNull(before);
        var initialViews = before.ViewCount;

        // Act
        var after = await service.GetByIdAsync(SeededArticleId, incrementView: true);

        // Assert
        Assert.NotNull(after);
        Assert.Equal(initialViews + 1, after.ViewCount);
        Assert.Equal("Lập Trình .NET & ASP.NET Core", after.TopicTitle);
    }

    [Fact]
    public async Task GetBySlugAsync_ValidSlug_ReturnsArticleWithBreadcrumbs()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        var service = CreateService(context);

        // Act
        var article = await service.GetBySlugAsync("huong-dan-thiet-ke-solution-chuan-clean-architecture-trong-net-8");

        // Assert
        Assert.NotNull(article);
        Assert.Equal(SeededArticleId, article.Id);
        Assert.Equal("Clean Architecture & CQRS", article.CategoryTitle);
    }

    [Fact]
    public async Task GetAllAsync_FiltersBySearchTermAndCategory()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        var service = CreateService(context);

        await service.CreateAsync(new ArticleCreateDto
        {
            CategoryId = CategoryEfCoreId,
            Title = "EF Core Performance Tips",
            Summary = "Tối ưu truy vấn database",
            MarkdownContent = "Content",
            Status = ArticleStatus.Draft
        });

        // Act: Search for "Performance"
        var searchResults = (await service.GetAllAsync(new ArticleFilterQuery { SearchTerm = "Performance" })).ToList();

        // Assert
        Assert.Single(searchResults);
        Assert.Equal("EF Core Performance Tips", searchResults[0].Title);

        // Act: Filter by Status = Published
        var published = (await service.GetAllAsync(new ArticleFilterQuery { Status = ArticleStatus.Published })).ToList();
        Assert.Single(published);
        Assert.Equal(SeededArticleId, published[0].Id);
    }

    [Fact]
    public async Task GetByCategoryIdAsync_ReturnsArticlesInCategory()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        var service = CreateService(context);

        // Act
        var results = (await service.GetByCategoryIdAsync(CategoryCleanArchId)).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(SeededArticleId, results[0].Id);
    }

    [Fact]
    public async Task UpdateAsync_ValidData_UpdatesArticleAndTimestamp()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        var service = CreateService(context);

        var updateDto = new ArticleUpdateDto
        {
            CategoryId = CategoryEfCoreId,
            Title = "Tiêu Đề Đã Cập Nhật Mới",
            Slug = "tieu-de-da-cap-nhat-moi",
            Summary = "Tóm tắt mới",
            MarkdownContent = "# Nội dung mới cập nhật",
            Status = ArticleStatus.Published,
            Tags = new List<string> { "updated" }
        };

        // Act
        var updated = await service.UpdateAsync(SeededArticleId, updateDto);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("Tiêu Đề Đã Cập Nhật Mới", updated.Title);
        Assert.Equal("tieu-de-da-cap-nhat-moi", updated.Slug);
        Assert.Equal(CategoryEfCoreId, updated.CategoryId);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeleteHidesFromQueries_HardDeleteRemovesPermanently()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        var service = CreateService(context);

        var created = await service.CreateAsync(new ArticleCreateDto
        {
            CategoryId = CategoryCleanArchId,
            Title = "Bài viết chuẩn bị xóa",
            MarkdownContent = "To be deleted"
        });

        // Act 1: Soft Delete
        var softDeleted = await service.DeleteAsync(created.Id, isHardDelete: false);
        Assert.True(softDeleted);

        // Assert 1: Normal query cannot find it
        var foundAfterSoft = await service.GetByIdAsync(created.Id);
        Assert.Null(foundAfterSoft);

        // Assert 1b: Still exists in DB when ignoring query filters
        var existsInDb = await context.Articles.IgnoreQueryFilters().AnyAsync(a => a.Id == created.Id && a.IsDeleted);
        Assert.True(existsInDb);

        // Act 2: Hard Delete
        var hardDeleted = await service.DeleteAsync(created.Id, isHardDelete: true);
        Assert.True(hardDeleted);

        // Assert 2: Completely removed
        var stillInDb = await context.Articles.IgnoreQueryFilters().AnyAsync(a => a.Id == created.Id);
        Assert.False(stillInDb);
    }
}

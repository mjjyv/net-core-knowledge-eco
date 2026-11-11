using Ecosystem.Application.Models;
using Ecosystem.Application.Services;
using Ecosystem.Domain.Exceptions;
using Ecosystem.Infrastructure.Data;
using Ecosystem.Infrastructure.Data.Seed;
using Ecosystem.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.UnitTests.Application;

public class TaxonomyServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly UnitOfWork _unitOfWork;
    private readonly TaxonomyService _service;

    public TaxonomyServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        DatabaseSeeder.SeedInitialDataAsync(context).GetAwaiter().GetResult();

        _unitOfWork = new UnitOfWork(context);
        _service = new TaxonomyService(_unitOfWork);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetAllTopicsAsync_ReturnsBaselineTopics()
    {
        // Act
        var topics = (await _service.GetAllTopicsAsync()).ToList();

        // Assert
        Assert.NotNull(topics);
        Assert.True(topics.Count >= 2);
        Assert.Contains(topics, t => t.Slug == "lap-trinh-dotnet-aspnet-core");
        Assert.Contains(topics, t => t.Slug == "kien-truc-he-thong-phan-tan");
    }

    [Fact]
    public async Task GetAllTopicsAsync_WithSearchTerm_FiltersCorrectly()
    {
        // Arrange
        var query = new TaxonomyFilterQuery
        {
            SearchTerm = "Microservices"
        };

        // Act
        var topics = (await _service.GetAllTopicsAsync(query)).ToList();

        // Assert
        Assert.Single(topics);
        Assert.Equal("kien-truc-he-thong-phan-tan", topics[0].Slug);
    }

    [Fact]
    public async Task GetAllTopicsAsync_WithSortByTitle_SortsAlphabetically()
    {
        // Arrange
        var queryAsc = new TaxonomyFilterQuery { SortBy = "title", IsDescending = false };
        var queryDesc = new TaxonomyFilterQuery { SortBy = "title", IsDescending = true };

        // Act
        var ascTopics = (await _service.GetAllTopicsAsync(queryAsc)).ToList();
        var descTopics = (await _service.GetAllTopicsAsync(queryDesc)).ToList();

        // Assert
        Assert.True(string.Compare(ascTopics.First().Title, ascTopics.Last().Title, StringComparison.Ordinal) <= 0);
        Assert.True(string.Compare(descTopics.First().Title, descTopics.Last().Title, StringComparison.Ordinal) >= 0);
    }

    [Fact]
    public async Task GetTopicByIdAsync_WhenExists_ReturnsTopic()
    {
        // Arrange
        var existingId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var topic = await _service.GetTopicByIdAsync(existingId);

        // Assert
        Assert.NotNull(topic);
        Assert.Equal(existingId, topic.Id);
        Assert.Equal("lap-trinh-dotnet-aspnet-core", topic.Slug);
    }

    [Fact]
    public async Task GetTopicByIdAsync_WhenNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.GetTopicByIdAsync(nonExistentId));
    }

    [Fact]
    public async Task CreateTopicAsync_WhenValid_AddsTopicAndGeneratesSlug()
    {
        // Arrange
        var request = new CreateTopicRequest
        {
            Title = "Điện Toán Đám Mây & DevOps",
            Description = "Kiến trúc đám mây và tự động hóa CI/CD",
            OrderIndex = 3
        };

        // Act
        var created = await _service.CreateTopicAsync(request);

        // Assert
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("dien-toan-dam-may-devops", created.Slug);
        Assert.Equal(3, created.OrderIndex);

        var retrieved = await _service.GetTopicByIdAsync(created.Id);
        Assert.Equal(created.Id, retrieved.Id);
    }

    [Fact]
    public async Task CreateTopicAsync_WhenDuplicateSlug_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateTopicRequest
        {
            Title = "Lập trình .NET & ASP.NET Core", // Same title as seeded topic
            Slug = "lap-trinh-dotnet-aspnet-core"
        };

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _service.CreateTopicAsync(request));
    }

    [Fact]
    public async Task CreateTopicAsync_WhenEmptyTitle_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateTopicRequest { Title = "   " };

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _service.CreateTopicAsync(request));
    }

    [Fact]
    public async Task UpdateTopicAsync_WhenValid_UpdatesTopicFields()
    {
        // Arrange
        var topicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var updateRequest = new UpdateTopicRequest
        {
            Id = topicId,
            Title = "Lập trình .NET 8 & ASP.NET Core Toàn Diện",
            Description = "Cập nhật mô tả mới",
            OrderIndex = 10
        };

        // Act
        var updated = await _service.UpdateTopicAsync(topicId, updateRequest);

        // Assert
        Assert.Equal("Lập trình .NET 8 & ASP.NET Core Toàn Diện", updated.Title);
        Assert.Equal(10, updated.OrderIndex);
    }

    [Fact]
    public async Task DeleteTopicAsync_SoftDeletesTopicAndCascades()
    {
        // Arrange
        var topicId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Act
        await _service.DeleteTopicAsync(topicId);

        // Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.GetTopicByIdAsync(topicId));
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.GetAspectsByTopicIdAsync(topicId));
    }

    [Fact]
    public async Task Aspect_And_Category_Crud_Workflow_Succeeds()
    {
        // Arrange
        var topicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // 1. Create Aspect
        var createAspectReq = new CreateAspectRequest
        {
            Title = "Kiểm Thử Phần Mềm Nâng Cao",
            Description = "Unit testing và Integration testing trong .NET",
            OrderIndex = 5
        };
        var aspect = await _service.CreateAspectAsync(topicId, createAspectReq);
        Assert.NotNull(aspect);
        Assert.Equal("kiem-thu-phan-mem-nang-cao", aspect.Slug);

        // 2. Create Category for that Aspect
        var createCatReq = new CreateCategoryRequest
        {
            Title = "xUnit & Moq Frameworks",
            Description = "Thực hành viết test suite",
            OrderIndex = 1
        };
        var category = await _service.CreateCategoryAsync(aspect.Id, createCatReq);
        Assert.NotNull(category);
        Assert.Equal("xunit-moq-frameworks", category.Slug);

        // 3. Verify retrieval
        var categories = (await _service.GetCategoriesByAspectIdAsync(aspect.Id)).ToList();
        Assert.Single(categories);
        Assert.Equal(category.Id, categories[0].Id);

        // 4. Delete Category
        await _service.DeleteCategoryAsync(category.Id);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.GetCategoryByIdAsync(category.Id));

        // 5. Delete Aspect
        await _service.DeleteAspectAsync(aspect.Id);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.GetAspectByIdAsync(aspect.Id));
    }
}

using Ecosystem.Application.Services;
using Ecosystem.Domain.Exceptions;
using Ecosystem.Infrastructure.Data;
using Ecosystem.Infrastructure.Data.Seed;
using Ecosystem.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.UnitTests.Application;

public class TaxonomyOverviewServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly UnitOfWork _unitOfWork;
    private readonly TaxonomyOverviewService _service;

    public TaxonomyOverviewServiceTests()
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
        _service = new TaxonomyOverviewService(_unitOfWork);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetTaxonomyHierarchyAsync_ShouldReturnBaselineTopics()
    {
        // Act
        var topics = await _service.GetTaxonomyHierarchyAsync();

        // Assert
        Assert.NotNull(topics);
        Assert.True(topics.Count >= 2, "Hệ thống phải có ít nhất 2 chủ đề lớn cốt lõi.");
        Assert.Contains(topics, t => t.Slug == "lap-trinh-dotnet-aspnet-core");
        Assert.Contains(topics, t => t.Slug == "kien-truc-he-thong-phan-tan");
    }

    [Fact]
    public async Task GetTopicBySlugAsync_WhenSlugExists_ShouldReturnTopic()
    {
        // Act
        var topic = await _service.GetTopicBySlugAsync("lap-trinh-dotnet-aspnet-core");

        // Assert
        Assert.NotNull(topic);
        Assert.Equal("Lập Trình .NET & ASP.NET Core", topic.Title);
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

using Ecosystem.Domain.Common;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;
using Ecosystem.Infrastructure.Data;
using Ecosystem.Infrastructure.Data.Seed;
using Ecosystem.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.UnitTests.Infrastructure;

public class ApplicationDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public ApplicationDbContextTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new ApplicationDbContext(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task CanConnect_DatabaseCreatedSuccessfully()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);

        // Act
        var canConnect = await context.Database.CanConnectAsync();

        // Assert
        Assert.True(canConnect);
    }

    [Fact]
    public async Task SaveChangesAsync_AutomaticallySetsCreatedAtAndUpdatedAt()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        var student = new Student
        {
            Name = "Nguyen Van A",
            Major = "Software Engineering",
            GPA = 3.8
        };

        // Act
        context.Students.Add(student);
        await context.SaveChangesAsync();

        // Assert - CreatedAt is set
        Assert.NotEqual(default, student.CreatedAt);
        Assert.True((DateTime.UtcNow - student.CreatedAt).TotalSeconds < 5);
        Assert.Null(student.UpdatedAt);

        // Act - Update entity
        student.Name = "Nguyen Van A Updated";
        await context.SaveChangesAsync();

        // Assert - UpdatedAt is now populated
        Assert.NotNull(student.UpdatedAt);
        Assert.True((DateTime.UtcNow - student.UpdatedAt.Value).TotalSeconds < 5);
    }

    [Fact]
    public async Task UniqueSlugConstraint_OnTopic_ThrowsDbUpdateException()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        var topic1 = new Topic
        {
            Title = "Topic 1",
            Slug = "duplicate-slug",
            Description = "First"
        };
        var topic2 = new Topic
        {
            Title = "Topic 2",
            Slug = "duplicate-slug",
            Description = "Second duplicate"
        };

        context.Topics.Add(topic1);
        await context.SaveChangesAsync();

        // Act & Assert
        context.Topics.Add(topic2);
        await Assert.ThrowsAsync<DbUpdateException>(async () => await context.SaveChangesAsync());
    }

    [Fact]
    public async Task GlobalQueryFilter_SoftDelete_FiltersOutDeletedEntities()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        var topic = new Topic
        {
            Title = "Active Topic",
            Slug = "active-topic",
            Description = "Description",
            IsDeleted = false
        };
        var deletedTopic = new Topic
        {
            Title = "Deleted Topic",
            Slug = "deleted-topic",
            Description = "Description",
            IsDeleted = true
        };

        context.Topics.AddRange(topic, deletedTopic);
        await context.SaveChangesAsync();

        // Act
        var activeTopics = await context.Topics.ToListAsync();
        var allTopics = await context.Topics.IgnoreQueryFilters().ToListAsync();

        // Assert
        Assert.Single(activeTopics);
        Assert.Equal("active-topic", activeTopics[0].Slug);

        Assert.Equal(2, allTopics.Count);
    }

    [Fact]
    public async Task CascadeDelete_Topic_DeletesAssociatedAspectsAndCategories()
    {
        // Arrange
        await using (var context = new ApplicationDbContext(_options))
        {
            var topic = new Topic
            {
                Title = "Cloud Computing",
                Slug = "cloud-computing",
                Description = "Cloud topics"
            };

            var aspect = new Aspect
            {
                Topic = topic,
                Title = "DevOps",
                Slug = "devops",
                Description = "DevOps aspects"
            };

            var category = new Category
            {
                Aspect = aspect,
                Title = "Kubernetes",
                Slug = "k8s",
                Description = "K8s articles"
            };

            context.Topics.Add(topic);
            context.Aspects.Add(aspect);
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }

        // Act: Delete Topic
        await using (var context = new ApplicationDbContext(_options))
        {
            var topic = await context.Topics.FirstAsync(t => t.Slug == "cloud-computing");
            context.Topics.Remove(topic);
            await context.SaveChangesAsync();
        }

        // Assert: Aspect and Category are cascade deleted
        await using (var context = new ApplicationDbContext(_options))
        {
            var aspects = await context.Aspects.ToListAsync();
            var categories = await context.Categories.ToListAsync();

            Assert.Empty(aspects);
            Assert.Empty(categories);
        }
    }

    [Fact]
    public async Task DatabaseSeeder_SeedsInitialData_Idempotent()
    {
        // Arrange
        await using (var context = new ApplicationDbContext(_options))
        {
            // Act 1: Initial Seed
            await DatabaseSeeder.SeedAsync(context);

            // Assert 1
            Assert.True(await context.Users.AnyAsync(u => u.Email == "admin@ecosystem.vn"));
            Assert.True(await context.Users.AnyAsync(u => u.Email == "user@ecosystem.vn"));
            Assert.True(await context.Topics.AnyAsync(t => t.Slug == "lap-trinh-dotnet-aspnet-core"));
            Assert.True(await context.Aspects.AnyAsync(a => a.Slug == "kien-truc-backend-csharp"));
            Assert.True(await context.Categories.AnyAsync(c => c.Slug == "clean-architecture-cqrs"));
            Assert.True(await context.Articles.AnyAsync(art => art.Slug == "huong-dan-thiet-ke-solution-chuan-clean-architecture-trong-net-8"));
            Assert.True(await context.Students.AnyAsync());

            // Act 2: Idempotent Seed (call again should not throw or duplicate)
            await DatabaseSeeder.SeedAsync(context);

            // Assert 2
            var userCount = await context.Users.CountAsync();
            var topicCount = await context.Topics.CountAsync();
            Assert.Equal(2, userCount);
            Assert.Equal(2, topicCount);
        }
    }

    [Fact]
    public async Task DiagnosticsController_GetDatabaseStatus_ReturnsOkWithCounts()
    {
        // Arrange
        await using var context = new ApplicationDbContext(_options);
        await DatabaseSeeder.SeedAsync(context);

        var controller = new DiagnosticsController(
            new Ecosystem.Application.Services.LifetimeService(),
            new Ecosystem.Application.Services.LifetimeService(),
            new Ecosystem.Application.Services.LifetimeService(),
            new Ecosystem.Application.Services.LifetimeService(),
            new Ecosystem.Application.Services.LifetimeService(),
            new Ecosystem.Application.Services.LifetimeService(),
            new Ecosystem.Infrastructure.Services.DatabaseDiagnosticsService(context));

        // Act
        var result = await controller.GetDatabaseStatusAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var response = Assert.IsType<ApiResponse<Ecosystem.Application.Interfaces.DatabaseStatusReport>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.CanConnect);
    }
}

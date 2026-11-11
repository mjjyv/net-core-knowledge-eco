using Ecosystem.Application.Models;
using Ecosystem.Application.Services;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Ecosystem.UnitTests.Application;

/// <summary>
/// Kiểm thử độc lập tầng TaxonomyService bằng xUnit + Moq (kiểm chứng Transaction &amp; Rollback của UnitOfWork).
/// </summary>
public class TaxonomyServiceMoqTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ITopicRepository> _mockTopicRepo;
    private readonly Mock<IAspectRepository> _mockAspectRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly TaxonomyService _service;

    public TaxonomyServiceMoqTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTopicRepo = new Mock<ITopicRepository>();
        _mockAspectRepo = new Mock<IAspectRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();

        _mockUnitOfWork.Setup(u => u.Topics).Returns(_mockTopicRepo.Object);
        _mockUnitOfWork.Setup(u => u.Aspects).Returns(_mockAspectRepo.Object);
        _mockUnitOfWork.Setup(u => u.Categories).Returns(_mockCategoryRepo.Object);

        _service = new TaxonomyService(_mockUnitOfWork.Object, NullLogger<TaxonomyService>.Instance);
    }

    [Fact]
    public async Task CreateTopicAsync_CallsAddAndSaveChanges()
    {
        // Arrange
        var request = new CreateTopicRequest { Title = "Moq Topic", OrderIndex = 1 };
        _mockTopicRepo.Setup(r => r.SlugExistsAsync("moq-topic", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var created = await _service.CreateTopicAsync(request);

        // Assert
        Assert.Equal("moq-topic", created.Slug);
        _mockTopicRepo.Verify(r => r.AddAsync(It.IsAny<Topic>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTopicAsync_CommitsTransaction_AfterCascadingSoftDelete()
    {
        // Arrange
        var topicId = Guid.NewGuid();
        var aspectId = Guid.NewGuid();
        var catId = Guid.NewGuid();

        var topic = new Topic { Id = topicId, Title = "Topic" };
        var aspect = new Aspect { Id = aspectId, TopicId = topicId, Title = "Aspect" };
        var cat = new Category { Id = catId, AspectId = aspectId, Title = "Category" };

        _mockTopicRepo.Setup(r => r.GetByIdAsync(topicId, It.IsAny<CancellationToken>())).ReturnsAsync(topic);
        _mockAspectRepo.Setup(r => r.GetByTopicIdAsync(topicId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { aspect });
        _mockCategoryRepo.Setup(r => r.GetByAspectIdAsync(aspectId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { cat });

        // Act
        await _service.DeleteTopicAsync(topicId);

        // Assert
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockTopicRepo.Verify(r => r.SoftDelete(topic), Times.Once);
        _mockAspectRepo.Verify(r => r.SoftDelete(aspect), Times.Once);
        _mockCategoryRepo.Verify(r => r.SoftDelete(cat), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTopicAsync_WhenCommitFails_RollsBackTransaction()
    {
        // Arrange
        var topicId = Guid.NewGuid();
        var topic = new Topic { Id = topicId, Title = "Topic" };
        _mockTopicRepo.Setup(r => r.GetByIdAsync(topicId, It.IsAny<CancellationToken>())).ReturnsAsync(topic);
        _mockAspectRepo.Setup(r => r.GetByTopicIdAsync(topicId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.DeleteTopicAsync(topicId));
        _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

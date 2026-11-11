using Ecosystem.Domain.Entities;
using Ecosystem.Infrastructure.Data;
using Ecosystem.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.UnitTests.Infrastructure;

public class UnitOfWorkTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public UnitOfWorkTests()
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
    public async Task UnitOfWork_CommitTransactionAsync_PersistsChangesAtomically()
    {
        // Arrange
        var topicId = Guid.NewGuid();
        await using (var context = new ApplicationDbContext(_options))
        using (var uow = new UnitOfWork(context))
        {
            await uow.BeginTransactionAsync();

            await uow.Topics.AddAsync(new Topic
            {
                Id = topicId,
                Title = "Transaction Topic",
                Slug = "transaction-topic"
            });

            await uow.Aspects.AddAsync(new Aspect
            {
                Id = Guid.NewGuid(),
                TopicId = topicId,
                Title = "Transaction Aspect",
                Slug = "transaction-aspect"
            });

            await uow.CommitTransactionAsync();
        }

        // Assert - Query with fresh DbContext
        await using (var verifyContext = new ApplicationDbContext(_options))
        using (var verifyUow = new UnitOfWork(verifyContext))
        {
            var topic = await verifyUow.Topics.GetByIdWithAspectsAsync(topicId);
            Assert.NotNull(topic);
            Assert.Single(topic.Aspects);
        }
    }

    [Fact]
    public async Task UnitOfWork_RollbackTransactionAsync_DiscardsUncommittedChanges()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        await using (var context = new ApplicationDbContext(_options))
        using (var uow = new UnitOfWork(context))
        {
            await uow.BeginTransactionAsync();

            await uow.Students.AddAsync(new Student("Rollback Student", "IT", 3.2)
            {
                Id = studentId
            });

            await uow.SaveChangesAsync(); // Flushed within transaction
            await uow.RollbackTransactionAsync(); // Rolled back
        }

        // Assert - Fresh DbContext must not see rolled-back student
        await using (var verifyContext = new ApplicationDbContext(_options))
        using (var verifyUow = new UnitOfWork(verifyContext))
        {
            var student = await verifyUow.Students.GetByIdAsync(studentId);
            Assert.Null(student);
        }
    }
}

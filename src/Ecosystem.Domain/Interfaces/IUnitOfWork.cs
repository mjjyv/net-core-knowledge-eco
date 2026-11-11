namespace Ecosystem.Domain.Interfaces;

/// <summary>
/// Unit of Work Pattern: Điều phối giao dịch (Transaction) và gom lệnh SaveChangesAsync() tập trung cho mọi Repository.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IArticleRepository Articles { get; }
    ITopicRepository Topics { get; }
    IAspectRepository Aspects { get; }
    ICategoryRepository Categories { get; }
    IStudentRepository Students { get; }
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

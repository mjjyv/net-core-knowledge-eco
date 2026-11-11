using Ecosystem.Domain.Interfaces;
using Ecosystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ecosystem.Infrastructure.Repositories;

/// <summary>
/// Cài đặt Unit of Work: Chia sẻ chung một thực thể ApplicationDbContext trong một HTTP Request (Scoped)
/// để quản lý thay đổi đồng bộ và đảm bảo toàn vẹn Transaction đa bảng.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IArticleRepository? _articles;
    private ITopicRepository? _topics;
    private IAspectRepository? _aspects;
    private ICategoryRepository? _categories;
    private IStudentRepository? _students;
    private IUserRepository? _users;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IArticleRepository Articles => _articles ??= new ArticleRepository(_context);
    public ITopicRepository Topics => _topics ??= new TopicRepository(_context);
    public IAspectRepository Aspects => _aspects ??= new AspectRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IStudentRepository Students => _students ??= new StudentRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            return;
        }
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

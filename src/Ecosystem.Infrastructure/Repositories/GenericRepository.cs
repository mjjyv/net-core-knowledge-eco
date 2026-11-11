using System.Linq.Expressions;
using Ecosystem.Domain.Common;
using Ecosystem.Domain.Interfaces;
using Ecosystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.Infrastructure.Repositories;

/// <summary>
/// Cài đặt Generic Repository dùng chung các thao tác truy vấn &amp; ghi cơ bản cho mọi Entity thông qua EF Core DbSet.
/// Không tự gọi SaveChangesAsync() — việc commit giao dịch do UnitOfWork điều phối.
/// </summary>
/// <typeparam name="T">Kiểu thực thể Domain kế thừa BaseEntity</typeparam>
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var local = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
        if (local != null && !ReferenceEquals(local, entity))
        {
            _context.Entry(local).CurrentValues.SetValues(entity);
            return;
        }
        _dbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        var local = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
        if (local != null && !ReferenceEquals(local, entity))
        {
            _dbSet.Remove(local);
            return;
        }
        _dbSet.Remove(entity);
    }

    public virtual void SoftDelete(T entity)
    {
        var local = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
        if (local != null)
        {
            local.IsDeleted = true;
            local.UpdatedAt = DateTime.UtcNow;
            if (_context.Entry(local).State == EntityState.Unchanged)
            {
                _context.Entry(local).State = EntityState.Modified;
            }
            return;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }
}

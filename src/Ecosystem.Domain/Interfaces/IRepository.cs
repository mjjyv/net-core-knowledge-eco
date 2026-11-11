using System.Linq.Expressions;
using Ecosystem.Domain.Common;

namespace Ecosystem.Domain.Interfaces;

/// <summary>
/// Giao diện Generic Repository chuẩn hóa các thao tác truy xuất dữ liệu cơ bản cho mọi thực thể Domain.
/// Tuân thủ Unit of Work Pattern: các thao tác ghi chỉ đánh dấu trạng thái trong Change Tracker, không tự ý SaveChanges.
/// </summary>
/// <typeparam name="T">Kiểu thực thể kế thừa BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
    void SoftDelete(T entity);
}

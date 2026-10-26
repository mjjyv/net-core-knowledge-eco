namespace Ecosystem.Domain.Common;

/// <summary>
/// Lớp thực thể cơ sở cung cấp định danh duy nhất và các trường kiểm toán (Audit Fields).
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}

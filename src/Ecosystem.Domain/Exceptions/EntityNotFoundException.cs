namespace Ecosystem.Domain.Exceptions;

/// <summary>
/// Ngoại lệ xảy ra khi không tìm thấy thực thể được yêu cầu trong hệ thống.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object Key { get; }

    public EntityNotFoundException(string entityName, object key)
        : base($"Không tìm thấy thực thể '{entityName}' với khóa: {key}")
    {
        EntityName = entityName;
        Key = key;
    }
}

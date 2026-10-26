using Ecosystem.Application.Interfaces;

namespace Ecosystem.Application.Services;

/// <summary>
/// Lớp triển khai dùng chung để minh chứng sự khác biệt của 3 vòng đời DI (Transient, Scoped, Singleton).
/// </summary>
public class LifetimeService : ITransientService, IScopedService, ISingletonService
{
    public Guid InstanceId { get; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;
}

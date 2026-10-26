namespace Ecosystem.Application.Interfaces;

/// <summary>
/// Giao diện cơ sở định danh một dịch vụ có thể theo dõi InstanceId để kiểm chứng vòng đời DI.
/// </summary>
public interface ILifetimeService
{
    Guid InstanceId { get; }
    DateTime CreatedAtUtc { get; }
}

/// <summary>
/// Vòng đời Transient: Mỗi lần yêu cầu (inject) là một instance mới hoàn toàn.
/// </summary>
public interface ITransientService : ILifetimeService
{
}

/// <summary>
/// Vòng đời Scoped: Một instance duy nhất được tạo ra và dùng chung trong cùng một HTTP Request Scope.
/// </summary>
public interface IScopedService : ILifetimeService
{
}

/// <summary>
/// Vòng đời Singleton: Một instance duy nhất được tạo ra lần đầu và dùng chung vĩnh viễn trong toàn bộ vòng đời ứng dụng.
/// </summary>
public interface ISingletonService : ILifetimeService
{
}

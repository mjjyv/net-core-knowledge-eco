using Ecosystem.Domain.Common;
using Ecosystem.Domain.Enums;

namespace Ecosystem.Domain.Entities;

/// <summary>
/// Tài khoản người dùng trong hệ sinh thái The Ecosystem.
/// </summary>
public class User : BaseEntity, IAggregateRoot
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Audience;
    public bool IsActive { get; set; } = true;

    // Navigation collections
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<ClassroomSession> HostedSessions { get; set; } = new List<ClassroomSession>();
}

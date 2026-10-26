using Ecosystem.Domain.Common;

namespace Ecosystem.Domain.Entities;

/// <summary>
/// Token làm mới phiên đăng nhập (Refresh Token).
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsRevoked && !IsExpired && !IsDeleted;
}

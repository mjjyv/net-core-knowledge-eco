using Ecosystem.Domain.Entities;

namespace Ecosystem.Application.Interfaces;

/// <summary>
/// Hợp đồng sinh JWT Access Token chuẩn RFC 7519 và Refresh Token ngẫu nhiên bảo mật cao.
/// </summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(User user, out DateTime expiresAt);
    string GenerateRefreshToken();
    string HashRefreshToken(string rawRefreshToken);
}

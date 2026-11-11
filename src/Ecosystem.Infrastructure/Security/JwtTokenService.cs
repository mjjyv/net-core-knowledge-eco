using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Ecosystem.Application.Common;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ecosystem.Infrastructure.Security;

/// <summary>
/// Dịch vụ sinh chuỗi JWT Access Token (HS256, UTC Claims) và Refresh Token bảo mật (Buổi 8).
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateAccessToken(User user, out DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey) ||
            Encoding.UTF8.GetByteCount(_jwtSettings.SecretKey) < 32)
        {
            throw new InvalidOperationException("JwtSettings:SecretKey phải có độ dài tối thiểu 32 bytes (256 bit) cho thuật toán HS256.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expiryMinutes = _jwtSettings.ExpiryInMinutes > 0 ? _jwtSettings.ExpiryInMinutes : 60;
        expiresAt = now.AddMinutes(expiryMinutes);

        var department = user.Role switch
        {
            UserRole.Host => AuthConstants.Departments.PlatformAdmin,
            UserRole.Presenter => AuthConstants.Departments.Education,
            _ => AuthConstants.Departments.Community
        };

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(AuthConstants.Claims.Department, department),
            new(ClaimTypes.DateOfBirth, "1998-01-01"),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashRefreshToken(string rawRefreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawRefreshToken);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken));
        return Convert.ToBase64String(bytes);
    }
}

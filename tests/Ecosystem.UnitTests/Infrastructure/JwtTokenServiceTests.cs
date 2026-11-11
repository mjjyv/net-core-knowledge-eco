using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecosystem.Application.Common;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;
using Ecosystem.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ecosystem.UnitTests.Infrastructure;

public class JwtTokenServiceTests
{
    private static JwtSettings CreateValidSettings() => new()
    {
        SecretKey = "UnitTest_Super_Secret_Key_At_Least_64_Bytes_For_HS256_Validation_2026!",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        ExpiryInMinutes = 60,
        RefreshTokenExpiryInDays = 7
    };

    [Fact]
    public void GenerateAccessToken_ValidUser_ProducesVerifiableJwtWithExpectedClaims()
    {
        // Arrange
        var settings = CreateValidSettings();
        var service = new JwtTokenService(Options.Create(settings));
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "jwt_tester",
            Email = "jwt@ecosystem.vn",
            PasswordHash = "SECRET_HASH_MUST_NOT_APPEAR_IN_JWT",
            Role = UserRole.Host
        };

        // Act
        var tokenString = service.GenerateAccessToken(user, out var expiresAt);

        // Assert - Check 3-part format
        var parts = tokenString.Split('.');
        Assert.Equal(3, parts.Length);
        Assert.True(expiresAt > DateTime.UtcNow.AddMinutes(55));

        // Verify signature & parse claims
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(tokenString, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        }, out var validatedToken);

        Assert.NotNull(validatedToken);
        Assert.Equal(user.Id.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("jwt_tester", principal.FindFirstValue(ClaimTypes.Name));
        Assert.Equal("jwt@ecosystem.vn", principal.FindFirstValue(ClaimTypes.Email));
        Assert.Equal("Host", principal.FindFirstValue(ClaimTypes.Role));
        Assert.DoesNotContain("SECRET_HASH_MUST_NOT_APPEAR_IN_JWT", tokenString);
    }

    [Fact]
    public void GenerateAccessToken_SecretKeyTooShort_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = new JwtTokenService(Options.Create(new JwtSettings
        {
            SecretKey = "short_key", // < 32 bytes
            Issuer = "Test",
            Audience = "Test"
        }));

        var user = new User { Id = Guid.NewGuid(), Username = "test", Email = "t@t.vn" };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => service.GenerateAccessToken(user, out _));
    }

    [Fact]
    public void GenerateAndHashRefreshToken_GeneratesUniqueTokensAndDeterministicSha256Hash()
    {
        // Arrange
        var service = new JwtTokenService(Options.Create(CreateValidSettings()));

        // Act
        var raw1 = service.GenerateRefreshToken();
        var raw2 = service.GenerateRefreshToken();
        var hash1a = service.HashRefreshToken(raw1);
        var hash1b = service.HashRefreshToken(raw1);

        // Assert
        Assert.NotEqual(raw1, raw2);
        Assert.NotEqual(raw1, hash1a);
        Assert.Equal(hash1a, hash1b); // SHA256 là deterministic hash
        Assert.Equal(44, hash1a.Length); // SHA256 Base64 chuẩn = 44 ký tự
    }
}

namespace Ecosystem.Application.Common;

/// <summary>
/// Lớp cấu hình Strongly-Typed ánh xạ từ section "JwtSettings" trong appsettings.json (Buổi 8).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryInMinutes { get; set; } = 60;
    public int RefreshTokenExpiryInDays { get; set; } = 7;
}

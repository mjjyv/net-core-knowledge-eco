namespace Ecosystem.Application.Interfaces;

/// <summary>
/// Hợp đồng băm và xác minh mật khẩu bảo mật một chiều (BCrypt).
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

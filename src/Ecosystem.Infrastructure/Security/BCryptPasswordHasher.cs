using Ecosystem.Application.Interfaces;

namespace Ecosystem.Infrastructure.Security;

/// <summary>
/// Cài đặt băm mật khẩu bảo mật sử dụng thuật toán BCrypt (workFactor = 11) chống brute-force và rainbow table.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
        {
            return false;
        }

        // Bảo vệ tránh SaltParseException nếu chuỗi hash cũ hoặc không đúng định dạng BCrypt ($2a$/$2b$/$2y$)
        if (!hashedPassword.StartsWith("$2", StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            return false;
        }
    }
}

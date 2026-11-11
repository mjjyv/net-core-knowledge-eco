using System.ComponentModel.DataAnnotations;

namespace Ecosystem.Application.DTOs;

/// <summary>
/// DTO tiếp nhận yêu cầu đăng nhập (hỗ trợ Username hoặc Email) - Buổi 8.
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Tên đăng nhập hoặc địa chỉ Email đã đăng ký.
    /// </summary>
    /// <example>admin_demo</example>
    [Required(ErrorMessage = "Tên đăng nhập hoặc Email không được để trống.")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu đăng nhập.
    /// </summary>
    /// <example>Admin@123456</example>
    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO tiếp nhận yêu cầu làm mới Access Token thông qua Refresh Token.
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// Chuỗi Refresh Token hợp lệ nhận được từ lần đăng nhập/refresh trước đó.
    /// </summary>
    [Required(ErrorMessage = "RefreshToken không được để trống.")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// DTO tiếp nhận yêu cầu đăng xuất và thu hồi Refresh Token.
/// </summary>
public class LogoutRequestDto
{
    /// <summary>
    /// Chuỗi Refresh Token cần thu hồi hiệu lực.
    /// </summary>
    [Required(ErrorMessage = "RefreshToken không được để trống.")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// DTO phản hồi kết quả xác thực thành công gồm cặp Access Token (JWT) và Refresh Token.
/// </summary>
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAt { get; set; }
    public UserResponseDto User { get; set; } = null!;
}

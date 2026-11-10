using Ecosystem.Application.DTOs;

namespace Ecosystem.Application.Interfaces;

/// <summary>
/// Giao diện dịch vụ quản lý người dùng và đăng ký tài khoản (Bài tập 1 &amp; 3 - Buổi 4).
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Đăng ký tài khoản người dùng mới với xác thực DTO và kiểm tra trùng lặp.
    /// </summary>
    Task<UserResponseDto> RegisterAsync(UserRegisterDto dto);

    /// <summary>
    /// Lấy danh sách toàn bộ người dùng đã đăng ký (an toàn, không lộ mật khẩu).
    /// </summary>
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();

    /// <summary>
    /// Lấy thông tin người dùng theo Id.
    /// </summary>
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
}

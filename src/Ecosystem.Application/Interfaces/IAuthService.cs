using Ecosystem.Application.DTOs;

namespace Ecosystem.Application.Interfaces;

/// <summary>
/// Hợp đồng dịch vụ xác thực danh tính người dùng (Register, Login, Refresh Token Rotation, Logout, Me).
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(UserRegisterDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

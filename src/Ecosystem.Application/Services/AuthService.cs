using AutoMapper;
using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;
using Ecosystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Ecosystem.Application.Services;

/// <summary>
/// Dịch vụ xác thực danh tính chuẩn Production: BCrypt Password Hashing, JWT Bearer Token,
/// Refresh Token Rotation &amp; Token Reuse Attack Detection (Buổi 8).
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IMapper mapper,
        IOptions<JwtSettings>? jwtOptions = null,
        ILogger<AuthService>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _mapper = mapper;
        _jwtSettings = jwtOptions?.Value ?? new JwtSettings();
        _logger = logger ?? NullLogger<AuthService>.Instance;
    }

    public async Task<AuthResponseDto> RegisterAsync(UserRegisterDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Xử lý đăng ký tài khoản mới cho Username '{Username}'", dto.Username);

        if (await _unitOfWork.Users.ExistsByUsernameAsync(dto.Username, cancellationToken))
        {
            throw new InvalidOperationException($"Tên đăng nhập '{dto.Username}' đã tồn tại trong hệ sinh thái.");
        }

        if (await _unitOfWork.Users.ExistsByEmailAsync(dto.Email, cancellationToken))
        {
            throw new InvalidOperationException($"Địa chỉ email '{dto.Email}' đã được đăng ký bởi tài khoản khác.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username.Trim(),
            Email = dto.Email.Trim(),
            FullName = dto.Username.Trim(),
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Role = UserRole.Audience,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            var authResponse = await IssueTokensAsync(user, dto.Age, dto.PhoneNumber, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Đăng ký và cấp JWT thành công cho User {UserId}", user.Id);
            return authResponse;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByUsernameOrEmailAsync(dto.Username, cancellationToken);

        // Chống User Enumeration Attack: không tiết lộ username hay password sai
        if (user == null || user.IsDeleted || !user.IsActive ||
            !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Đăng nhập thất bại cho tài khoản '{Username}'", dto.Username);
            return null;
        }

        var response = await IssueTokensAsync(user, age: 25, phoneNumber: null, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Người dùng {UserId} ({Username}) đã đăng nhập thành công", user.Id, user.Username);
        return response;
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var tokenHash = _jwtTokenService.HashRefreshToken(refreshToken);
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenHashWithUserAsync(tokenHash, cancellationToken);

        if (storedToken == null)
        {
            return null;
        }

        // Bảo mật: Phát hiện tái sử dụng Refresh Token đã bị hủy (Token Reuse Attack Detection)
        if (storedToken.IsRevoked)
        {
            _logger.LogCritical(
                "CẢNH BÁO BẢO MẬT: Phát hiện tái sử dụng Refresh Token đã thu hồi của User {UserId}. Thu hồi toàn bộ phiên đăng nhập!",
                storedToken.UserId);

            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(storedToken.UserId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return null;
        }

        if (storedToken.IsExpired || storedToken.IsDeleted || storedToken.User == null || !storedToken.User.IsActive || storedToken.User.IsDeleted)
        {
            return null;
        }

        // Thực hiện Refresh Token Rotation nguyên tử (thu hồi cũ + cấp cặp token mới)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            storedToken.IsRevoked = true;
            _unitOfWork.RefreshTokens.Update(storedToken);

            var response = await IssueTokensAsync(storedToken.User, age: 25, phoneNumber: null, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Xoay vòng Refresh Token thành công cho User {UserId}", storedToken.UserId);
            return response;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        var tokenHash = _jwtTokenService.HashRefreshToken(refreshToken);
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenHashWithUserAsync(tokenHash, cancellationToken);

        if (storedToken == null || storedToken.IsRevoked)
        {
            return false;
        }

        storedToken.IsRevoked = true;
        _unitOfWork.RefreshTokens.Update(storedToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Đã đăng xuất và thu hồi Refresh Token cho User {UserId}", storedToken.UserId);
        return true;
    }

    public async Task<UserResponseDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        return user == null ? null : _mapper.Map<UserResponseDto>(user);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user, int age, string? phoneNumber, CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user, out var expiresAt);
        var rawRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var hashedRefreshToken = _jwtTokenService.HashRefreshToken(rawRefreshToken);

        var refreshDays = _jwtSettings.RefreshTokenExpiryInDays > 0 ? _jwtSettings.RefreshTokenExpiryInDays : 7;

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = hashedRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);

        var userDto = _mapper.Map<UserResponseDto>(user);
        userDto.Age = age;
        userDto.PhoneNumber = phoneNumber;

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            TokenType = "Bearer",
            ExpiresAt = expiresAt,
            User = userDto
        };
    }
}

using AutoMapper;
using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Application.Mappings;
using Ecosystem.Application.Services;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;
using Ecosystem.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Ecosystem.UnitTests.Application;

/// <summary>
/// Kiểm thử độc lập AuthService với Moq: Register, Login, Refresh Token Rotation &amp; Token Reuse Attack Detection (Buổi 8).
/// </summary>
public class AuthServiceMoqTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepo;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenService> _mockJwtService;
    private readonly IMapper _mapper;
    private readonly AuthService _authService;

    public AuthServiceMoqTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockRefreshTokenRepo = new Mock<IRefreshTokenRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtService = new Mock<IJwtTokenService>();

        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.RefreshTokens).Returns(_mockRefreshTokenRepo.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _mockJwtService.Setup(j => j.GenerateRefreshToken()).Returns("raw_refresh_token_123");
        _mockJwtService.Setup(j => j.HashRefreshToken(It.IsAny<string>())).Returns<string>(s => $"hash_{s}");

        var outDate = DateTime.UtcNow.AddMinutes(60);
        _mockJwtService.Setup(j => j.GenerateAccessToken(It.IsAny<User>(), out outDate))
            .Returns("mocked.jwt.token");

        _authService = new AuthService(
            _mockUnitOfWork.Object,
            _mockPasswordHasher.Object,
            _mockJwtService.Object,
            _mapper,
            Options.Create(new JwtSettings { ExpiryInMinutes = 60, RefreshTokenExpiryInDays = 7 }),
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_ValidUser_CommitsTransactionAndReturnsTokens()
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "auth_new",
            Email = "auth_new@ecosystem.vn",
            Password = "Password123",
            ConfirmPassword = "Password123",
            Age = 24
        };

        _mockUserRepo.Setup(r => r.ExistsByUsernameAsync("auth_new", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockUserRepo.Setup(r => r.ExistsByEmailAsync("auth_new@ecosystem.vn", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockPasswordHasher.Setup(h => h.HashPassword("Password123")).Returns("$2a$11$hashed");

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        Assert.Equal("mocked.jwt.token", result.AccessToken);
        Assert.Equal("raw_refresh_token_123", result.RefreshToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal("auth_new", result.User.Username);

        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.PasswordHash == "$2a$11$hashed"), It.IsAny<CancellationToken>()), Times.Once);
        _mockRefreshTokenRepo.Verify(r => r.AddAsync(It.Is<RefreshToken>(rt => rt.Token == "hash_raw_refresh_token_123"), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin_demo",
            Email = "admin@ecosystem.vn",
            PasswordHash = "$2a$11$validhash",
            Role = UserRole.Host,
            IsActive = true
        };

        _mockUserRepo.Setup(r => r.GetByUsernameOrEmailAsync("admin_demo", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword("Admin@123456", "$2a$11$validhash")).Returns(true);

        // Act
        var result = await _authService.LoginAsync(new LoginRequestDto { Username = "admin_demo", Password = "Admin@123456" });

        // Assert
        Assert.NotNull(result);
        Assert.Equal("mocked.jwt.token", result.AccessToken);
        Assert.Equal("Host", result.User.Role);
        _mockRefreshTokenRepo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WrongPasswordOrInactiveUser_ReturnsNull()
    {
        // Arrange
        var user = new User { Username = "admin_demo", PasswordHash = "$2a$11$validhash", IsActive = true };
        _mockUserRepo.Setup(r => r.GetByUsernameOrEmailAsync("admin_demo", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword("wrong", "$2a$11$validhash")).Returns(false);

        // Act
        var result = await _authService.LoginAsync(new LoginRequestDto { Username = "admin_demo", Password = "wrong" });

        // Assert
        Assert.Null(result);
        _mockRefreshTokenRepo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidActiveToken_RotatesTokenAndCommits()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Username = "tester", Role = UserRole.Audience, IsActive = true };
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Token = "hash_old_refresh",
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            IsRevoked = false
        };

        _mockRefreshTokenRepo.Setup(r => r.GetByTokenHashWithUserAsync("hash_old_refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        var result = await _authService.RefreshTokenAsync("old_refresh");

        // Assert
        Assert.NotNull(result);
        Assert.True(storedToken.IsRevoked);
        _mockRefreshTokenRepo.Verify(r => r.Update(storedToken), Times.Once);
        _mockRefreshTokenRepo.Verify(r => r.AddAsync(It.Is<RefreshToken>(rt => rt.Token == "hash_raw_refresh_token_123"), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenAlreadyRevoked_DetectsReuseAttackAndRevokesAllTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var revokedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = new User { Id = userId, IsActive = true },
            Token = "hash_compromised",
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            IsRevoked = true // Đã bị thu hồi trước đó -> Dấu hiệu tái sử dụng!
        };

        _mockRefreshTokenRepo.Setup(r => r.GetByTokenHashWithUserAsync("hash_compromised", It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedToken);

        // Act
        var result = await _authService.RefreshTokenAsync("compromised");

        // Assert
        Assert.Null(result);
        _mockRefreshTokenRepo.Verify(r => r.RevokeAllUserTokensAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesAndSaves()
    {
        // Arrange
        var token = new RefreshToken { Token = "hash_active", IsRevoked = false };
        _mockRefreshTokenRepo.Setup(r => r.GetByTokenHashWithUserAsync("hash_active", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var success = await _authService.LogoutAsync("active");

        // Assert
        Assert.True(success);
        Assert.True(token.IsRevoked);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

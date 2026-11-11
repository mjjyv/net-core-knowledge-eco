using System.Security.Claims;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Common;
using Ecosystem.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Ecosystem.UnitTests.WebAPI;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task Register_Valid_Returns201CreatedWithAuthResponse()
    {
        // Arrange
        var authResponse = new AuthResponseDto
        {
            AccessToken = "jwt",
            RefreshToken = "rt",
            User = new UserResponseDto { Id = Guid.NewGuid(), Username = "dev" }
        };
        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<UserRegisterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Register(new UserRegisterDto());

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        var apiResp = Assert.IsType<ApiResponse<AuthResponseDto>>(created.Value);
        Assert.Equal("jwt", apiResp.Data!.AccessToken);
    }

    [Fact]
    public async Task Register_Conflict_Returns409Conflict()
    {
        // Arrange
        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<UserRegisterDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Duplicate username"));

        // Act
        var result = await _controller.Register(new UserRegisterDto());

        // Assert
        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflict.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401Unauthorized()
    {
        // Arrange
        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponseDto?)null);

        // Act
        var result = await _controller.Login(new LoginRequestDto { Username = "x", Password = "y" });

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal(401, unauthorized.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_Invalid_Returns401Unauthorized()
    {
        // Arrange
        _mockAuthService.Setup(s => s.RefreshTokenAsync("bad_rt", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponseDto?)null);

        // Act
        var result = await _controller.RefreshToken(new RefreshTokenRequestDto { RefreshToken = "bad_rt" });

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal(401, unauthorized.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidClaimsPrincipal_Returns200Ok()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserResponseDto { Id = userId, Username = "auth_user", Role = "Host" };
        _mockAuthService.Setup(s => s.GetCurrentUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
            }
        };

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<UserResponseDto>>(okResult.Value);
        Assert.Equal(userId, response.Data!.Id);
        Assert.Equal("auth_user", response.Data!.Username);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutUserIdClaim_Returns401Unauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal(401, unauthorized.StatusCode);
    }
}

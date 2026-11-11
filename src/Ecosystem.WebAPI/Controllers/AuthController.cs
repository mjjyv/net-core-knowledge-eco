using System.Security.Claims;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý xác thực người dùng bằng JWT Bearer Token và Refresh Token Rotation (Buổi 8).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Đăng ký tài khoản mới và cấp ngay cặp Access Token (JWT) + Refresh Token.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(
        [FromBody] UserRegisterDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto, cancellationToken);
            return CreatedAtAction(
                nameof(GetCurrentUser),
                null,
                ApiResponse<AuthResponseDto>.Ok(result, "Đăng ký tài khoản và cấp JWT thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Xung đột dữ liệu tài khoản",
                ex.Message));
        }
    }

    /// <summary>
    /// Đăng nhập vào hệ sinh thái bằng Username hoặc Email để nhận JWT Access Token và Refresh Token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        [FromBody] LoginRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _authService.LoginAsync(dto, cancellationToken);
        if (result == null)
        {
            return Unauthorized(CreateProblemDetails(
                StatusCodes.Status401Unauthorized,
                "Xác thực thất bại",
                "Tên đăng nhập hoặc mật khẩu không chính xác."));
        }

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Đăng nhập thành công."));
    }

    /// <summary>
    /// Làm mới JWT Access Token bằng Refresh Token (tự động Rotate &amp; phát hiện Reuse Attack).
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken(
        [FromBody] RefreshTokenRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, cancellationToken);
        if (result == null)
        {
            return Unauthorized(CreateProblemDetails(
                StatusCodes.Status401Unauthorized,
                "Refresh Token không hợp lệ",
                "Refresh Token không hợp lệ, đã hết hạn hoặc đã bị thu hồi."));
        }

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Làm mới JWT Token thành công."));
    }

    /// <summary>
    /// Đăng xuất phiên hiện tại và thu hồi hiệu lực Refresh Token (yêu cầu JWT Bearer Token).
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var revoked = await _authService.LogoutAsync(dto.RefreshToken, cancellationToken);
        return Ok(ApiResponse<object>.Ok(
            new { revoked },
            revoked ? "Đăng xuất và thu hồi Refresh Token thành công." : "Refresh Token đã được thu hồi trước đó hoặc không tồn tại."));
    }

    /// <summary>
    /// Lấy thông tin hồ sơ của người dùng đang đăng nhập từ JWT Claims (Endpoint kiểm chứng [Authorize]).
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(CreateProblemDetails(
                StatusCodes.Status401Unauthorized,
                "Token không hợp lệ",
                "Không thể trích xuất định danh người dùng từ JWT Claims."));
        }

        var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        if (user == null)
        {
            return NotFound(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Không tìm thấy người dùng",
                "Tài khoản tương ứng với Token không còn tồn tại trong hệ thống."));
        }

        return Ok(ApiResponse<UserResponseDto>.Ok(user, "Xác thực danh tính JWT thành công."));
    }

    private ProblemDetails CreateProblemDetails(int statusCode, string title, string detail)
    {
        return new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = HttpContext?.Request?.Path ?? "/api/v1/auth"
        };
    }
}

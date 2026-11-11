using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.WebAPI.Controllers;

/// <summary>
/// Quản lý RESTful API cho Tài khoản Người dùng (Bài tập 1, 2, 3 - Buổi 4).
/// Hiện thực hoá phân tách DTO, Model Validation, ngăn chặn Over-posting và xử lý lỗi xung đột 409 Conflict.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Đăng ký tài khoản người dùng mới với xác thực dữ liệu đầu vào nghiêm ngặt (DTO Validation).
    /// </summary>
    /// <param name="dto">Dữ liệu đăng ký (Username, Password, ConfirmPassword, Email, Age, PhoneNumber).</param>
    /// <returns>Thông tin tài khoản vừa tạo (được bảo vệ, không lộ thông tin nhạy cảm).</returns>
    /// <response code="201">Đăng ký người dùng thành công.</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ (vi phạm Data Annotations hoặc logic kiểm tra).</response>
    /// <response code="409">Tên đăng nhập hoặc địa chỉ Email đã tồn tại trong hệ thống.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> Register([FromBody] UserRegisterDto dto)
    {
        try
        {
            var user = await _userService.RegisterAsync(dto);
            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                ApiResponse<UserResponseDto>.Ok(user, "Đăng ký tài khoản người dùng thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Type = "https://httpstatuses.com/409",
                Title = "Xung đột dữ liệu tài khoản",
                Status = StatusCodes.Status409Conflict,
                Detail = ex.Message,
                Instance = HttpContext?.Request?.Path ?? "/api/v1/users/register"
            });
        }
    }

    /// <summary>
    /// Lấy danh sách toàn bộ người dùng đã đăng ký trong hệ sinh thái.
    /// </summary>
    /// <returns>Danh sách thông tin tóm lược của người dùng (UserResponseDto).</returns>
    /// <response code="200">Lấy danh sách người dùng thành công.</response>
    [HttpGet]
    [Authorize(Roles = AuthConstants.Roles.Host)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDto>>>> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(ApiResponse<IEnumerable<UserResponseDto>>.Ok(users, "Lấy danh sách người dùng thành công."));
    }

    /// <summary>
    /// Lấy chi tiết thông tin người dùng theo định danh GUID.
    /// </summary>
    /// <param name="id">Mã GUID của người dùng.</param>
    /// <returns>Thông tin chi tiết người dùng.</returns>
    /// <response code="200">Tìm thấy thông tin người dùng.</response>
    /// <response code="404">Không tìm thấy người dùng với Id yêu cầu.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = AuthConstants.Roles.Host)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetById([FromRoute] Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new ProblemDetails
            {
                Type = "https://httpstatuses.com/404",
                Title = "Không tìm thấy người dùng",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Không tìm thấy người dùng với Id '{id}'.",
                Instance = HttpContext?.Request?.Path ?? "/api/v1/users/register"
            });
        }

        return Ok(ApiResponse<UserResponseDto>.Ok(user, "Lấy thông tin người dùng thành công."));
    }
}
